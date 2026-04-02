using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum Guestphase { CheckingIn, InRoom, RequestingService, CheckingOut }

/// <summary>
/// Base class — data + logic เท่านั้น ไม่รู้จัก UI
/// UI ทั้งหมดจัดการโดย GuestUIController (หาจาก children อัตโนมัติ)
/// </summary>
public class GuestAI : MoveHandleAI
{
    public Guestphase guestPhase;
    [SerializeField] public ObjColor guestColor;


    [Header("Material References")]


    [SerializeField] private Material orangeMat;
    [SerializeField] private Material blueMat;
    [SerializeField] private Material yellowMat;



    [Header("Heart and Decaying")]
    public float heart = 5f;
    public float decaysHit = 1f;
    public bool isDecaying = false;

    private ExitDoor door;
    public bool isExit = false;

    [Header("Service")]
    public List<ItemSO> servicePool = new List<ItemSO>();
    public int serviceCount;
    public int deliveryPerSlot = 1;  // จำนวนชิ้นที่ต้องส่งต่อ 1 slot (Witch = 2)

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
        door = FindAnyObjectByType<ExitDoor>();
        guestPhase = Guestphase.CheckingIn;

        guestUI = GetComponentInChildren<GuestUIController>(true);
        if (guestUI != null) guestUI.Init(this);
        RandomColor();
    }


    private void RandomColor() //สุ่มสี
    {
        System.Array values = System.Enum.GetValues(typeof(ObjColor));

        int randomIndex = UnityEngine.Random.Range(0, values.Length);

        // ต้องแน่ใจว่ามีการประกาศตัวแปร guestColor ไว้แล้ว
        guestColor = (ObjColor)values.GetValue(randomIndex);

        Debug.Log(this.name + " สุ่มได้สี: " + guestColor);
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

            room.isDelivered = false;
            room.deliveryCount = 0;
            currentService = service;
            OnServiceStart(service);
            guestUI?.ShowBubble(service);
            guestUI?.SetDeliveryCount(deliveryPerSlot);

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
        heart = 5f;
    }

    public virtual void OnServiceFail(ItemSO service , Room room)
    {
        servicePayment--;
        
    }

    public virtual void OnAllServicesComplete(Counter counter)
        => CheckOut(counter.interactObjData);

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
        OnCheckOut(isAnger: true);
        guestUI?.OnCheckOut();
        AnimateExitRoom();
        if (room != null) room.DirtyRoom();
        StopAllCoroutines();
        StartTravel(exit);
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
        if (characterVisual == null) return;
        characterVisual.transform.DOKill();
        characterVisual.transform.localScale = _originalScale;
        characterVisual.transform
            .DOPunchScale(_originalScale * 0.2f, 0.3f, 5, 1f)
            .OnComplete(() =>
                characterVisual.transform
                    .DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InBack));
    }

    public void AnimateExitRoom()
    {
        if (characterVisual == null) return;
        characterVisual.transform.DOKill();
        characterVisual.transform
            .DOScale(_originalScale, 0.5f)
            .SetEase(Ease.OutBack);
    }
}