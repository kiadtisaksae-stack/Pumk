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
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(
            new Vector3(clickPosition.x, clickPosition.y, 0));

        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector2.zero);

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.CompareTag("Employee")) continue;

            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(this);
                return;
            }
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
        Debug.Log($"เก็บ {newItem.itemName} เรียบร้อย ช่องว่างเหลือ: {maxSlots - inventory.Count}");
        RefreshInventoryUI();
        return true;
    }

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