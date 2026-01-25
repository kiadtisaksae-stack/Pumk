using UnityEngine;

public abstract class GuestEventSO : ScriptableObject
{
    public string eventName;

    [Header("Trigger State")]
    public Guestphase triggerGuestPhase;

    [Range(0f, 1f)]
    public float chance = 0.3f;

    public abstract void Execute(GuestAI guest);
}
