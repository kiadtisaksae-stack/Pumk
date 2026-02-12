using System.Collections.Generic;
using UnityEngine;

public class Employee : MoveHandleAI
{
    [Header("Inventory Settings")]
    public int maxSlots = 3;
    public List<ItemSO> inventory = new List<ItemSO>();

    /// <summary>
    /// เพิ่มไอเทมเข้ากระเป๋าของพนักงาน
    /// </summary>
    /// <param name="newItem">ไอเทมที่ต้องการเพิ่ม</param>
    /// <returns>True หากเพิ่มสำเร็จ, False หากกระเป๋าเต็ม</returns>
    public bool AddItem(ItemSO newItem)
    {
        // 1. ตรวจสอบว่ากระเป๋าเต็มหรือยัง?
        if (inventory.Count >= maxSlots)
        {
            Debug.Log($"<color=red>กระเป๋าเต็มแล้ว! (มีครบ {maxSlots} ชิ้นแล้ว)</color>");
            return false; // เพิ่มไม่สำเร็จ
        }

        // 2. ถ้ายังไม่เต็ม ให้เพิ่มเข้า List
        inventory.Add(newItem);
        Debug.Log($"เก็บ {newItem.itemName} เรียบร้อย. ช่องว่างเหลือ: {maxSlots - inventory.Count}");

        // (Optional) เรียกอัปเดต UI ตรงนี้
        // UpdateUI(); 

        return true; // เพิ่มสำเร็จ
    }

    /// <summary>
    /// ลบไอเทมออกจากกระเป๋า
    /// </summary>
    /// <param name="itemToRemove">ไอเทมที่ต้องการลบ</param>
    public void RemoveItem(ItemSO itemToRemove)
    {
        if (inventory.Contains(itemToRemove))
        {
            inventory.Remove(itemToRemove);
            Debug.Log($"นำ {itemToRemove.itemName} ออกจากกระเป๋าแล้ว");
        }
        else
        {
            Debug.LogWarning($"ไม่พบไอเทม {itemToRemove.itemName} ในกระเป๋า");
        }
    }
}