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

public class ElevatorController : MonoBehaviour
{
    [Header("Elevator Settings")]
    public int maxCapacity = 4;
    public float speed = 3f;
    public int currentFloor = 0;

    public event Action<bool> OnElevatorFullStatusChanged;
    private bool _isCurrentlyFull = false;

    [Header("Floor Settings")]
    public Transform[] floorTargets;
    [Header("ชั้นแรกคือ Index 0")]
    public FloorQueue[] floorQueues;

    [Header("Debug")]
    public bool isDebugMode = true;

    // เปลี่ยนจาก private เป็น public หรือ internal
    public List<int> destinationQueue = new List<int>();
    public List<MoveHandleAI> passengers = new List<MoveHandleAI>();
    public Dictionary<int, List<MoveHandleAI>> floorWaitQueue = new Dictionary<int, List<MoveHandleAI>>();
    public ElevatorDirection currentDirection = ElevatorDirection.Idle;
    public bool isMoving = false;

    // ระบบ Stay ด้วย TriggerStay2D
    private Dictionary<MoveHandleAI, float> guestStayTimers = new Dictionary<MoveHandleAI, float>();

    private void Start()
    {
        // กำหนดค่าเริ่มต้นให้ floorWaitQueue ทุกชั้น
        for (int i = 0; i < floorTargets.Length; i++)
        {
            floorWaitQueue[i] = new List<MoveHandleAI>();
        }

        if (isDebugMode)
            Debug.Log($"<color=cyan>Elevator System Started on floor {currentFloor}</color>");
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<MoveHandleAI>(out var guest))
        {
            if (guest.travelState == TravelState.InElevator)
            {
                if (!guestStayTimers.ContainsKey(guest))
                    guestStayTimers.Add(guest, 0);
                guestStayTimers[guest] += Time.deltaTime;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<MoveHandleAI>(out var guest))
        {
            if (guestStayTimers.ContainsKey(guest))
                guestStayTimers.Remove(guest);
        }
    }

    private bool HasAnyoneGoingInDirection(int floor, ElevatorDirection direction)
    {
        if (!floorWaitQueue.ContainsKey(floor))
            return false;

        return floorWaitQueue[floor].Any(g =>
        {
            if (g.travelState != TravelState.WaitingForElevator)
                return false;

            bool guestGoingUp = g.targetFloor > floor;
            if (direction == ElevatorDirection.Up)
                return guestGoingUp;
            if (direction == ElevatorDirection.Down)
                return !guestGoingUp;

            return true;
        });
    }

    private int DetermineNextTargetSmart()
    {
        bool isFull = passengers.Count >= maxCapacity;
        var dropOffFloors = passengers.Select(p => p.targetFloor).ToList();

        if (currentDirection == ElevatorDirection.Up || currentDirection == ElevatorDirection.Idle)
        {
            var upperTargets = destinationQueue.Where(f => f >= currentFloor).OrderBy(f => f).ToList();
            foreach (int f in upperTargets)
            {
                if (dropOffFloors.Contains(f))
                {
                    currentDirection = ElevatorDirection.Up;
                    return f;
                }
                if (!isFull && HasAnyoneGoingInDirection(f, ElevatorDirection.Up))
                {
                    currentDirection = ElevatorDirection.Up;
                    return f;
                }
            }
        }

        currentDirection = ElevatorDirection.Down;
        var lowerTargets = destinationQueue.Where(f => f < currentFloor).OrderByDescending(f => f).ToList();
        foreach (int f in lowerTargets)
        {
            if (dropOffFloors.Contains(f))
                return f;
            if (!isFull && HasAnyoneGoingInDirection(f, ElevatorDirection.Down))
                return f;
        }

        return destinationQueue.Count > 0 ? destinationQueue[0] : -1;
    }

    // เปลี่ยนจาก private เป็น public
    public IEnumerator ProcessElevatorLoop()
    {
        // ตรวจสอบซ้ำว่ายังมีงานต้องทำหรือไม่
        if (destinationQueue.Count == 0)
        {
            if (isDebugMode)
                Debug.Log("<color=grey>ไม่มีงานในคิว ลิฟต์หยุด</color>");
            yield break;
        }

        isMoving = true;

        if (isDebugMode)
            Debug.Log($"<color=cyan>ลิฟต์เริ่มทำงาน! Queue: [{string.Join(", ", destinationQueue)}]</color>");

        while (destinationQueue.Count > 0)
        {
            int targetFloor = DetermineNextTargetSmart();
            if (targetFloor == -1) break;

            // ถ้าอยู่ชั้นที่ต้องการอยู่แล้ว
            if (currentFloor == targetFloor)
            {
                yield return StartCoroutine(OpenDoors());
            }
            else
            {
                yield return StartCoroutine(MoveToFloor(targetFloor));
                currentFloor = targetFloor;
                yield return StartCoroutine(OpenDoors());
            }

            yield return new WaitForSeconds(0.2f); // หยุดพักสั้นๆ
        }

        isMoving = false;
        currentDirection = ElevatorDirection.Idle;

        if (isDebugMode)
            Debug.Log("<color=grey>ลิฟต์ทำงานเสร็จสิ้น</color>");
    }

