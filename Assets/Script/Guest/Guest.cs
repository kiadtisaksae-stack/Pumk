using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum Guestphase
{
    CheckingIn,
    InRoom,
    RequestingService,
    CheckingOut
}
public enum GuestType
{
    FriendlyGhost,
    Vampire,
    Witch,
    Werewolf,
    Mummy,
    Franken,
    Reaper
}

public class GuestAI : MoveHandleAI
{
    public Guestphase guestPhase;

    [Header("Heart and Decaying to Exit")]
    public float heart;
    public float decaysHit;
    public bool isDecaying = false;


    private ExitDoor door;
    public bool isExit = false;

    [Header("Service")]
    public int serviceCount;
    public List<ItemSO> servicePool = new List<ItemSO>();
    public float serviceCooldown = 5f;

    public int rentNet;
    public int servicePoint = 0;

    public ItemSO currentService;

    private Vector3 originalScale;


    public override void Start()
    {
        base.Start();
        originalScale = transform.localScale;
        door = FindAnyObjectByType<ExitDoor>();
        guestPhase = Guestphase.CheckingIn;
 
    }
    //Flow = TriggerEvent => SO => This





    public void RequestService(ServiceManager serviceManager)
    {
        guestPhase = Guestphase.RequestingService;
        //SetRendererActive(false);
        serviceManager.listService.Clear();
        serviceManager.ServiceSetUp(servicePool , serviceCount);
        Debug.Log("เริ่มขอรายการ !!");
        serviceManager.StartRequests(this);
    }

    public void StartDelay(bool isDecaying)
    {
        if (isDecaying)
        {
            StartCoroutine(DecayProgress());
        }       
    }

    IEnumerator DecayProgress()
    {
        while (heart > 0)
        {
            yield return new WaitForSeconds(3f); 
            Decay(decaysHit);         
        }
    }

    private void Decay(float decayAmount)
    {
        heart -= decayAmount;
        Debug.Log("เหลือ" + heart);
        if (heart <= 0)
        {
            QuitHotel(door.interactObjData);
        }
    }


    public void QuitHotel(InteractObjData exit)
    {
        guestPhase = Guestphase.CheckingOut;
        isExit = true;
        AnimateExitRoom();
        StopAllCoroutines();
        Debug.Log("ออกจากโรงเรมแล้ว ไม่พอใจ");
        StartTravel(exit);
    }

    public void CheckOut(InteractObjData targetObj)
    {
        guestPhase = Guestphase.CheckingOut;
        AnimateExitRoom();
        CalculateRentNET();
        targetIObj.objCollider.isTrigger = true;
        StartTravel(targetObj);
    }



    //public void SetRendererActive(bool isEnable)
    //{
    //    Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
    //    foreach (Renderer r in allRenderers)
    //    {
    //        r.enabled = isEnable;
    //    }
    //}

    public void SetGuestPhase(Guestphase guestPhase)
    {
        this.guestPhase = guestPhase;
    }  

    public void CalculateRentNET() //คำนวนรายจ่ายของแขก (เดี๋ยวมาทำต่อ)
    {
        rentNet = servicePoint + 100;
    }

    public void TriggerEvents(Guestphase guestphase)
    {
        //RequestService(serviceManager);
    }
    public void AnimateEnterRoom()
    {
        // 1. เด้ง (Punch) โดยอิงจาก Scale ปัจจุบัน
        // 2. หดตัวลงเหลือ 0
        transform.DOPunchScale(originalScale * 0.2f, 0.3f, 5, 1f)
            .OnComplete(() => {
                transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
            });
    }
    public void AnimateExitRoom()
    {
        // ขยายกลับมาเท่ากับค่าดั้งเดิมที่เก็บไว้ตอน Start
        transform.DOScale(originalScale, 0.5f).SetEase(Ease.OutBack);
    }
}
