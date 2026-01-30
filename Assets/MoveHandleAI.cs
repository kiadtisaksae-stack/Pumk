using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class MoveHandleAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public int currentFloor;
    public int targetFloor;
    public InteractObjData targetIObj;
    public TravelState travelState = TravelState.Idle;
    public System.Action<MoveHandleAI> OnLeaveWalkInQueue;

    [Header("Debug Settings")]
    public bool showDebugInfo = true;
    public Vector3 currentDestination;

    private int currentSlotIndex = -1;
    private ElevatorController assignedElevator;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Start()
    {
        if (showDebugInfo)
            Debug.Log($"<color=white>{gameObject.name}: เริ่มทำงานที่ชั้น {currentFloor}</color>");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // **เพิ่ม Debug:**
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: ชนกับ {collision.tag} - {collision.name}");

        // เมื่อชนจุดรอหน้าลิฟต์
        if (collision.CompareTag("WaitSlot") && travelState == TravelState.WalkingToElevator)
        {
            if (showDebugInfo)
                Debug.Log($"<color=cyan>{gameObject.name}: ถึงจุดรอแล้ว เปลี่ยนเป็น WaitingForElevator</color>");

            travelState = TravelState.WaitingForElevator;

            // แจ้งลิฟต์ว่าถึงจุดรอแล้ว
            if (assignedElevator != null)
            {
                Debug.Log($"<color=magenta>แจ้งลิฟต์: {gameObject.name} ถึงจุดรอที่ชั้น {currentFloor}</color>");

                // **เพิ่ม: แจ้งลิฟต์ให้รีเช็คคนรอ**
                assignedElevator.CheckWaitingGuestsImmediately();
            }

            if (agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }
    }


    public void StartTravel(InteractObjData iObj, ElevatorController elevator)
    {
        if (iObj == null)
        {
            Debug.LogError($"{gameObject.name}: ไม่มีเป้าหมาย!");
            return;
        }

        targetIObj = iObj;
        targetFloor = iObj.floorNumber;
        assignedElevator = elevator;

        if (showDebugInfo)
            Debug.Log($"<color=white>{gameObject.name}: เริ่มเดินทางจากชั้น {currentFloor} ไปชั้น {targetFloor}</color>");

        OnLeaveWalkInQueue?.Invoke(this);
        OnLeaveWalkInQueue = null;

        if (agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        if (currentFloor == targetFloor)
        {
            if (showDebugInfo)
                Debug.Log($"<color=yellow>{gameObject.name}: เดินไปที่เป้าหมายโดยตรง (อยู่ชั้นเดียวกัน)</color>");

            MoveTo(targetIObj.ObjPosition.position, TravelState.WalkingToTarget);
        }
        else
        {
            Transform slot = elevator.RequestWaitSlot(this, currentFloor, out currentSlotIndex);
            if (slot != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"<color=yellow>{gameObject.name}: เดินไปจุดรอที่ slot {currentSlotIndex}</color>");
                    Debug.Log($"ตำแหน่งจุดรอ: {slot.position}");
                    Debug.Log($"ตำแหน่งปัจจุบัน: {transform.position}");
                }

                MoveTo(slot.position, TravelState.WalkingToElevator);

                // **เพิ่ม Debug:**
                Debug.Log($"<color=magenta>เรียก RequestElevator: {gameObject.name} จากชั้น {currentFloor} ไปชั้น {targetFloor}</color>");

                elevator.RequestElevator(this, currentFloor, targetFloor);
            }
            else
            {
                Debug.LogError($"{gameObject.name}: ไม่มี slot ว่างที่ชั้น {currentFloor}!");
            }
        }
    }
    public void MoveTo(Vector3 pos, TravelState newState)
    {
        if (!agent.isActiveAndEnabled)
        {
            Debug.LogError($"{gameObject.name}: NavMeshAgent ไม่เปิดใช้งาน!");
            return;
        }

        // บังคับพิกัด Z ให้เท่ากับ AI
        Vector3 cleanPos = new Vector3(pos.x, pos.y, transform.position.z);
        currentDestination = cleanPos;

        agent.isStopped = false;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(cleanPos, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            travelState = newState;

            if (showDebugInfo && newState == TravelState.WalkingToElevator)
                Debug.Log($"<color=cyan>{gameObject.name}: เดินไปจุดรอลิฟต์</color>");
        }
        else
        {
            Debug.LogError($"{gameObject.name}: หาทางไป {cleanPos} ไม่เจอ! (NavMesh ไม่อยู่ในจุดนี้)");

            // Fallback: ใช้ transform.position โดยตรง
            transform.position = cleanPos;
            travelState = newState;
        }
    }

    public virtual void EnterElevator(Transform elevatorTransform)
    {
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: EnterElevator called. Current state: {travelState}");

        if (currentSlotIndex != -1 && assignedElevator != null)
        {
            assignedElevator.ReleaseSlot(currentFloor, currentSlotIndex);
            currentSlotIndex = -1;
        }

        travelState = TravelState.InElevator;
        StartCoroutine(GoInsideElevatorRoutine(elevatorTransform));
    }

    private IEnumerator GoInsideElevatorRoutine(Transform elevatorTransform)
    {
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Starting GoInsideElevatorRoutine");

        agent.enabled = true;
        yield return null;

        Vector3 targetPos = elevatorTransform.position;
        targetPos.z = transform.position.z;

        // ตรวจสอบว่าไปถึงจุดหมายได้ไหม
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(targetPos, path) && path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetDestination(targetPos);

            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Path found to elevator");
        }
        else
        {
            if (showDebugInfo)
                Debug.LogWarning($"{gameObject.name}: No path to elevator! Using direct movement");

            agent.enabled = false;
            transform.position = targetPos;
        }

        float timeout = 2.0f;
        while (Vector3.Distance(transform.position, elevatorTransform.position) > 0.15f && timeout > 0)
        {
            timeout -= Time.deltaTime;

            // ถ้าใกล้เกินไปแต่ NavMesh ไปไม่ได้
            if (Vector3.Distance(transform.position, elevatorTransform.position) < 0.5f)
            {
                if (showDebugInfo)
                    Debug.Log($"{gameObject.name}: Close enough, teleporting into elevator");
                break;
            }
            yield return null;
        }

        agent.enabled = false;
        transform.SetParent(elevatorTransform);

        // Lerp เข้าตำแหน่งกลางเป๊ะๆ
        float t = 0;
        Vector3 startPos = transform.localPosition;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            transform.localPosition = Vector3.Lerp(startPos, Vector3.zero, t);
            yield return null;
        }
        transform.localPosition = Vector3.zero;

        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: อยู่ในลิฟต์เรียบร้อย");
    }

    public virtual void ExitElevator()
    {
        transform.SetParent(null);
        agent.enabled = true;
        currentFloor = targetFloor;

        if (showDebugInfo)
            Debug.Log($"<color=green>{gameObject.name}: ออกจากลิฟต์ที่ชั้น {currentFloor}</color>");

        MoveTo(targetIObj.ObjPosition.position, TravelState.WalkingToTarget);
    }

    protected virtual void OnDrawGizmos()
    {
        if (!showDebugInfo || !Application.isPlaying) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, currentDestination);
        Gizmos.DrawWireSphere(currentDestination, 0.2f);

        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < agent.path.corners.Length - 1; i++)
                Gizmos.DrawLine(agent.path.corners[i], agent.path.corners[i + 1]);
        }
    }
}