using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum Guestphase { CheckingIn, InRoom, RequestingService, CheckingOut }
public enum GuestRoomRequirementMode { Any, SpecificType }

/// <summary>
/// Base class — data + logic เท่านั้น ไม่รู้จัก UI
/// UI ทั้งหมดจัดการโดย GuestUIController (หาจาก children อัตโนมัติ)
/// </summary>
public class GuestAI : MoveHandleAI
{
    public Guestphase guestPhase;
    [SerializeField] public ObjColor guestColor;
    public AudioClip guestSound;


    [Header("Material References")]


    [SerializeField] private Material orangeMat;
    [SerializeField] private Material blueMat;
    [SerializeField] private Material yellowMat;



    [Header("Heart and Decaying")]
    public float heart = 5f;
    public float decaysHit = 1f;
    public bool isDecaying = false;
    private float _maxHeartValue;

    private ExitDoor door;
    public bool isExit = false;

    [Header("Service")]
    public List<ItemSO> servicePool = new List<ItemSO>();
    public int serviceCount;
    public int deliveryPerSlot = 1;  // จำนวนชิ้นที่ต้องส่งต่อ 1 slot (Witch = 2)

    [Header("Room Requirement")]
    [SerializeField] private GuestRoomRequirementMode roomRequirementMode = GuestRoomRequirementMode.Any;
    [SerializeField] private RoomType requiredRoomType = RoomType.Small;

    [Header("Economy")]
    public int roomPayment;
    public int tip = 0;
    public int servicePayment = 0;
    public int totalIncome;

    public ItemSO currentService;
    private int _roomCost;

    // หา GuestUIController จาก children อัตโนมัติ
    [HideInInspector] public GuestUIController guestUI;
    [HideInInspector] public Room room;

    // ─────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────

    public override void Start()
    {
        base.Start();
        _maxHeartValue = heart;
        door = FindAnyObjectByType<ExitDoor>();
        if (guestSound != null) SoundManager.Instance.PlaySFX(guestSound);
        guestPhase = Guestphase.CheckingIn;

        guestUI = GetComponentInChildren<GuestUIController>(true);
        if (guestUI != null) guestUI.Init(this);
        
    }


    public void RandomColor(List<ObjColor> colors) //สุ่มสี
    {
        
        int randomIndex = UnityEngine.Random.Range(0, colors.Count);

        // ต้องแน่ใจว่ามีการประกาศตัวแปร guestColor ไว้แล้ว
        guestColor = colors[randomIndex];


        UpdateAllChildrenMaterials();
    }

    private void UpdateAllChildrenMaterials()
    {
        SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>(true);

        Material targetMat = GetMaterialByEnum(guestColor);

        if (targetMat != null)
        {
            foreach (SpriteRenderer renderer in allRenderers)
            {
                renderer.material = targetMat;
            }
        }
    }

    private Material GetMaterialByEnum(ObjColor color)
    {
        switch (color)
        {
            case ObjColor.Orange: return orangeMat;
            case ObjColor.Blue: return blueMat;
            case ObjColor.Yellow: return yellowMat;
            default: return null;
        }
    }



    // ─────────────────────────────────────────────
    //  Entry Point
    // ─────────────────────────────────────────────

    public bool CanAssignToRoom(Room targetRoom)
    {
        if (targetRoom == null || targetRoom.RoomData == null) return false;
        if (roomRequirementMode == GuestRoomRequirementMode.Any) return true;
        return targetRoom.RoomData.roomType == requiredRoomType;
    }

    public string GetRoomRequirementFailMessage()
    {
        if (roomRequirementMode == GuestRoomRequirementMode.Any) return "Room is not allowed";
        return "Need a " + requiredRoomType + " Room Only";
    }

