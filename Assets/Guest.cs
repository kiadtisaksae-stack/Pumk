using System.Collections.Generic;
using UnityEngine;

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

    public ItemSO serviceRequest;

    public ServiceManager serviceManager;


    private void Start()
    {
        guestPhase = Guestphase.CheckingIn;
        serviceManager = FindAnyObjectByType<ServiceManager>();
    }
    //Flow = TriggerEvent => SO => This
    public void RequestService(ItemSO item)
    {
        serviceRequest = item;

        
    }
    
    public void SetGuestPhase(Guestphase guestPhase)
    {
        this.guestPhase = guestPhase;
    }

    public void TriggerEvents(Guestphase guestphase)
    {
        RequestService(this.serviceRequest);
    }
}
