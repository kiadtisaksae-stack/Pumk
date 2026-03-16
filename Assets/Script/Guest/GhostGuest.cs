using UnityEngine;

/// <summary>
/// Ghost — The Classic Guest
/// - Requests 2 items (Luggage fixed + 1 random from Food/Drink/Soul)
/// - Heart: 5, decays -0.5 every 3s
/// - No special events
/// - Used for tutorial and system testing
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