    public void SetRequiredRoomType(RoomType roomType)
    {
        roomRequirementMode = GuestRoomRequirementMode.SpecificType;
        requiredRoomType = roomType;
    }
    public void StartProcessing(Room room)
    {
        this.room = room;
        guestPhase = Guestphase.RequestingService;
        StartCoroutine(ProcessRequests(room));
    }

    // ─────────────────────────────────────────────
    //  Core Loop
    // ─────────────────────────────────────────────

    private IEnumerator ProcessRequests(Room room)
    {
        for (int slotIndex = 0; slotIndex < room.serviceQueue.Count; slotIndex++)
        {
            ItemSO service = GetServiceForSlot(slotIndex, room.serviceQueue[slotIndex]);
            int requiredDeliveryCount = GetRequiredDeliveryCountForService(service);

            room.isDelivered = false;
            room.deliveryCount = 0;
            currentService = service;
            OnServiceStart(service);
            guestUI?.ShowBubble(service);
            guestUI?.SetDeliveryCount(requiredDeliveryCount);

            Coroutine decayCoroutine = StartDecayCoroutine();

            while (true)
            {
                if (room.isDelivered)
                {
                    isDecaying = false;
                    if (decayCoroutine != null) StopCoroutine(decayCoroutine);
                    break;
                }
                if (isExit)
                {
                    guestUI?.HideBubble();
                    if (room != null) room.DirtyRoom();
                    if (decayCoroutine != null) StopCoroutine(decayCoroutine);
                    StopAllCoroutines();
                    yield break;
                }
                yield return null;
            }

            guestUI?.HideBubble();
            guestUI?.SetDeliveryCount(0);
            currentService = null;

            if (room.isDelivered) OnServiceSuccess(service);
            else OnServiceFail(service, room);

            if (slotIndex < room.serviceQueue.Count - 1)
                yield return OnBetweenServices(slotIndex);

            yield return new WaitForSeconds(room.serviceCooldown);
        }

        OnAllServicesComplete(room.counter);
        if (room != null) room.DirtyRoom();
    }

    // ─────────────────────────────────────────────
    //  Virtual Hooks
    // ─────────────────────────────────────────────

    protected virtual ItemSO GetServiceForSlot(int slotIndex, ItemSO listItem) => listItem;
    public virtual int GetRequiredDeliveryCountForService(ItemSO service)
    {
        if (service != null && service.requiredForService == ServiceRequestType.DeliveryLuggage)
        {
            return 1;
        }

        return Mathf.Max(1, deliveryPerSlot);
    }

    protected virtual IEnumerator OnBetweenServices(int completedSlotIndex) { yield break; }

    public virtual void OnCheckIn() { }
    public virtual void OnCheckOut(bool isAnger)
    {
        
    }
    public virtual void OnServiceStart(ItemSO service) { }

    public virtual void OnServiceSuccess(ItemSO service)
    {
        servicePayment += 35;
        tip += 10;
        RestoreHeart();
    }

    public virtual void OnServiceFail(ItemSO service , Room room)
    {
        servicePayment--;
        CleanupFailedService(service);
    }

    public virtual void OnAllServicesComplete(Counter counter)
        => CheckOut(counter.interactObjData);

    public void RestoreHeart()
    {
        heart = _maxHeartValue > 0f ? _maxHeartValue : 5f;
    }

    // ─────────────────────────────────────────────
    //  Decay
    // ─────────────────────────────────────────────

    public Coroutine StartDecayCoroutine()
    {
        isDecaying = true;
        return StartCoroutine(DecayProgress());
    }

    IEnumerator DecayProgress()
    {
        while (heart > 0)
        {
            yield return new WaitForSeconds(3f);
            Decay(decaysHit);
        }
    }

    private void Decay(float amount)
    {
        heart -= amount;
        if (heart <= 0) QuitHotel(door.interactObjData);
    }

    // ─────────────────────────────────────────────
    //  Check-in / Check-out
    // ─────────────────────────────────────────────

