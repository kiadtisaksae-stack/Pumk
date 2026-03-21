using UnityEngine;

/// <summary>
/// Ghost — The Classic Guest
/// - 2 items (Luggage fixed + 1 random: Food/Drink/Soul)
/// - Heart: 5, decays -0.5 every 3s
/// - No events
/// servicePool ใน Inspector: Food, Drink, Soul
/// </summary>
public class GhostGuest : GuestAI
{
    public override void Start()
    {
        base.Start();
        serviceCount = 2;
        decaysHit = 0.5f;
    }
}