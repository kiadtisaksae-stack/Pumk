using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : MoveHandleAI
{
    public GameInput gameInput;
    public Camera mainCamera;

    [Header("Inventory")]
    public int maxSlots = GameManager.BaseInventorySlots;
    public List<ItemSO> inventory = new List<ItemSO>();

    [Header("Inventory UI")]
    public List<Image> inventorySlotImages = new List<Image>();
    public Sprite emptySlotSprite;
    public Sprite lockedSlotSprite;

    [Header("Inventory Upgrade UI")]
    public Button upgradeInventoryButton;

    public bool isbusy = false;

    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
    }

    public override void Start()
    {
        base.Start();

        SetupInventorySlotComponents();
        SetupUpgradeButton();
        SyncInventoryUpgradeFromGameManager();
        RefreshInventoryUI();
        RefreshUpgradeButtonUI();
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.OnClickPosition += OnClickPosition;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStarChanged += RefreshUpgradeButtonUI;
            GameManager.Instance.OnInventoryUpgradeChanged += OnInventoryUpgraded;
        }

        RefreshInventoryUI();
        RefreshUpgradeButtonUI();
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.OnClickPosition -= OnClickPosition;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStarChanged -= RefreshUpgradeButtonUI;
            GameManager.Instance.OnInventoryUpgradeChanged -= OnInventoryUpgraded;
        }
    }

    private void OnInventoryUpgraded()
    {
        SyncInventoryUpgradeFromGameManager();
        RefreshInventoryUI();
        RefreshUpgradeButtonUI();
    }

    private void SetupUpgradeButton()
    {
        if (upgradeInventoryButton == null) return;

        upgradeInventoryButton.onClick.RemoveListener(UpgradeInventoryByStar);
        upgradeInventoryButton.onClick.AddListener(UpgradeInventoryByStar);
    }

    private void UpgradeInventoryByStar()
    {
        if (GameManager.Instance == null) return;

        bool isUpgraded = GameManager.Instance.TryUpgradeInventoryWithStar();
        if (!isUpgraded)
        {
            LevelUI levelUI = FindAnyObjectByType<LevelUI>();
            if (levelUI != null)
            {
                levelUI.Notify("Not enough stars or already max inventory");
            }

            RefreshUpgradeButtonUI();
            return;
        }

        LevelUI ui = FindAnyObjectByType<LevelUI>();
        if (ui != null)
        {
            ui.UpdateStarUI();
        }
    }

    private void SyncInventoryUpgradeFromGameManager()
    {
        int targetSlots = GameManager.BaseInventorySlots;

        if (GameManager.Instance != null)
        {
            targetSlots = GameManager.Instance.InventoryUnlockedSlots;
        }

        int maxAvailableSlots = inventorySlotImages.Count > 0
            ? Mathf.Min(inventorySlotImages.Count, GameManager.MaxInventorySlots)
            : GameManager.MaxInventorySlots;

        maxSlots = Mathf.Clamp(targetSlots, GameManager.BaseInventorySlots, maxAvailableSlots);

        if (inventory.Count > maxSlots)
        {
            inventory.RemoveRange(maxSlots, inventory.Count - maxSlots);
        }
    }

    private void SetupInventorySlotComponents()
    {
        for (int i = 0; i < inventorySlotImages.Count; i++)
        {
            if (inventorySlotImages[i] == null) continue;

            InventorySlotUI slot = inventorySlotImages[i].GetComponent<InventorySlotUI>();
            if (slot == null)
            {
                slot = inventorySlotImages[i].gameObject.AddComponent<InventorySlotUI>();
            }

            slot.Setup(this, i);
        }
    }

    private bool IsPointerOverBlockingUI(Vector2 pointerPosition)
    {
        if (EventSystem.current == null) return false;

        uiRaycastResults.Clear();
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = pointerPosition
        };

        EventSystem.current.RaycastAll(eventData, uiRaycastResults);

        for (int i = 0; i < uiRaycastResults.Count; i++)
        {
            GameObject go = uiRaycastResults[i].gameObject;
            if (go == null) continue;

            if (go.GetComponentInParent<InventorySlotUI>() != null) return true;
            if (go.GetComponentInParent<Selectable>() != null) return true;
        }

        return false;
    }

    private void OnClickPosition(Vector2 clickPosition)
    {
        if (IsPointerOverBlockingUI(clickPosition)) return;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(clickPosition.x, clickPosition.y, 0f));
        worldPoint.z = 0f;

        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector2.zero);

        IInteractable bestTarget = null;

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;

            if (hit.collider.CompareTag("Employee") || hit.collider.CompareTag("Player")) continue;

            if (hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                bestTarget = interactable;
                break;
            }
        }

        if (bestTarget != null)
        {
            bestTarget.Interact(this);
        }
    }

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
            return;
        }

        Debug.LogWarning($"ไม่พบ {itemToRemove.itemName} ในกระเป๋า");
    }

    public void DestroyItemAtSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots) return;
        if (slotIndex >= inventory.Count) return;

        ItemSO removed = inventory[slotIndex];
        inventory.RemoveAt(slotIndex);
        RefreshInventoryUI();

        if (removed != null)
        {
            Debug.Log($"Destroy item from inventory slot {slotIndex}: {removed.itemName}");
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
            Image slotImage = inventorySlotImages[i];
            if (slotImage == null) continue;

            bool isUnlocked = i < maxSlots;
            if (!isUnlocked)
            {
                slotImage.sprite = lockedSlotSprite != null ? lockedSlotSprite : emptySlotSprite;
                slotImage.enabled = slotImage.sprite != null;
                slotImage.color = Color.white;
                continue;
            }

            if (i < inventory.Count)
            {
                slotImage.sprite = inventory[i].itemIcon;
                slotImage.enabled = true;
                slotImage.color = Color.white;
            }
            else
            {
                if (emptySlotSprite != null)
                {
                    slotImage.sprite = emptySlotSprite;
                    slotImage.enabled = true;
                    slotImage.color = Color.white;
                }
                else
                {
                    slotImage.enabled = false;
                }
            }
        }
    }

    private void RefreshUpgradeButtonUI()
    {
        if (upgradeInventoryButton == null) return;

        if (GameManager.Instance == null)
        {
            upgradeInventoryButton.interactable = false;
            return;
        }

        if (!GameManager.Instance.CanUpgradeInventory)
        {
            upgradeInventoryButton.interactable = false;
            return;
        }

        int cost = GameManager.Instance.GetInventoryUpgradeCost();
        bool canPay = GameManager.Instance.Star >= cost;

        upgradeInventoryButton.interactable = canPay;
    }
}