    public void QuitHotel(InteractObjData exit)
    {
        if (isExit) return;
        guestPhase = Guestphase.CheckingOut;
        isExit = true;
        CleanupFailedService(currentService);
        OnCheckOut(isAnger: true);
        guestUI?.OnCheckOut();
        AnimateExitRoom();
        if (room != null) room.DirtyRoom();
        StopAllCoroutines();
        StartTravel(exit);
    }

    protected void CleanupFailedService(ItemSO failedService)
    {
        if (failedService == null) return;

        if (failedService.requiredForService != ServiceRequestType.DeliveryLuggage) return;

        RoomManager.Instance?.DeleteLuggage();

        if (TryRemoveLuggageFromPlayers(1)) return;
        TryRemoveLuggageFromEmployees(1);
    }

    private bool TryRemoveLuggageFromPlayers(int amount)
    {
        int remaining = Mathf.Max(0, amount);
        if (remaining == 0) return false;

        Player[] players = FindObjectsByType<Player>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < players.Length && remaining > 0; i++)
        {
            Player player = players[i];
            if (player == null) continue;

            int removed = player.RemoveItemsByServiceType(ServiceRequestType.DeliveryLuggage, remaining);
            remaining -= removed;
        }

        return remaining <= 0;
    }

    private bool TryRemoveLuggageFromEmployees(int amount)
    {
        int remaining = Mathf.Max(0, amount);
        if (remaining == 0) return false;

        Employee[] employees = FindObjectsByType<Employee>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < employees.Length && remaining > 0; i++)
        {
            Employee employee = employees[i];
            if (employee == null) continue;

            int removed = employee.RemoveItemsByServiceType(ServiceRequestType.DeliveryLuggage, remaining);
            remaining -= removed;
        }

        return remaining <= 0;
    }

    public void CheckOut(InteractObjData targetObj)
    {
        guestPhase = Guestphase.CheckingOut;
        OnCheckOut(isAnger: false);
        guestUI?.OnCheckOut();
        AnimateExitRoom();
        CalculateRentNET();
        targetIObj.objCollider.isTrigger = true;
        StartTravel(targetObj);
    }

    public void CheckRoomCost(int cost) => _roomCost = cost;
    public void CalculateRentNET() => totalIncome = tip + servicePayment + roomPayment;

    // ─────────────────────────────────────────────
    //  Animation — DOScale เฉพาะ characterVisual
    // ─────────────────────────────────────────────

    public void AnimateEnterRoom()
    {
        List<GameObject> visuals = GetCharacterVisuals();
        if (visuals.Count == 0) return;

        if (_originalVisualScales.Count != visuals.Count)
        {
            CacheCharacterVisualScales();
            visuals = GetCharacterVisuals();
        }

        for (int i = 0; i < visuals.Count; i++)
        {
            GameObject visual = visuals[i];
            if (visual == null) continue;

            Vector3 scale = i < _originalVisualScales.Count ? _originalVisualScales[i] : _originalScale;
            visual.transform.DOKill();
            visual.transform.localScale = scale;
            visual.transform
                .DOPunchScale(scale * 0.2f, 0.3f, 5, 1f)
                .OnComplete(() =>
                    visual.transform
                        .DOScale(Vector3.zero, 0.5f)
                        .SetEase(Ease.InBack));
        }
    }

    public void AnimateExitRoom()
    {
        List<GameObject> visuals = GetCharacterVisuals();
        if (visuals.Count == 0) return;

        if (_originalVisualScales.Count != visuals.Count)
        {
            CacheCharacterVisualScales();
            visuals = GetCharacterVisuals();
        }

        for (int i = 0; i < visuals.Count; i++)
        {
            GameObject visual = visuals[i];
            if (visual == null) continue;

            Vector3 scale = i < _originalVisualScales.Count ? _originalVisualScales[i] : _originalScale;
            visual.transform.DOKill();
            visual.transform
                .DOScale(scale, 0.5f)
                .SetEase(Ease.OutBack);
        }
    }
}
