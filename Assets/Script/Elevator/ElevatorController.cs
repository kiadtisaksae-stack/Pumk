using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum ElevatorDirection { Idle, Up, Down }
#region FloorQueue
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
#endregion


/* FLOW สรุปของระบบลิฟต์ (SCAN Algorithm):
   1. Register: AI เดินถึงจุดรอเรียก RegisterGuestReady -> ลิฟต์บันทึกคนรอลง readyAIsOnFloor และเริ่ม Loop
   2. Decision (SCAN): DetermineNextTargetSmart คำนวณเป้าหมายตามทิศทาง (Direction Priority)
      - ขาขึ้น (UP): วิ่งขึ้นไปหาชั้นที่มีงาน รับเฉพาะคนที่ "จะขึ้น" (คนจะลงต้องรอขากลับ ยกเว้นเป็นจุดกลับรถ)
      - ขาลง (DOWN): วิ่งลงไปเก็บคนที่ "จะลง"
      - จุดกลับรถ (Turning Point): เมื่อสุดทางและไม่มีงานต่อในทิศเดิม จะรับทุกคนและเปลี่ยนทิศ
   3. Movement: MoveToFloor เคลื่อนที่ไปชั้นเป้าหมาย
   4. OpenDoors: 
      - ส่งคนออก (ExitElevator)
      - รับคนเข้า: เลือกรับเฉพาะคนที่ไปทิศเดียวกับลิฟต์ (หรือรับหมดถ้าลิฟต์ว่าง/กลับรถ)
      - **Anti-Ghost & Distance**: รอจนกว่า AI จะเดินถึงหน้าลิฟต์ (Distance < 0.8f) และรอให้ AI ย้าย Parent เสร็จจริงก่อนปิดประตู
*/

public class ElevatorController : MonoBehaviour
{
    [Header("Elevator Settings")]
    public int maxCapacity = 4;        // จำนวนคนสูงสุดที่ลิฟต์รับได้
    public float speed = 3f;           // ความเร็วการเคลื่อนที่ของลิฟต์
    public int currentFloor = 0;       // ชั้นปัจจุบันที่ลิฟต์อยู่
    public float distacneWaitslottolift = 3f; // ระยะห่างสูงสุดที่ AI จะยอมขึ้นลิฟต์ได้

    [Header("Floor Settings")]
    public Transform[] floorTargets;   // ตำแหน่งพิกัด Y ของแต่ละชั้น
    public List<GameObject> imagelift;
    public List<GameObject> floorLights; // ลาก GameObject ลูกไฟของแต่ละชั้นใส่ที่นี่ (Index 0 = ชั้น 0)
    public float lightThreshold = 0.5f;
    public FloorQueue[] floorQueues;   // ระบบจัดการคิว Slot หน้าลิฟต์แต่ละชั้น

    [Header("Debug")]
    public bool isDebugMode = true;

    // สถานะภายใน
    public List<int> destinationQueue = new List<int>(); // คิวเลขชั้นที่ลิฟต์ต้องไป
    public List<MoveHandleAI> passengers = new List<MoveHandleAI>(); // รายชื่อ AI ที่อยู่ในลิฟต์
    public Dictionary<int, List<MoveHandleAI>> readyAIsOnFloor = new Dictionary<int, List<MoveHandleAI>>(); // AI ที่มายืนรอที่จุดเรียกแล้วจริงๆ

    public ElevatorDirection currentDirection = ElevatorDirection.Idle;
    public bool isMoving = false;
    private Coroutine elevatorRoutine;

