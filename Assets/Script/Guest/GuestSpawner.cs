using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// หน้าที่: Spawn guest + จัดคิวหน้าร้านเท่านั้น
/// service pool อยู่ใน Prefab ของแต่ละ guest type แล้ว ไม่ต้อง inject จากที่นี่
/// </summary>
public class GuestSpawner : MonoBehaviour
{
    public List<GameObject> guestPrefabs;

    public List<ObjColor> guestColor = new List<ObjColor>();

    private List<ItemSO> guestServices = new List<ItemSO>();
    private LevelManager levelManager;

    [Header("Positions")]
    public Transform standPoint;    // จุด Spawn (นอกจอ)
    public Transform slot1;         // คิวหน้าร้าน จุดที่ 1
    public Transform slot2;         // คิวหน้าร้าน จุดที่ 2

    private MoveHandleAI guestInSlot1;
    private MoveHandleAI guestInSlot2;

    private void Start()
    {
        levelManager = FindAnyObjectByType<LevelManager>();
        guestServices.AddRange(levelManager.inLevelService);
        InvokeRepeating(nameof(SpawnGuest), 1f, 5f);
    }

    [ContextMenu("Spawn Guest")]
    public void SpawnGuest()
    {
        if (guestPrefabs == null || guestPrefabs.Count == 0)
        {
            Debug.Log("แขกใน List หมดแล้ว! หยุดการ Spawn");
            CancelInvoke(nameof(SpawnGuest));
            return;
        }

        if (guestInSlot1 != null && guestInSlot2 != null) return;

        GameObject selectedPrefab = guestPrefabs[0];
        guestPrefabs.RemoveAt(0);

        GameObject go = Instantiate(selectedPrefab, standPoint.position, Quaternion.identity);
        GuestAI guest = go.GetComponent<GuestAI>();

        if (guest == null) { Destroy(go); return; }
        guest.servicePool.AddRange(guestServices);
        guest.RandomColor(guestColor);
        guest.currentFloor = 0;
        guest.name = $"Guest_Remaining_{guestPrefabs.Count}";

        if (guestInSlot1 == null) MoveToSlot1(guest);
        else MoveToSlot2(guest);
    }

    private void MoveToSlot1(MoveHandleAI guest)
    {
        guestInSlot1 = guest;
        guest.MoveTo(slot1.position, TravelState.WalkToWaitSlot);
        guest.OnLeaveWalkInQueue = (g) => {
            guestInSlot1 = null;
            CheckAndShiftQueue();
        };
    }

    private void MoveToSlot2(MoveHandleAI guest)
    {
        guestInSlot2 = guest;
        guest.MoveTo(slot2.position, TravelState.WalkToWaitSlot);
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