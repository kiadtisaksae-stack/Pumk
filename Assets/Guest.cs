using System.Collections.Generic;
using UnityEngine;

public class GuestAI : MoveHandleAI
{
    public Guestphase guestPhase;
    [SerializeField]
    private ItemSO serviceRequest;
    public ItemSO ServiceRequest => serviceRequest;

    [Header("Special Events")]
    [SerializeField]
    private List<GuestEventSO> specialEvents;

    public void RequestService(ItemSO item)
    {
        serviceRequest = item;
        SetGuestPhase(Guestphase.RequestingService);

        Debug.Log($"🛎 แขก {name} ขอ {item.itemName}");
    }
    public void SetGuestPhase(Guestphase newPhase)
    {
        if (guestPhase == newPhase) return;

        guestPhase = newPhase;
        TriggerEvents();
    }


    private void TriggerEvents()
    {
        foreach (var ev in specialEvents)
        {
            if (ev.triggerGuestPhase != guestPhase) continue;
            if (Random.value > ev.chance) continue;

            ev.Execute(this);
        }
    }
}
