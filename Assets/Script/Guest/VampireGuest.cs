using UnityEngine;

/// <summary>
/// Vampire — The High-Class Guest
/// - Requests 3 items (Luggage fixed + 2 random from Food/Drink/Soul)
/// - Heart: 5, decays -1.0 every 3s (เร็วกว่าปกติ 2x)
/// - No special events
/// - Reward: ~105 coins
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
