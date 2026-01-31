using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum ElevatorDirection { Idle, Up, Down }

[System.Serializable]
public class FloorQueue
{
    public List<Transform> waitSlots;
    private Dictionary<int, MoveHandleAI> occupiedSlots = new Dictionary<int, MoveHandleAI>();

    public Transform GetAvailableSlot(MoveHandleAI guest, out int slotIndex)
    {
        for (int i = 0; i < waitSlots.Count; i++)
        {
            if (!occupiedSlots.ContainsKey(i) || occupiedSlots[i] == null)
            {
                occupiedSlots[i] = guest;
                slotIndex = i;
                return waitSlots[i];
            }
        }
        slotIndex = -1;
        return null;
    }

    public void ReleaseSlot(int index)
    {
        if (occupiedSlots.ContainsKey(index))
            occupiedSlots.Remove(index);
    }
}

/* FLOW สรุปของระบบลิฟต์:
   1. AI เรียก RequestElevator: ลิฟต์บันทึกเลขชั้นลง destinationQueue (ยังไม่เคลื่อนที่)
   2. AI เดินถึงจุดรอ: เรียก RegisterGuestReady ลิฟต์จะบันทึกตัว AI ลง readyAIsOnFloor และเริ่ม ProcessElevatorLoop
   3. ProcessElevatorLoop: ตัดสินใจว่าจะไปชั้นไหนต่อโดยใช้ DetermineNextTargetSmart
   4. MoveToFloor: เคลื่อนย้าย Transform ไปยังชั้นเป้าหมาย
   5. OpenDoors: 
      - ส่ง AI ที่ถึงชั้นเป้าหมายออก (ExitElevator)
      - **สำคัญ** รอเช็คระยะห่าง (Distance) ของ AI ที่รออยู่หน้าชั้นนั้นๆ 
      - เมื่อ AI เข้าใกล้ลิฟต์ตามระยะที่กำหนด (0.8f) จึงจะสั่งให้ AI เดินเข้าลิฟต์ (EnterElevator)
*/

public class ElevatorController : MonoBehaviour
{
    [Header("Elevator Settings")]
    public int maxCapacity = 4;        // จำนวนคนสูงสุดที่ลิฟต์รับได้
    public float speed = 3f;           // ความเร็วการเคลื่อนที่ของลิฟต์
    public int currentFloor = 0;       // ชั้นปัจจุบันที่ลิฟต์อยู่

    [Header("Floor Settings")]
    public Transform[] floorTargets;   // ตำแหน่งพิกัด Y ของแต่ละชั้น
    public FloorQueue[] floorQueues;   // ระบบจัดการคิว Slot หน้าลิฟต์แต่ละชั้น

    [Header("Debug")]
    public bool isDebugMode = true;

    // สถานะภายใน
    public List<int> destinationQueue = new List<int>(); // คิวเลขชั้นที่ลิฟต์ต้องไป
    public List<MoveHandleAI> passengers = new List<MoveHandleAI>(); // รายชื่อ AI ที่อยู่ในลิฟต์
    public Dictionary<int, List<MoveHandleAI>> readyAIsOnFloor = new Dictionary<int, List<MoveHandleAI>>(); // AI ที่มายืนรอที่จุดเรียกแล้วจริงๆ

    public ElevatorDirection currentDirection = ElevatorDirection.Idle;
    public bool isMoving = false;

    private void Awake()
    {
        // เริ่มต้น Dictionary สำหรับเก็บคนรอในแต่ละชั้น
        for (int i = 0; i < floorTargets.Length; i++)
        {
            readyAIsOnFloor[i] = new List<MoveHandleAI>();
        }
    }

