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

    [Header("Move")]
    [SerializeField] private float baseMoveSpeed = 3.5f;

    [Header("Inventory UI")]
    public List<Image> inventorySlotImages = new List<Image>();
    public Sprite emptySlotSprite;
    public Sprite lockedSlotSprite;

    public bool isbusy = false;

    private readonly List<RaycastResult> uiRaycastResults = new List<RaycastResult>();
    private readonly List<Collider2D> interactableProbeResults = new List<Collider2D>();

    [Header("Interaction Detection")]
    [SerializeField] private LayerMask interactionLayerMask = Physics2D.DefaultRaycastLayers;
    [SerializeField] private float clickProbeRadius = 0.2f;
    [SerializeField] private bool logInteractionDebug;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;

        if (agent != null)
        {
            baseMoveSpeed = agent.speed;
        }
    }

    public override void Start()
    {
        base.Start();

        SetupInventorySlotComponents();
        ApplyGlobalUpgradesFromGameManager(GameManager.Instance, GameManager.Instance != null ? GameManager.Instance.GetPlayerSpeedMultiplier() : 1f);
        RefreshInventoryUI();
    }

    private void OnEnable()
    {
        if (gameInput != null)
        {
            gameInput.OnClickPosition += OnClickPosition;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnUpgradeDataChanged += OnUpgradeDataChanged;
        }

        RefreshInventoryUI();
    }

    private void OnDisable()
    {
        if (gameInput != null)
        {
            gameInput.OnClickPosition -= OnClickPosition;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnUpgradeDataChanged -= OnUpgradeDataChanged;
        }
    }

    private void OnUpgradeDataChanged()
    {
        if (GameManager.Instance == null) return;
        ApplyGlobalUpgradesFromGameManager(GameManager.Instance, GameManager.Instance.GetPlayerSpeedMultiplier());
    }

    public void ApplyGlobalUpgradesFromGameManager(GameManager gameManager, float playerSpeedMultiplier)
    {
        SyncInventoryUpgradeFromGameManager(gameManager);
        ApplyMoveSpeedMultiplier(playerSpeedMultiplier);
        RefreshInventoryUI();
    }

    public void ApplyMoveSpeedMultiplier(float multiplier)
    {
        if (agent == null) return;

        float finalSpeed = Mathf.Max(0.1f, baseMoveSpeed * Mathf.Max(0.01f, multiplier));
        agent.speed = finalSpeed;
    }

    private void SyncInventoryUpgradeFromGameManager(GameManager gameManager)
    {
        int targetSlots = GameManager.BaseInventorySlots;

        if (gameManager != null)
        {
            targetSlots = gameManager.InventoryUnlockedSlots;
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
        if (IsPointerOverBlockingUI(clickPosition))
        {
            if (logInteractionDebug)
            {
                Debug.Log($"[Player Interaction] Click blocked by UI at {clickPosition}.");
            }
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(clickPosition.x, clickPosition.y, 0f));
        worldPoint.z = 0f;

        IInteractable bestTarget = FindBestInteractable(worldPoint);
        if (bestTarget != null)
        {
            bestTarget.Interact(this);
            return;
        }

        if (logInteractionDebug)
        {
            Debug.Log($"[Player Interaction] No interactable found at world {worldPoint}.");
            LogHitDebug(worldPoint);
        }
    }

    private IInteractable FindBestInteractable(Vector2 worldPoint)
    {
        interactableProbeResults.Clear();
        Physics2D.OverlapPoint(worldPoint, new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = interactionLayerMask,
            useTriggers = true
        }, interactableProbeResults);

        if (interactableProbeResults.Count == 0 && clickProbeRadius > 0f)
        {
            Collider2D[] nearby = Physics2D.OverlapCircleAll(worldPoint, clickProbeRadius, interactionLayerMask);
            for (int i = 0; i < nearby.Length; i++)
            {
                Collider2D c = nearby[i];
                if (c != null && !interactableProbeResults.Contains(c))
                {
                    interactableProbeResults.Add(c);
                }
            }
        }

        IInteractable bestTarget = null;
        float bestScore = float.MinValue;

        for (int i = 0; i < interactableProbeResults.Count; i++)
        {
            Collider2D collider = interactableProbeResults[i];
            if (collider == null) continue;
            if (collider.CompareTag("Employee") || collider.CompareTag("Player")) continue;

            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable == null)
            {
                interactable = collider.GetComponentInParent<IInteractable>();
            }

            if (interactable == null) continue;

            float score = ComputeInteractionScore(collider, worldPoint);
            if (score > bestScore)
            {
                bestTarget = interactable;
                bestScore = score;
            }
        }

        return bestTarget;
    }

    private float ComputeInteractionScore(Collider2D collider, Vector2 worldPoint)
    {
        float score = 0f;

        Renderer renderer = collider.GetComponentInParent<Renderer>();
        if (renderer != null)
        {
            score += SortingLayer.GetLayerValueFromID(renderer.sortingLayerID) * 10000f;
            score += renderer.sortingOrder * 10f;
            score += renderer.transform.position.z;
        }
        else
        {
            score += collider.transform.position.z;
        }

        float centerDistance = Vector2.Distance(worldPoint, collider.bounds.center);
        score -= centerDistance;
        return score;
    }

    private void LogHitDebug(Vector2 worldPoint)
    {
        List<Collider2D> debugHits = new List<Collider2D>(Physics2D.OverlapPointAll(worldPoint, interactionLayerMask));
        if (debugHits.Count == 0 && clickProbeRadius > 0f)
        {
            debugHits.AddRange(Physics2D.OverlapCircleAll(worldPoint, clickProbeRadius, interactionLayerMask));
        }

        if (debugHits.Count == 0)
        {
            Debug.Log("[Player Interaction] Overlap probe found no collider.");
            return;
        }

        int logCount = Mathf.Min(debugHits.Count, 8);
        for (int i = 0; i < logCount; i++)
        {
            Collider2D col = debugHits[i];
            if (col == null) continue;

            bool hasInteractable = col.GetComponent<IInteractable>() != null || col.GetComponentInParent<IInteractable>() != null;
            Debug.Log($"[Player Interaction] Hit {i}: {col.name} (layer={LayerMask.LayerToName(col.gameObject.layer)}, trigger={col.isTrigger}, interactable={hasInteractable})");
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
        if (removed != null &&
            (removed.requiredForService == ServiceRequestType.DeliveryLuggage ||
             removed.requiredForService == ServiceRequestType.Laundry))
        {
            Debug.Log($"Cannot destroy protected item: {removed.itemName}");
            return;
        }

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
}