    private void Awake()
    {
        // เริ่มต้น Dictionary สำหรับเก็บคนรอในแต่ละชั้น
        for (int i = 0; i < floorTargets.Length; i++)
        {
            readyAIsOnFloor[i] = new List<MoveHandleAI>();
        }
    }
    private void Update()
    {
        UpdateFloorLights();
    }
    private void UpdateFloorLights()
    {
        if (floorLights == null || floorLights.Count == 0) return;

        for (int i = 0; i < floorTargets.Length; i++)
        {
            if (floorLights[i] == null) continue;

            // คำนวณระยะห่างระหว่างลิฟต์กับจุดเป้าหมายของแต่ละชั้นในแนวตั้ง (Y)
            float distanceToFloor = Mathf.Abs(transform.position.y - floorTargets[i].position.y);

            // ถ้าลิฟต์จอดอยู่ที่ชั้นนี้ (isMoving เป็น false) ให้ไฟติดค้าง
            if (!isMoving && currentFloor == i)
            {
                floorLights[i].SetActive(true);
            }
            // ถ้าลิฟต์กำลังวิ่งผ่าน (Distance น้อยกว่าค่าที่กำหนด) ให้ไฟสว่าง
            else if (distanceToFloor < lightThreshold)
            {
                floorLights[i].SetActive(true);
            }
            // ถ้าเลยไปแล้ว ให้ไฟดับ
            else
            {
                floorLights[i].SetActive(false);
            }
        }
    }
    /// <summary>
    /// AI จะเรียกฟังก์ชันนี้เมื่อเดินถึงระยะที่กำหนดหน้าลิฟต์ (WaitSlot)
    /// เป็นตัวจุดชนวนให้ลิฟต์เริ่มทำงาน (Process)
    /// </summary>
    public void RegisterGuestReady(MoveHandleAI character)
    {
        int floor = character.currentFloor;
        //AddDestination(floor); // มั่นใจว่าชั้นนี้อยู่ในคิว

        if (!readyAIsOnFloor.ContainsKey(floor))
            readyAIsOnFloor[floor] = new List<MoveHandleAI>();

        if (!readyAIsOnFloor[floor].Contains(character))
        {
            readyAIsOnFloor[floor].Add(character);
            if (isDebugMode)
                Debug.Log($"<color=green>Register: {character.name} พร้อมที่ชั้น {floor}</color>");
        }
        // แก้ไขตรงนี้: ถ้ายังไม่มี Loop ทำงานอยู่ หรือ Loop เก่าจบไปแล้ว ให้เริ่มใหม่
        if (elevatorRoutine == null)
        {
            elevatorRoutine = StartCoroutine(ProcessElevatorLoop());
        }
        
    }

    /// <summary>
    /// ตรรกะตัดสินใจเลือกชั้นถัดไป (Smart Logic)
    /// โดยจะไล่เก็บคนตามทิศทาง (Up/Down) ก่อนคล้ายลิฟต์จริง
    /// </summary>
    private int DetermineNextTargetSmart()
    {
        // 1. ถ้าลิฟต์ว่างและไม่มีคิว ให้จบงาน
        if (passengers.Count == 0 && !HasAnyWaitingGuests())
        {
            currentDirection = ElevatorDirection.Idle;
            return -1;
        }

        // 2. ถ้าสถานะเป็น Idle ให้เลือกทิศทางตามคนที่ใกล้ที่สุด
        if (currentDirection == ElevatorDirection.Idle)
        {
            int closestFloor = GetClosestRequestFloor();
            if (closestFloor != -1)
            {
                currentDirection = (closestFloor >= currentFloor) ? ElevatorDirection.Up : ElevatorDirection.Down;
                return closestFloor;
            }
            return -1;
        }

        // 3. SCAN Logic: วิ่งไปตามทิศทางเดิมจนสุดทาง
        if (currentDirection == ElevatorDirection.Up)
        {
            // หาชั้นที่ "สูงกว่าหรือเท่ากับ" ปัจจุบัน ที่จำเป็นต้องจอด
            // ต้องจอดถ้า: 
            // A. มีคนในลิฟต์จะลงชั้นนั้น
            // B. มีคนรอที่ชั้นนั้น และต้องการจะ "ขึ้น" (ทิศเดียวกับลิฟต์)
            // C. เป็นชั้นที่ไกลที่สุดที่มีคนรอ (แม้เขาจะลง) กรณีไม่มีงานอื่นที่สูงกว่านี้แล้ว (จุดวกกลับ)

            for (int f = currentFloor; f < floorTargets.Length; f++)
            {
                if (f == currentFloor && IsDoorOpening()) continue; // ข้ามถ้ากำลังเปิดประตูอยู่แล้ว

                bool someoneGettingOff = passengers.Any(p => p.targetFloor == f);
                bool someoneWantingUp = HasGuestGoing(f, ElevatorDirection.Up);

                if (someoneGettingOff || someoneWantingUp) return f;
            }

            // ถ้าไม่มีงานข้างบนแล้ว เช็คว่ามีใครรออยู่ข้างบนสุดไหม (จุดกลับรถ)
            // หรือถ้าไม่มีเลย ให้กลับทิศ
            if (HasAnyRequestAbove(currentFloor))
            {
                // วิ่งไปหาชั้นบนสุดที่มีคนรอ (แม้เขาจะลง)
                for (int f = currentFloor + 1; f < floorTargets.Length; f++)
                {
                    if (readyAIsOnFloor.ContainsKey(f) && readyAIsOnFloor[f].Count > 0) return f;
                }
            }

            // หมดงานขาขึ้น -> เปลี่ยนเป็นขาลง
            currentDirection = ElevatorDirection.Down;
            return DetermineNextTargetSmart(); // เรียกซ้ำด้วยทิศใหม่
        }
        else // ElevatorDirection.Down
        {
            for (int f = currentFloor; f >= 0; f--)
            {
                if (f == currentFloor && IsDoorOpening()) continue;

                bool someoneGettingOff = passengers.Any(p => p.targetFloor == f);
                bool someoneWantingDown = HasGuestGoing(f, ElevatorDirection.Down);

                if (someoneGettingOff || someoneWantingDown) return f;
            }

            if (HasAnyRequestBelow(currentFloor))
            {
                for (int f = currentFloor - 1; f >= 0; f--)
                {
                    if (readyAIsOnFloor.ContainsKey(f) && readyAIsOnFloor[f].Count > 0) return f;
                }
            }

            currentDirection = ElevatorDirection.Up;
            return DetermineNextTargetSmart();
        }
    }
    // Helpers สำหรับ Logic ใหม่
    private bool IsDoorOpening() => false; // ใช้เช็คสถานะละเอียดถ้าจำเป็น