    private IEnumerator MoveToFloor(int floor)
    {
        if (isDebugMode)
            Debug.Log($"<color=blue>ลิฟต์เคลื่อนที่จากชั้น {currentFloor} ไปชั้น {floor}</color>");

        float targetY = floorTargets[floor].position.y;
        Vector3 targetPos = new Vector3(transform.position.x, targetY, transform.position.z);

        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;

        if (isDebugMode)
            Debug.Log($"<color=green>ลิฟต์ถึงชั้น {floor} แล้ว</color>");
    }

    private IEnumerator OpenDoors()
    {
        if (isDebugMode)
            Debug.Log($"<color=orange>ลิฟต์เปิดประตูที่ชั้น {currentFloor}</color>");

        // 1. คนออก
        for (int i = passengers.Count - 1; i >= 0; i--)
        {
            if (passengers[i].targetFloor == currentFloor)
            {
                if (isDebugMode)
                    Debug.Log($"<color=blue>คนออก: {passengers[i].name}</color>");

                passengers[i].ExitElevator();
                passengers.RemoveAt(i);
            }
        }

        yield return new WaitForSeconds(0.3f);

        // 2. คนเข้า
        if (floorWaitQueue.ContainsKey(currentFloor))
        {
            List<MoveHandleAI> waitingLine = floorWaitQueue[currentFloor];

            // **เพิ่ม Debug: แสดง State ของทุกคนที่รอ**
            if (isDebugMode && waitingLine.Count > 0)
            {
                Debug.Log($"<color=yellow>ตรวจสอบคนรอที่ชั้น {currentFloor}:</color>");
                foreach (var guest in waitingLine)
                {
                    Debug.Log($"  - {guest.name}: State={guest.travelState}, TargetFloor={guest.targetFloor}");
                }
            }

            float doorOpenTimer = 2.0f;

            if (waitingLine.Count > 0 && isDebugMode)
                Debug.Log($"<color=yellow>มีคนรอ {waitingLine.Count} คนที่ชั้น {currentFloor}</color>");

            while (doorOpenTimer > 0 && passengers.Count < maxCapacity && waitingLine.Count > 0)
            {
                // **แก้ไขเงื่อนไข: รับทุกคนที่รออยู่**
                List<MoveHandleAI> guestsToEnter = waitingLine
                    .Where(g => g.travelState == TravelState.WaitingForElevator)
                    .Take(maxCapacity - passengers.Count)
                    .ToList();

                // **เพิ่ม: ถ้าไม่มีคนที่ State ถูกต้อง ให้ลองดูว่าใครอยู่บ้าง**
                if (guestsToEnter.Count == 0 && waitingLine.Count > 0)
                {
                    Debug.Log($"<color=red>ไม่มีคนที่ State=WaitingForElevator! คนที่รออยู่มี State:</color>");
                    foreach (var guest in waitingLine)
                    {
                        Debug.Log($"  - {guest.name}: {guest.travelState}");

                        // ถ้ายังเป็น WalkingToElevator ให้ลองเปลี่ยน State
                        if (guest.travelState == TravelState.WalkingToElevator)
                        {
                            Debug.Log($"<color=magenta>เปลี่ยน State {guest.name} เป็น WaitingForElevator</color>");
                            guest.travelState = TravelState.WaitingForElevator;
                        }
                    }

                    // ลองใหม่หลังจากเปลี่ยน State
                    guestsToEnter = waitingLine
                        .Where(g => g.travelState == TravelState.WaitingForElevator)
                        .Take(maxCapacity - passengers.Count)
                        .ToList();
                }

                foreach (var guest in guestsToEnter)
                {
                    if (isDebugMode)
                        Debug.Log($"<color=green>รับคนเข้า: {guest.name}</color>");

                    guest.EnterElevator(this.transform);
                    passengers.Add(guest);
                    waitingLine.Remove(guest);

                    // รอสักครู่ให้คนเข้าลิฟต์
                    yield return new WaitForSeconds(0.3f);
                }

                // ถ้าไม่มีคนที่สามารถขึ้นได้ ให้ลดเวลา
                if (guestsToEnter.Count == 0)
                {
                    doorOpenTimer -= Time.deltaTime;
                    yield return null;
                }
                else
                {
                    // ถ้ามีคนขึ้น รีเซ็ตเวลาเปิดประตู
                    doorOpenTimer = 1.0f;
                }
            }

            // ถ้ายังมีคนรออยู่ ให้เพิ่มจุดหมายชั้นนี้อีกครั้ง
            if (waitingLine.Count > 0)
            {
                if (isDebugMode)
                    Debug.Log($"<color=red>ยังมีคนรอ {waitingLine.Count} คนที่ชั้น {currentFloor}</color>");

                if (!destinationQueue.Contains(currentFloor))
                    AddDestination(currentFloor);
            }
        }

        destinationQueue.Remove(currentFloor);
        yield return new WaitForSeconds(0.3f);
    }

