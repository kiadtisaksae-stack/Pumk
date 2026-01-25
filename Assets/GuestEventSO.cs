using UnityEngine;

public abstract class GuestEventSO : ScriptableObject
{
    public string eventName;

    [Range(0f, 1f)]
    public float chance = 0.3f;

    public abstract void Execute(GuestAI guest);
}
