using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine;

public class Emplyee : MoveHandleAI
{
    [Header("Inventory")]
    public int maxSlots = 3;
    public List<ItemSO> inventory = new List<ItemSO>();
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public bool AddItem(ItemSO newItem)
    {
        // 1. เช็คว่ากระเป๋าเต็มหรือยัง?
        if (inventory.Count >= maxSlots)
        {
            Debug.Log("กระเป๋าเต็มแล้ว! (มีครบ 3 ชิ้นแล้ว)");
            return false; // เพิ่มไม่สำเร็จ
        }

        // 2. ถ้ายังไม่เต็ม ให้เพิ่มเข้า List
        inventory.Add(newItem);
        Debug.Log($"เก็บ {newItem.itemName} เรียบร้อย. ช่องว่างเหลือ: {maxSlots - inventory.Count}");

        // (Optional) เรียกอัปเดต UI ตรงนี้
        // UpdateUI(); 

        return true; // เพิ่มสำเร็จ
    }

    public void RemoveItem(ItemSO itemToRemove)
    {
        if (inventory.Contains(itemToRemove))
        {
            inventory.Remove(itemToRemove);
            Debug.Log($"ทิ้ง {itemToRemove.itemName} แล้ว");
        }
    }
}
