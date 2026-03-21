using UnityEngine;

/// <summary>
/// Vampire — The High-Class Guest
/// - 3 items (Luggage fixed + 2 random: Food/Drink/Soul)
/// - Heart: 5, decays -1.0 every 3s
/// - No events
/// servicePool ใน Inspector: Food, Drink, Soul
/// </summary>
public class VampireGuest : GuestAI
{
    public override void Start()
    {
        base.Start();
        serviceCount = 3;
        decaysHit = 1.0f;
    }
}