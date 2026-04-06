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
    public List<Image> inventorySlotImages = new List<Image>();
    public Sprite emptySlotSprite;
    public bool isbusy = false;

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
        gameInput.OnClickPosition -= OnClickPosition;
    }

    // ─────────────────────────────────────────────
    //  Click Handler
    //  FrankenGuest implement IInteractable อยู่แล้ว
    //  click ที่ Franken → Interact() → StartTravel ไปหา → OnTriggerEnter2D ปลุก
    // ─────────────────────────────────────────────

    private void OnClickPosition(Vector2 clickPosition)
    {
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(clickPosition.x, clickPosition.y, 0));
        worldPoint.z = 0; // ล็อคค่า Z ให้เป็น 0 เพื่อให้ Raycast ทำงานในระนาบเดียวกับ Collider 2D

        // 1. ใช้ RaycastAll เพื่อดึง Object ทั้งหมดที่อยู่ในตำแหน่งที่คลิก
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector2.zero);

        IInteractable bestTarget = null;

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;

            // ข้ามพวกพนักงานหรือตัวผู้เล่นเอง (เพื่อไม่ให้คลิกโดนตัวเองแล้วติด)
            if (hit.collider.CompareTag("Employee") || hit.collider.CompareTag("Player")) continue;

            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                // เก็บตัวที่เจอไว้ (คุณสามารถเพิ่มเงื่อนไขเลือกตัวที่อยู่ Layer บนสุดได้ที่นี่)
                bestTarget = interactable;
                break; // เจออันแรกที่คลิกได้แล้วหยุดทันที (หรือเอา break ออกถ้าต้องการเช็คตัวอื่นต่อ)
            }
        }

        // 2. ถ้าเจอเป้าหมายที่ Interact ได้ ให้ทำงาน
        if (bestTarget != null)
        {
            bestTarget.Interact(this);
        }
    }

    // ─────────────────────────────────────────────
    //  Inventory
    // ─────────────────────────────────────────────

    public bool AddItem(ItemSO newItem)
    {
        if (inventory.Count >= maxSlots)
        {
            Debug.Log($"<color=red>กระเป๋าเต็มแล้ว! ({maxSlots} ชิ้น)</color>");
            return false;
        }
        inventory.Add(newItem);

        RefreshInventoryUI();
        return true;
    }

    public void RemoveItem(ItemSO itemToRemove)
    {
        if (inventory.Contains(itemToRemove))
        {
            inventory.Remove(itemToRemove);
            RefreshInventoryUI();

        }
        else
        {
            Debug.LogWarning($"ไม่พบ {itemToRemove.itemName} ในกระเป๋า");
        }
    }

    public void ClearItem()
    {
        inventory.Clear();
        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        for (int i = 0; i < inventorySlotImages.Count; i++)
        {
            if (i < inventory.Count)
            {
                inventorySlotImages[i].sprite = inventory[i].itemIcon;
                inventorySlotImages[i].enabled = true;
                inventorySlotImages[i].color = Color.white;
            }
            else
            {
                if (emptySlotSprite != null)
                {
                    inventorySlotImages[i].sprite = emptySlotSprite;
                    inventorySlotImages[i].enabled = true;
                }
                else
                {
                    inventorySlotImages[i].enabled = false;
                }
            }
        }
    }
}