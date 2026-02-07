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
    private List<ItemSO> serviceQueue = new List<ItemSO>();
    public float serviceCooldown = 5f;

    private ItemSO currentService;
    public Image serviceImage;
    public ServiceManager serviceManager;


    private void Start()
    {
        guestPhase = Guestphase.CheckingIn;
        //serviceManager = FindAnyObjectByType<ServiceManager>();
    }
    //Flow = TriggerEvent => SO => This

    public void RequestService(ServiceManager serviceManager)
    {
        guestPhase = Guestphase.RequestingService;
        serviceManager.listService.Clear();
        serviceManager.ServiceSetUp(allService);
        Debug.Log("เริ่มขอรายการ !!");
        //serviceManager.StartCoroutine(ProcessRequests());
        
    }

    //IEnumerator ProcessRequests()
    //{
    //    foreach (ItemSO service in serviceQueue)
    //    {
    //        Debug.Log("ลูกค้าขอ: " + service.name);
    //        ServicePopUp(service);

    //        yield return new WaitForSeconds(service.deliverTime);


    //        yield return new WaitForSeconds(serviceCooldown);
    //    }

    //}

    //public void ServiceSetUp(List<ItemSO> allService)
    //{
    //    //หา Luggage ก่อน
    //    FindLuggageService(currentService, allService);

    //    List<ItemSO> randomService = new List<ItemSO>(allService);
    //    ShuffleList(randomService);
    //    serviceQueue.AddRange(randomService);


    //}
    //void ShuffleList(List<ItemSO> allService)
    //{
    //    for (int i = 0; i < allService.Count; i++)
    //    {
    //        ItemSO service = allService[i];
    //        int random = Random.Range(i, allService.Count);
    //        allService[i] = allService[random];
    //        allService[random] = service;
    //    }
    //}

    //public ItemSO FindLuggageService(ItemSO currentService, List<ItemSO> allService)
    //{
    //    foreach (ItemSO service in allService)
    //    {
    //        if (service.requiredForService == ServiceRequestType.DeliveryLuggage)
    //        {
    //            currentService = service;
    //            serviceQueue.Add(currentService);
    //            allService.Remove(service);
    //            break;
    //        }
    //    }
    //    return currentService;
    //}

    //public void ServicePopUp(ItemSO service)
    //{
    //    serviceImage = service.itemIcon;
    //}
    
    public void SetGuestPhase(Guestphase guestPhase)
    {
        this.guestPhase = guestPhase;
    }

    public void TriggerEvents(Guestphase guestphase)
    {
        //RequestService(serviceManager);
    }
}
