using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class GuestSpawner : MonoBehaviour
{
    public List<ItemSO> servicePoolInLevel = new List<ItemSO>();
    public List<GameObject> guestPrefabs;
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


    public void AddServiceToGuest(GuestAI guest)
    {
        guest.servicePool.AddRange(servicePoolInLevel);
    }



    [ContextMenu("Spawn Guest")]
    public void SpawnGuest()
    {
        // ตรวจสอบว่าใน List ยังมีแขกเหลืออยู่ไหม
        if (guestPrefabs == null || guestPrefabs.Count == 0)
        {
            Debug.Log("แขกใน List หมดแล้ว! หยุดการ Spawn");
            CancelInvoke("SpawnGuest"); // ยกเลิกการเรียกซ้ำเพราะไม่มีแขกให้สร้างแล้ว
            return;
        }
        // ตรวจสอบคิวหน้าร้าน (ถ้าเต็มให้รอ Invoke รอบหน้าค่อยมาเช็คใหม่)
        if (guestInSlot1 != null && guestInSlot2 != null)
        {
            return;
        }

        GameObject selectedPrefab = guestPrefabs[0];
        guestPrefabs.RemoveAt(0);
        GameObject go = Instantiate(selectedPrefab, standPoint.position, Quaternion.identity);
        GuestAI guest = go.GetComponent<GuestAI>();
        if (guest != null)
        {
            AddServiceToGuest(guest);
            guest.currentFloor = 0;
            guest.name = $"Guest_Remaining_{guestPrefabs.Count}";

            // 6. จัดเข้าคิวหน้าร้าน
            if (guestInSlot1 == null)
                MoveToSlot1(guest);
            else
                MoveToSlot2(guest);
        }
        else
        {
            Destroy(go);
        }
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