    /// <summary>
    /// AI จะเรียกฟังก์ชันนี้เมื่อเดินถึงระยะที่กำหนดหน้าลิฟต์ (WaitSlot)
    /// เป็นตัวจุดชนวนให้ลิฟต์เริ่มทำงาน (Process)
    /// </summary>
    public void RegisterGuestReady(MoveHandleAI character)
    {
        int floor = character.currentFloor;
        AddDestination(floor); // มั่นใจว่าชั้นนี้อยู่ในคิว

        if (!readyAIsOnFloor.ContainsKey(floor))
            readyAIsOnFloor[floor] = new List<MoveHandleAI>();

        if (!readyAIsOnFloor[floor].Contains(character))
        {
            readyAIsOnFloor[floor].Add(character);
            if (isDebugMode)
                Debug.Log($"<color=green>Register: {character.name} พร้อมที่ชั้น {floor}</color>");
        }

        // เริ่มการทำงานถ้าลิฟต์ว่างอยู่
        if (!isMoving)
        {
            StartCoroutine(ProcessElevatorLoop());
        }
    }

    /// <summary>
    /// ตรรกะตัดสินใจเลือกชั้นถัดไป (Smart Logic)
    /// โดยจะไล่เก็บคนตามทิศทาง (Up/Down) ก่อนคล้ายลิฟต์จริง
    /// </summary>
    private int DetermineNextTargetSmart()
    {
        bool isFull = passengers.Count >= maxCapacity;
        var dropOffFloors = passengers.Select(p => p.targetFloor).ToList();

        // ค้นหาเป้าหมายตามทิศทางปัจจุบัน (ขึ้นหรือหยุดนิ่ง)
        if (currentDirection == ElevatorDirection.Up || currentDirection == ElevatorDirection.Idle)
        {
            var upperTargets = destinationQueue.Where(f => f >= currentFloor).OrderBy(f => f).ToList();
            foreach (int f in upperTargets)
            {
                // ไปถ้ามีคนจะลง หรือมีคน Ready รออยู่ (และลิฟต์ไม่เต็ม)
                if (dropOffFloors.Contains(f) || (!isFull && readyAIsOnFloor[f].Count > 0))
                {
                    currentDirection = ElevatorDirection.Up;
                    return f;
                }
            }
        }

        // ค้นหาเป้าหมายทิศทางลง
        currentDirection = ElevatorDirection.Down;
        var lowerTargets = destinationQueue.Where(f => f < currentFloor).OrderByDescending(f => f).ToList();
        foreach (int f in lowerTargets)
        {
            if (dropOffFloors.Contains(f) || (!isFull && readyAIsOnFloor[f].Count > 0))
                return f;
        }

        return destinationQueue.Count > 0 ? destinationQueue[0] : -1;
    }

    public IEnumerator ProcessElevatorLoop()
    {
        if (isMoving || destinationQueue.Count == 0) yield break;
        isMoving = true;

        while (destinationQueue.Count > 0)
        {
            int targetFloor = DetermineNextTargetSmart();
            if (targetFloor == -1) break;

            // การเดินทางไปยังชั้นเป้าหมาย
            if (currentFloor != targetFloor)
            {
                yield return StartCoroutine(MoveToFloor(targetFloor));
                currentFloor = targetFloor;
            }

            // เปิดประตูเพื่อรับ-ส่งคน
            yield return StartCoroutine(OpenDoors());
            yield return new WaitForSeconds(0.5f);
        }

        isMoving = false;
        currentDirection = ElevatorDirection.Idle;
    }

    private IEnumerator MoveToFloor(int floor)
    {
        if (isDebugMode) Debug.Log($"<color=blue>Elevator: เคลื่อนที่ไปชั้น {floor}</color>");
        float targetY = floorTargets[floor].position.y;
        Vector3 targetPos = new Vector3(transform.position.x, targetY, transform.position.z);

        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
    }

