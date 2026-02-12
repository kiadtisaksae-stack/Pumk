using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Service")]
    public List<ItemSO> allService = new List<ItemSO>();
    public float serviceCooldown = 5f;

    public int rentNet;
    public int servicePoint = 0;

    public ItemSO currentService;


    private void Start()
    {
        guestPhase = Guestphase.CheckingIn;
    }
    //Flow = TriggerEvent => SO => This

    public void RequestService(ServiceManager serviceManager)
    {
        guestPhase = Guestphase.RequestingService;
        gameObject.SetActive(false);
        serviceManager.listService.Clear();
        serviceManager.ServiceSetUp(allService);
        Debug.Log("เริ่มขอรายการ !!");
        serviceManager.StartRequests(this);
    }


    public void CheckOut(InteractObjData targetObj)
    {
        guestPhase = Guestphase.CheckingOut;
        gameObject.SetActive(true);
        CalculateRentNET();
        targetIObj.objCollider.isTrigger = true;
        StartTravel(targetObj);
    }

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
}
