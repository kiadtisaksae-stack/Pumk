using UnityEngine;

public class GuestSpawner : MonoBehaviour
{
    public GameObject guestPrefab;
    [Header("Positions")]
    public Transform standPoint;    // นอกจอ (จุด Spawn)
    public Transform slot1;         // หน้าร้าน จุดที่ 1 (คนแรก)
    public Transform slot2;         // หน้าร้าน จุดที่ 2 (คนต่อแถว)

    private MoveHandleAI guestInSlot1;
    private MoveHandleAI guestInSlot2;

    private void Start()
    {
        InvokeRepeating("SpawnGuest", 1f, 5f);
    }

    [ContextMenu("Spawn Guest")]
    public void SpawnGuest()
    {
        // 1. ตรวจสอบก่อนว่าคิวเต็มไหม
        if (guestInSlot1 != null && guestInSlot2 != null)
        {
            Debug.Log("คิวหน้าร้านเต็มแล้ว!");
            return;
        }

        // 2. สร้างแขกนอกจอ
        GameObject go = Instantiate(guestPrefab, standPoint.position, Quaternion.identity);
        MoveHandleAI guest = go.GetComponent<MoveHandleAI>();

        if (guest == null)
        {
            Debug.LogError("GuestPrefab ไม่มี component MoveHandleAI!");
            Destroy(go);
            return;
        }

        guest.currentFloor = 0;
        guest.name = $"Guest_{Random.Range(1000, 9999)}";

        // 3. ส่งแขกเข้าจุดหน้าร้านที่ว่างอยู่
        if (guestInSlot1 == null)
        {
            MoveToSlot1(guest);
        }
        else
        {
            MoveToSlot2(guest);
        }
    }

    private void MoveToSlot1(MoveHandleAI guest)
    {
        guestInSlot1 = guest;
        guest.MoveTo(slot1.position, TravelState.Idle);

        guest.OnLeaveWalkInQueue = (g) => {
            guestInSlot1 = null;
            CheckAndShiftQueue();
        };
    }

    private void MoveToSlot2(MoveHandleAI guest)
    {
        guestInSlot2 = guest;
        guest.MoveTo(slot2.position, TravelState.Idle);

        guest.OnLeaveWalkInQueue = (g) => {
            guestInSlot2 = null;
        };
    }

    private void CheckAndShiftQueue()
    {
        if (guestInSlot1 == null && guestInSlot2 != null)
        {
            MoveHandleAI guestToMove = guestInSlot2;
            guestInSlot2 = null;
            MoveToSlot1(guestToMove);
        }
    }
}