    private void Update()
    {
        CheckCapacityStatus();
    }

    private void CheckCapacityStatus()
    {
        bool nowFull = passengers.Count >= maxCapacity;
        if (nowFull != _isCurrentlyFull)
        {
            _isCurrentlyFull = nowFull;
            OnElevatorFullStatusChanged?.Invoke(_isCurrentlyFull);
        }
    }

    public void RequestElevator(MoveHandleAI character, int fromFloor, int toFloor)
    {
        if (!floorWaitQueue.ContainsKey(fromFloor))
            floorWaitQueue[fromFloor] = new List<MoveHandleAI>();

        if (!floorWaitQueue[fromFloor].Contains(character))
            floorWaitQueue[fromFloor].Add(character);

        AddDestination(fromFloor);
        AddDestination(toFloor);

        // ถ้าลิฟต์ว่างอยู่ชั้นเดียวกับที่เรียก ให้เริ่มทำงานทันที
        if (currentDirection == ElevatorDirection.Idle && !isMoving && currentFloor == fromFloor)
        {
            if (isDebugMode)
                Debug.Log($"<color=yellow>ลิฟต์ว่างอยู่ชั้น {fromFloor} เริ่มทำงานทันที!</color>");

            if (!isMoving)
                StartCoroutine(ProcessElevatorLoop());
        }
    }

    private void AddDestination(int floor)
    {
        if (floor < 0 || floor >= floorTargets.Length)
        {
            if (isDebugMode)
                Debug.LogError($"<color=red>ชั้น {floor} ไม่มีอยู่!</color>");
            return;
        }

        if (!destinationQueue.Contains(floor))
        {
            destinationQueue.Add(floor);
            destinationQueue.Sort();

            if (isDebugMode)
                Debug.Log($"เพิ่มจุดหมาย: {floor}, Queue: [{string.Join(", ", destinationQueue)}]");
        }

        // ถ้ายังไม่เคลื่อนที่ก็เริ่มได้เลย
        if (!isMoving && !IsInvoking("DelayedStart"))
        {
            Invoke("DelayedStart", 0.1f);
        }
    }

    private void DelayedStart()
    {
        if (!isMoving && destinationQueue.Count > 0)
        {
            if (isDebugMode)
                Debug.Log($"<color=green>เริ่มทำงานลิฟต์!</color>");

            StartCoroutine(ProcessElevatorLoop());
        }
    }

    public void ReleaseSlot(int floor, int index)
    {
        if (floor >= 0 && floor < floorQueues.Length)
            floorQueues[floor].ReleaseSlot(index);
    }

    public Transform RequestWaitSlot(MoveHandleAI character, int floor, out int slotIndex)
    {
        if (floor >= 0 && floor < floorQueues.Length)
            return floorQueues[floor].GetAvailableSlot(character, out slotIndex);

        slotIndex = -1;
        return null;
    }

    public bool IsIdleAndOnFloor(int floor)
    {
        return currentDirection == ElevatorDirection.Idle &&
               !isMoving &&
               currentFloor == floor;
    }
    public void CheckWaitingGuestsImmediately()
    {
        if (isDebugMode)
            Debug.Log($"<color=magenta>รีเช็คคนรอที่ชั้น {currentFloor}...</color>");

        if (floorWaitQueue.ContainsKey(currentFloor) && floorWaitQueue[currentFloor].Count > 0)
        {
            if (isDebugMode)
                Debug.Log($"<color=cyan>พบคนรอ {floorWaitQueue[currentFloor].Count} คนที่ชั้น {currentFloor} ขณะลิฟต์อยู่ชั้นเดียวกัน</color>");

            // ถ้าลิฟต์อยู่ชั้นนี้และกำลังเปิดประตูอยู่ ให้เพิ่มจุดหมายใหม่
            if (!destinationQueue.Contains(currentFloor))
                AddDestination(currentFloor);
        }
    }
}