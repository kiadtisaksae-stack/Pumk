using System.Collections.Generic;
using UnityEngine;

public class Employee : MoveHandleAI
{
    [Header("Inventory Settings")]
    public int maxSlots = 3;
    public List<ItemSO> inventory = new List<ItemSO>();

    public bool AddItem(ItemSO newItem)
    {
        if (inventory.Count >= maxSlots)
        {
            Debug.Log($"<color=red>กระเป๋าเต็มแล้ว! (มีครบ {maxSlots} ชิ้นแล้ว)</color>");
            return false;
        }

        inventory.Add(newItem);
        return true;
    }

    public void RemoveItem(ItemSO itemToRemove)
    {
        if (inventory.Contains(itemToRemove))
        {
            inventory.Remove(itemToRemove);
        }
        else
        {
            Debug.LogWarning($"ไม่พบไอเท็ม {itemToRemove.itemName} ในกระเป๋า");
        }
    }

    public int RemoveItemsByServiceType(ServiceRequestType serviceType, int maxToRemove = int.MaxValue)
    {
        if (inventory == null || inventory.Count == 0) return 0;

        int removedCount = 0;
        int limit = Mathf.Max(0, maxToRemove);
        if (limit == 0) return 0;

        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            ItemSO item = inventory[i];
            if (item == null || item.requiredForService != serviceType) continue;

            inventory.RemoveAt(i);
            removedCount++;
            if (removedCount >= limit) break;
        }

        return removedCount;
    }
}
