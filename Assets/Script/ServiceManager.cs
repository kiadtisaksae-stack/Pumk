//using System.Collections.Generic;
//using UnityEngine;

///// <summary>
///// Pure utility — ไม่เก็บ state, ไม่รู้จัก Room
///// หน้าที่: build queue จาก pool + validate delivery
///// Room เป็นคนเรียกใช้และเก็บ state ทั้งหมด
///// </summary>
//public class ServiceManager : MonoBehaviour
//{
//    // ─────────────────────────────────────────────
//    //  Build Queue  (Room เรียกตอน guest check-in)
//    // ─────────────────────────────────────────────

//    public List<ItemSO> BuildQueue(List<ItemSO> pool, int serviceCount)
//    {
//        List<ItemSO> result = new List<ItemSO>();
//        List<ItemSO> working = new List<ItemSO>(pool);

//        // Luggage ก่อนเสมอ
//        for (int i = 0; i < working.Count; i++)
//        {
//            if (working[i].requiredForService == ServiceRequestType.DeliveryLuggage)
//            {
//                result.Add(working[i]);
//                working.RemoveAt(i);
//                break;
//            }
//        }

//        // สุ่มที่เหลือ
//        Shuffle(working);
//        result.AddRange(working);

//        // ตัดให้ครบพอดี
//        if (result.Count > serviceCount)
//            result.RemoveRange(serviceCount, result.Count - serviceCount);

//        Debug.Log($"<color=green>BuildQueue: {result.Count} รายการ</color>");
//        return result;
//    }

//    // ─────────────────────────────────────────────
//    //  Validate Delivery  (Room เรียกตอน actor เดินมาถึง)
//    // ─────────────────────────────────────────────

//    public bool CheckDelivery(ItemSO service, MoveHandleAI actor)
//    {
//        if (service == null) return false;

//        List<ItemSO> inventory = GetInventory(actor);
//        if (inventory == null) return false;

//        ItemSO found = inventory.Find(x => x == service);
//        if (found == null)
//        {
//            Debug.Log("<color=red>ไม่มีไอเทมที่ลูกค้าต้องการ</color>");
//            return false;
//        }

//        RemoveFromInventory(actor, found);
//        Debug.Log($"<color=cyan>ส่ง {found.itemName} สำเร็จ</color>");
//        return true;
//    }

//    // ─────────────────────────────────────────────
//    //  Helpers
//    // ─────────────────────────────────────────────

//    private List<ItemSO> GetInventory(MoveHandleAI actor)
//    {
//        if (actor is Player p) return p.inventory;
//        if (actor is Employee e) return e.inventory;
//        return null;
//    }

//    private void RemoveFromInventory(MoveHandleAI actor, ItemSO item)
//    {
//        if (actor is Player p) { p.RemoveItem(item); return; }
//        if (actor is Employee e) { e.RemoveItem(item); return; }
//    }

//    private void Shuffle(List<ItemSO> list)
//    {
//        for (int i = 0; i < list.Count; i++)
//        {
//            int r = Random.Range(i, list.Count);
//            (list[i], list[r]) = (list[r], list[i]);
//        }
//    }
//}