    private bool HasGuestGoing(int floor, ElevatorDirection dir)
    {
        if (!readyAIsOnFloor.ContainsKey(floor)) return false;
        foreach (var g in readyAIsOnFloor[floor])
        {
            // คนที่รออยู่ ต้องการไปทิศเดียวกับที่เช็คหรือไม่
            if (dir == ElevatorDirection.Up && g.targetFloor > floor) return true;
            if (dir == ElevatorDirection.Down && g.targetFloor < floor) return true;
        }
        return false;
    }

    private bool HasAnyRequestAbove(int floor)
    {
        // มีคนรออยู่ชั้นที่สูงกว่านี้ไหม
        for (int f = floor + 1; f < floorTargets.Length; f++)
        {
            if (readyAIsOnFloor.ContainsKey(f) && readyAIsOnFloor[f].Count > 0) return true;
        }
        return false;
    }

    private bool HasAnyRequestBelow(int floor)
    {
        for (int f = floor - 1; f >= 0; f--)
        {
            if (readyAIsOnFloor.ContainsKey(f) && readyAIsOnFloor[f].Count > 0) return true;
        }
        return false;
    }

    private bool HasAnyWaitingGuests()
    {
        foreach (var list in readyAIsOnFloor.Values) if (list.Count > 0) return true;
        return false;
    }
    private int GetClosestRequestFloor()
    {
        int closest = -1;
        int minDist = int.MaxValue;

        // เช็คคนรอข้างนอก
        for (int f = 0; f < floorTargets.Length; f++)
        {
            if (readyAIsOnFloor.ContainsKey(f) && readyAIsOnFloor[f].Count > 0)
            {
                int dist = Mathf.Abs(currentFloor - f);
                if (dist < minDist) { minDist = dist; closest = f; }
            }
        }
        return closest;
    }

    /// <summary>
    /// Coroutine หลักที่ควบคุมวงจรการทำงานของลิฟต์ตราบใดที่ยังมีชั้นอยู่ในคิว
    /// จะสั่งให้ลิฟต์เคลื่อนที่ (Move) และ เปิดประตู (OpenDoors) วนไปเรื่อยๆ
    /// </summary>
    public IEnumerator ProcessElevatorLoop()
    {
        // กันเหนียว: ถ้าเข้ามาแล้ว isMoving เป็น true อยู่แล้วให้จบ (ป้องกันซ้อน)
        if (isMoving)
        {
            elevatorRoutine = null;
            yield break;
        }
        isMoving = true;

        // Loop ตราบใดที่มีคิว หรือ มีผู้โดยสารค้างอยู่
        while (true)
        {
            // Update Destination Queue for safety (กันเหนียว)
            if (passengers.Count > 0)
            {
                foreach (var p in passengers) AddDestination(p.targetFloor);
            }

            int targetFloor = DetermineNextTargetSmart();

            // ถ้าไม่มีเป้าหมายแล้ว ให้จบการทำงาน
            if (targetFloor == -1) break;

            if (currentFloor != targetFloor)
            {
                yield return StartCoroutine(MoveToFloor(targetFloor));
                currentFloor = targetFloor;
            }

            yield return StartCoroutine(OpenDoors());
            yield return new WaitForSeconds(0.2f);
            elevatorRoutine = null; // สำคัญมาก!
        }

        isMoving = false;
        currentDirection = ElevatorDirection.Idle;
    }

    /// <summary>
    /// ควบคุมการเคลื่อนที่ของตัวลิฟต์ในแนวตั้ง (Y-Axis) ไปยังตำแหน่งของชั้นเป้าหมาย
    /// </summary>
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

