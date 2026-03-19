using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum Guestphase
{
    CheckingIn,
    InRoom,
    RequestingService,
    CheckingOut
}

/// <summary>
/// Base class สำหรับแขกทุกประเภท
/// type ถูกกำหนดโดย subclass — ไม่มี GuestType enum
/// ServiceManager เรียกผ่าน virtual hooks เท่านั้น
/// </summary>
public class GuestAI : MoveHandleAI
{
    public Guestphase guestPhase;

    [Header("Heart and Decaying")]
    public float heart = 5f;
    public float decaysHit = 1f;
    public bool isDecaying = false;

    private ExitDoor door;
    public bool isExit = false;

    [Header("Hide On Check-in")]
    public List<Transform> hideOnCheckIn = new List<Transform>();
    public float hideScaleDuration = 0.25f;

    [Header("Service")]
    public int serviceCount;
    public List<ItemSO> serviceRequest_All = new List<ItemSO>();

    [Header("Economy")]
    public int roomPayment;
    public int tip = 0;
    public int servicePayment = 0;

    public int totalIncome;


    public ItemSO currentService;

    private Vector3 _originalScale;
    private int _roomCost;

    // ─────────────────────────────────────────────
    //  Unity Lifecycle
    // ─────────────────────────────────────────────

    public override void Start()
    {
        base.Start();
        _originalScale = transform.localScale;
        door = FindAnyObjectByType<ExitDoor>();
        guestPhase = Guestphase.CheckingIn;
    }

    // ─────────────────────────────────────────────
    //  Service Hooks  (ServiceManager เรียกผ่านนี้เท่านั้น)
    // ─────────────────────────────────────────────

    /// <summary>ก่อนเริ่ม service แต่ละรายการ</summary>
    public virtual void OnServiceStart(ItemSO service) { }

    /// <summary>ส่งของสำเร็จ</summary>
    public virtual void OnServiceSuccess(ItemSO service)
    {
        servicePayment += 35;
        tip += 10;
        heart = 5f;
    }

    /// <summary>หมดเวลา / ส่งของไม่ได้</summary>
    public virtual void OnServiceFail(ItemSO service)
    {
        servicePayment--;
    }

    /// <summary>Service ครบทุกรายการ → checkout</summary>
    public virtual void OnAllServicesComplete(Counter counter)
    {
        CheckOut(counter.interactObjData);
    }

    // ─────────────────────────────────────────────
    //  Phase Events (Room.cs เรียกหลัง trigger)
    // ─────────────────────────────────────────────

    /// <summary>แขก Check-in เข้าห้องสำเร็จ</summary>
    public virtual void OnCheckIn()
    {
        foreach (var t in hideOnCheckIn)
            if (t != null) t.DOScale(Vector3.zero, hideScaleDuration).SetEase(Ease.InBack);
    }

    /// <summary>แขกกำลังจะ Check-out (ทั้งปกติและโกรธ)</summary>
    public virtual void OnCheckOut(bool isAnger) { }

    // ─────────────────────────────────────────────
    //  Decay
    // ─────────────────────────────────────────────

    /// <summary>
    /// เริ่ม decay และคืน Coroutine reference กลับให้ ServiceManager
    /// เพื่อให้ stop ได้เฉพาะตัว ไม่กระทบ coroutine อื่นของ subclass
    /// </summary>
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

    public void RequestService(ServiceManager serviceManager)
    {
        guestPhase = Guestphase.RequestingService;
        serviceManager.listService.Clear();
        serviceManager.ServiceSetUp(serviceRequest_All, serviceCount);
        serviceManager.StartRequests(this);
    }

    public void QuitHotel(InteractObjData exit)
    {
        if (isExit) return;
        guestPhase = Guestphase.CheckingOut;
        isExit = true;
        OnCheckOut(isAnger: true);
        AnimateExitRoom();
        StopAllCoroutines();
        StartTravel(exit);
    }

    public void CheckOut(InteractObjData targetObj)
    {
        guestPhase = Guestphase.CheckingOut;
        OnCheckOut(isAnger: false);
        AnimateExitRoom();
        CalculateRentNET();
        targetIObj.objCollider.isTrigger = true;
        StartTravel(targetObj);
    }

    public void CheckRoomCost(int cost) => _roomCost = cost;

    public void CalculateRentNET() => totalIncome = tip + servicePayment + roomPayment;

    // ─────────────────────────────────────────────
    //  Animation
    // ─────────────────────────────────────────────

    public void AnimateEnterRoom()
    {
        transform.DOPunchScale(_originalScale * 0.2f, 0.3f, 5, 1f)
            .OnComplete(() => transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));
    }

    public void AnimateExitRoom()
    {
        transform.DOScale(_originalScale, 0.5f).SetEase(Ease.OutBack);
    }
}