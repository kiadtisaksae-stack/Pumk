using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MoveHandleAI
{
    public GameInput gameInput;
    public Camera mainCamera;

    [Header("Inventory")]
    public int maxSlots = 3;
    public List<ItemSO> inventory = new List<ItemSO>();
    [Header("Inventory UI")]
    // ลาก Image ที่อยู่ใน UI Canvas มาใส่ที่นี่ตามจำนวน Max Slots
    public List<Image> inventorySlotImages = new List<Image>();
    public Sprite emptySlotSprite; // (Optional) รูปช่องว่าง
    public bool isbusy = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        gameInput.OnClickPosition += OnClickPosition;
        RefreshInventoryUI();
    }
    private void OnDisable()
    {
        
    }

    // Update is called once per frame
   

    private void OnClickPosition(Vector2 clickPosition)
    {
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(clickPosition.x, clickPosition.y, 0));
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            // ข้าม Employee
            if (hit.collider.CompareTag("Employee"))
                continue;

            // เจอ Interactable
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(this);
                return;
            }
        }
       
    }
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

        RefreshInventoryUI();

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
            RefreshInventoryUI();
            Debug.Log($"นำ {itemToRemove.itemName} ออกจากกระเป๋าแล้ว");
        }
        else
        {
            Debug.LogWarning($"ไม่พบไอเทม {itemToRemove.itemName} ในกระเป๋า");
        }
    }
    public void RefreshInventoryUI()
    {
        for (int i = 0; i < inventorySlotImages.Count; i++)
        {
            if (i < inventory.Count)
            {
                // มีไอเทม: แสดงรูป Icon
                inventorySlotImages[i].sprite = inventory[i].itemIcon;
                inventorySlotImages[i].enabled = true;
                inventorySlotImages[i].color = Color.white; // ให้สีชัดเจน
            }
            else
            {
                // ช่องว่าง
                if (emptySlotSprite != null)
                {
                    inventorySlotImages[i].sprite = emptySlotSprite;
                    inventorySlotImages[i].enabled = true;
                }
                else
                {
                    inventorySlotImages[i].enabled = false; // ถ้าไม่มีรูปว่างให้ปิด Image ไปเลย
                }
            }
        }
    }
}
