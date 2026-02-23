using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum TravelState
{
    Idle,
    WalkToWaitSlot,
    WalkToCallElevator,
    CallElevator,
    WaitAtSlot,
    WalkToElevator,
    InElevator,
    OutElevator,
    WalkToTarget,
    stayRoom
}

/*
   TRAVEL STATE LOGIC:
   1. Idle: ยืนนิ่งรอคำสั่ง
   2. WalkToCallElevator: กำลังเดินไปที่จุด WaitSlot หน้าลิฟต์
   3. CallElevator/WaitAtSlot: ถึงจุดรอแล้ว และทำการ Register ตัวเองกับลิฟต์
   4. WalkToElevator: ลิฟต์อนุญาตให้เข้า จึงเดินเข้าไปกึ่งกลางลิฟต์
   5. InElevator: อยู่ในลิฟต์ (Parent ถูกติดกับลิฟต์)
   6. OutElevator/WalkToTarget: ออกจากลิฟต์และเดินไปเป้าหมายสุดท้าย
*/

[RequireComponent(typeof(NavMeshAgent))]
public abstract class MoveHandleAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public int currentFloor;
    public int targetFloor;
    public InteractObjData targetIObj;
    public TravelState travelState = TravelState.Idle;
    public System.Action<MoveHandleAI> OnLeaveWalkInQueue;
    [Header("Final Destination")]
    public RoomData finalRoomData;

    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    public Vector3 currentDestination;

    protected int currentSlotIndex = -1;
    public ElevatorController assignedElevator;
    private ElevatorManager _manager;

    public Animator animator;

    [Header("Visual Settings")]
    public GameObject characterVisual;


    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // ปิดการหมุนอัตโนมัติของ Agent (เหมาะกับ 2D)
        agent.updateUpAxis = false;
    }
    public virtual void Start()
    {
        animator = GetComponentInChildren<Animator>();
        // เช็กเพื่อความชัวร์ว่าเจอไหม
        if (animator == null)
        {
            Debug.LogError($"หา Animator ไม่เจอใน {gameObject.name} หรือลูกๆ ของมัน!");
        }
    }
    /// <summary>
    /// ตรวจสอบสถานะการเดินทางในทุกเฟรม โดยเฉพาะการเช็คระยะห่างว่าเดินถึงจุดรอหน้าลิฟต์หรือยัง
    /// </summary>
    protected virtual void Update()
    {
        if (!agent.isActiveAndEnabled) return;


        // ตรวจสอบระยะเมื่อกำลังเดินไปรอลิฟต์
        if (travelState == TravelState.WalkToCallElevator || travelState == TravelState.WalkToWaitSlot)
        {
            if (agent.pathPending) return;

            float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                                          new Vector2(currentDestination.x, currentDestination.y));

            if (dist <= agent.stoppingDistance + 0.3f || agent.remainingDistance <= agent.stoppingDistance + 0.3f)
            {
                OnReachedWaitSlot(); // เมื่อถึงแล้วให้เรียกฟังก์ชันหยุด
            }
        }

        if (agent.velocity.magnitude > 0.1f)
        {
            
            if (travelState == TravelState.WalkToCallElevator || travelState == TravelState.WalkToTarget
                || travelState == TravelState.WalkToWaitSlot)
            {
                animator.SetBool("isIdle", false);
                animator.SetBool("isWalk", true);
            }
        }
        else
        {
            if (travelState == TravelState.Idle || travelState == TravelState.CallElevator ||
            travelState == TravelState.WaitAtSlot || travelState == TravelState.InElevator
            || travelState == TravelState.stayRoom)
            {
                animator.SetBool("isWalk", false);

                animator.SetBool("isIdle", true);
            }
        }
        if (agent.velocity.x > 0.1f)
        {
            // เดินไปทางขวา (บวก) -> หมุน 0 องศา
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (agent.velocity.x < -0.1f)
        {
            // เดินไปทางซ้าย (ลบ) -> หมุน 180 องศา (หันกลับหลัง)
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }


    }
    /// <summary>
    /// ทำงานเมื่อ AI เดินมาถึงจุดรอหน้าลิฟต์ จะหยุดเดินและส่งสัญญาณไปที่ลิฟต์ (RegisterGuestReady)
    /// </summary>
    private void OnReachedWaitSlot()
    {
        if (travelState != TravelState.WalkToCallElevator && travelState != TravelState.WalkToWaitSlot) return;

        // หยุดการเคลื่อนที่ทันที
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (travelState == TravelState.WalkToCallElevator)
        {
            // กรณีรอลิฟต์: ให้ทำการ Register กับลิฟต์
            travelState = TravelState.CallElevator;
            if (assignedElevator != null)
            {
                assignedElevator.RegisterGuestReady(this);
                travelState = TravelState.WaitAtSlot;
            }
        }
        else if (travelState == TravelState.WalkToWaitSlot)
        {
            // กรณีรอคิวหน้าร้าน: ให้เปลี่ยนเป็นสถานะ Idle เพื่อเล่นแอนิเมชันยืนนิ่ง
            travelState = TravelState.Idle;

            if (showDebugInfo)
                Debug.Log($"<color=yellow>{gameObject.name}: ถึงจุดรอหน้าร้านแล้ว</color>");
        }
    }

    /// <summary>
    /// จุดเริ่มต้นการเดินทางของ AI โดยรับข้อมูลเป้าหมายและตัวควบคุมลิฟต์มาประมวลผล
    /// </summary>
    public void StartTravel(InteractObjData iObj)
    {
        targetIObj = iObj;
        targetFloor = iObj.floorNumber;

        if (currentFloor == targetFloor)
        {
            MoveTo(targetIObj.ObjPosition.position, TravelState.WalkToTarget);
        }
        else
        {
            // หา Manager ใน Scene ปัจจุบัน
            if (_manager == null) _manager = Object.FindAnyObjectByType<ElevatorManager>();

            if (_manager != null)
            {
                ElevatorController best = _manager.GetBestElevator(currentFloor, targetFloor);
                if (best != null)
                {
                    AssignElevator(best);
                }
            }
            else
            {
                Debug.LogWarning("หา ElevatorManager ไม่เจอในด่านนี้!");
            }
        }
    }
    /// <summary>
    /// ลงทะเบียนลิฟต์ที่ AI จะใช้เดินทาง และขอจุดรอหน้าลิฟต์ (WaitSlot)
    /// </summary>
    public void AssignElevator(ElevatorController elevator)
    {
        assignedElevator = elevator;
        int slotIdx;
        Transform slot = assignedElevator.RequestWaitSlot(this, currentFloor, out slotIdx);

        if (slot != null)
        {
            currentSlotIndex = slotIdx;
            MoveTo(slot.position, TravelState.WalkToCallElevator);
        }
    }

    /// <summary>
    /// คำสั่งสั่งให้ NavMeshAgent เดินไปยังพิกัดที่กำหนด พร้อมเปลี่ยนสถานะการเดินทาง (TravelState)
    /// </summary>
    public void MoveTo(Vector3 pos, TravelState newState)
    {
        if (!agent.isActiveAndEnabled) return;
        Vector3 cleanPos = new Vector3(pos.x, pos.y, transform.position.z);
        currentDestination = cleanPos;
        agent.isStopped = false;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(cleanPos, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            travelState = newState;
        }
    }

    /// <summary>
    /// ทำงานเมื่อลิฟต์เปิดประตูรับ AI จะทำการคืน Slot หน้าลิฟต์และเริ่มขั้นตอนการเดินเข้าข้างใน
    /// </summary>
    public virtual void EnterElevator(Transform elevatorTransform)
    {
        // คืนค่า Slot หน้าลิฟต์เมื่อกำลังจะเข้า
        if (currentSlotIndex != -1 && assignedElevator != null)
        {
            assignedElevator.ReleaseSlot(currentFloor, currentSlotIndex);
            currentSlotIndex = -1;
        }

        travelState = TravelState.WalkToElevator;
        StartCoroutine(GoInsideElevatorRoutine(elevatorTransform));
    }

    /// <summary>
    /// ควบคุมการเดินเข้าสู่กึ่งกลางลิฟต์ เมื่อถึงที่แล้วจะทำการติด Parent ไปกับลิฟต์และปิดระบบเดินของตัวเอง
    /// </summary>
    private IEnumerator GoInsideElevatorRoutine(Transform elevatorTransform)
    {
        agent.enabled = true;
        agent.isStopped = false;
        yield return null;

        Vector3 targetPos = elevatorTransform.position;
        targetPos.z = transform.position.z;
        agent.SetDestination(targetPos);

        // รอจนกว่าจะเดินเข้าสู่ใจกลางลิฟต์
        float timeout = 2.0f;
        while (timeout > 0)
        {
            float dist = Vector2.Distance(new Vector2(transform.position.x, transform.position.y),
                                          new Vector2(targetPos.x, targetPos.y));
            if (dist < 0.2f) break;

            timeout -= Time.deltaTime;
            yield return null;
        }

        // สถานะอยู่ในลิฟต์: ปิด Agent และย้าย Parent ไปที่ลิฟต์
        travelState = TravelState.InElevator;
        agent.enabled = false;
        transform.SetParent(elevatorTransform);
        if (characterVisual != null) characterVisual.SetActive(false);
        // Lerp เพื่อจัดตำแหน่งให้กึ่งกลางเป๊ะๆ
        float t = 0;
        Vector3 startPos = transform.localPosition;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, t);
            yield return null;
        }
        transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// ทำงานเมื่อลิฟต์ถึงชั้นเป้าหมาย จะทำการออกจาก Parent ของลิฟต์ และเดินไปยังจุดหมายสุดท้ายบนชั้นนั้น
    /// </summary>
    public virtual void ExitElevator()
    {
        transform.SetParent(null); // ออกจากลิฟต์
        agent.enabled = true;
        if (characterVisual != null) characterVisual.SetActive(true);
        currentFloor = targetFloor;
        assignedElevator = null;
        currentSlotIndex = -1;
        travelState = TravelState.WalkToTarget;

        if (targetIObj != null)
        {
            MoveTo(targetIObj.ObjPosition.position, TravelState.WalkToTarget);
        }
    }
}