    private IEnumerator OpenDoors()
    {
        if (isDebugMode) Debug.Log($"<color=orange>Elevator: เปิดประตูชั้น {currentFloor}</color>");

        // 1. กระบวนการส่งคนออก (เช็ค AI ในลิฟต์ที่ตั้งเป้าหมายไว้ชั้นนี้)
        for (int i = passengers.Count - 1; i >= 0; i--)
        {
            if (passengers[i].targetFloor == currentFloor)
            {
                passengers[i].ExitElevator();
                passengers.RemoveAt(i);
                yield return new WaitForSeconds(0.2f);
            }
        }

        // 2. กระบวนการรับคนเข้า (เช็ค AI ที่ Register Ready ไว้)
        if (readyAIsOnFloor.ContainsKey(currentFloor))
        {
            List<MoveHandleAI> waitingList = readyAIsOnFloor[currentFloor];

            for (int i = waitingList.Count - 1; i >= 0; i--)
            {
                MoveHandleAI character = waitingList[i];

                // ** Logic ป้องกันลิฟต์ดูด **: ลิฟต์จะรอจนกว่า AI จะเดินเข้าใกล้ตัวลิฟต์จริงๆ (0.8f)
                float checkTimeout = 5.0f; // Timeout ป้องกันลิฟต์ค้างถ้า AI เดินติด
                yield return new WaitUntil(() => {
                    float dist = Vector2.Distance(
                        new Vector2(transform.position.x, transform.position.y),
                        new Vector2(character.transform.position.x, character.transform.position.y)
                    );
                    checkTimeout -= Time.deltaTime;
                    return dist <= 0.8f || checkTimeout <= 0;
                });

                if (passengers.Count < maxCapacity)
                {
                    if (isDebugMode) Debug.Log($"<color=green>Elevator: รับ {character.name} เข้าลิฟต์</color>");

                    character.EnterElevator(this.transform);
                    passengers.Add(character);
                    waitingList.RemoveAt(i);

                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        destinationQueue.Remove(currentFloor);
        yield return new WaitForSeconds(0.5f);
    }

    /// <summary>
    /// สั่งจองชั้นจากภายนอก (พนักงาน/แขก กดเรียกจากระยะไกล)
    /// </summary>
    public void RequestElevator(MoveHandleAI character, int fromFloor, int toFloor)
    {
        if (!destinationQueue.Contains(fromFloor)) { destinationQueue.Add(fromFloor); destinationQueue.Sort(); }
        if (!destinationQueue.Contains(toFloor)) { destinationQueue.Add(toFloor); destinationQueue.Sort(); }

        if (isDebugMode)
            Debug.Log($"<color=yellow>Request: {character.name} จองชั้น {fromFloor}->{toFloor}</color>");
    }

    /// <summary>
    /// ฟังก์ชันภายในสำหรับจัดการการเพิ่มเลขชั้นลงในคิวและทำการเรียงลำดับชั้น (Sort) ให้ถูกต้อง
    /// </summary>
    private void AddDestination(int floor)
    {
        if (floor < 0 || floor >= floorTargets.Length) return;
        if (!destinationQueue.Contains(floor))
        {
            destinationQueue.Add(floor);
            destinationQueue.Sort();
        }
    }

    /// <summary>
    /// ตรวจสอบและจองตำแหน่งจุดรอ (Wait Slot) หน้าลิฟต์สำหรับ AI แต่ละตัว
    /// </summary>
    public Transform RequestWaitSlot(MoveHandleAI character, int floor, out int slotIndex)
    {
        if (floor >= 0 && floor < floorQueues.Length)
            return floorQueues[floor].GetAvailableSlot(character, out slotIndex);
        slotIndex = -1;
        return null;
    }

    /// <summary>
    /// คืนสิทธิ์การใช้งานจุดรอ (Wait Slot) เมื่อ AI ออกจากจุดนั้นหรือเข้าลิฟต์ไปแล้ว
    /// </summary>
    public void ReleaseSlot(int floor, int index)
    {
        if (floor >= 0 && floor < floorQueues.Length)
            floorQueues[floor].ReleaseSlot(index);
    }
}