    /// <summary>
    /// จัดการเหตุการณ์เมื่อประตูลิฟต์เปิด: 
    /// 1. ปล่อยผู้โดยสารที่ต้องการลงชั้นนี้ออก 
    /// 2. ตรวจสอบระยะห่างของคนรอ และรับผู้โดยสารใหม่เข้าลิฟต์
    /// </summary>
    private IEnumerator OpenDoors()
    {
        if (imagelift != null && currentFloor < imagelift.Count)
        {
            if (imagelift[currentFloor] != null)
                imagelift[currentFloor].SetActive(false);
        }
        if (isDebugMode) Debug.Log($"<color=orange>Elevator: เปิดประตูชั้น {currentFloor}</color>");

        // 1. ส่งคนออก
        for (int i = passengers.Count - 1; i >= 0; i--)
        {
            if (passengers[i].targetFloor == currentFloor)
            {
                passengers[i].ExitElevator();
                passengers.RemoveAt(i);
                yield return new WaitForSeconds(0.3f);
            }
        }

        // 2. รับคนเข้า (เฉพาะคนที่ไปทางเดียวกัน หรือถ้าลิฟต์ว่าง/เปลี่ยนทิศก็รับหมด)
        if (readyAIsOnFloor.ContainsKey(currentFloor))
        {
            List<MoveHandleAI> waitingList = readyAIsOnFloor[currentFloor];

            // วนลูปย้อนหลังเพื่อความปลอดภัยในการ Remove
            for (int i = waitingList.Count - 1; i >= 0; i--)
            {
                if (passengers.Count >= maxCapacity) break;

                MoveHandleAI character = waitingList[i];

                // --- Logic เลือกรับคน ---
                bool shouldPickUp = false;

                // ถ้านี่คือจุดกลับรถ หรือลิฟต์ว่าง -> รับหมด
                bool isTurningPoint = !HasAnyRequestAbove(currentFloor) && currentDirection == ElevatorDirection.Up;
                if (currentDirection == ElevatorDirection.Down && !HasAnyRequestBelow(currentFloor)) isTurningPoint = true;

                if (isTurningPoint || passengers.Count == 0)
                {
                    shouldPickUp = true;
                    // อัปเดตทิศทางลิฟต์ตามคนแรกที่รับถ้าลิฟต์ว่าง
                    if (passengers.Count == 0)
                    {
                        currentDirection = (character.targetFloor > currentFloor) ? ElevatorDirection.Up : ElevatorDirection.Down;
                    }
                }
                else
                {
                    // รับเฉพาะคนไปทางเดียวกัน
                    if (currentDirection == ElevatorDirection.Up && character.targetFloor > currentFloor) shouldPickUp = true;
                    else if (currentDirection == ElevatorDirection.Down && character.targetFloor < currentFloor) shouldPickUp = true;
                }

                if (shouldPickUp)
                {
                    if (character.assignedElevator != this)
                    {
                        continue; // ถ้าไม่ใช่ลิฟต์ที่ชั้นส่งไปเรียก ห้ามรับ!
                    }
                    // รอจนกว่า AI จะเดินมาถึงหน้าลิฟต์จริงๆ (แก้บัคตัวทิพย์)
                    float waitTime = 3f;
                    yield return new WaitUntil(() => {
                        waitTime -= Time.deltaTime;
                        float dist = Vector2.Distance(character.transform.position, transform.position);
                        return dist < distacneWaitslottolift || waitTime <= 0;
                    });

                    if (isDebugMode) Debug.Log($"<color=green>Elevator: รับ {character.name} (ไป {character.targetFloor})</color>");

                    // สั่งเข้าลิฟต์และรอจนกว่าจะ Parent เสร็จ
                    character.EnterElevator(this.transform);
                    passengers.Add(character);
                    waitingList.RemoveAt(i);

                    // *** สำคัญ: รอให้ AI ย้าย Parent เสร็จจริงๆ ***
                    yield return new WaitForSeconds(0.4f);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        if (imagelift != null && currentFloor < imagelift.Count)
        {
            if (imagelift[currentFloor] != null)
                imagelift[currentFloor].SetActive(true);
        }
    }

    public void RequestElevator(MoveHandleAI character, int fromFloor, int toFloor)
    {
        // Function นี้ใช้แค่ Log ในระบบใหม่ เพราะ Logic อยู่ใน RegisterGuestReady และ DetermineNextTarget หมดแล้ว
        if (isDebugMode) Debug.Log($"<color=yellow>Request: {character.name} {fromFloor}->{toFloor}</color>");
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