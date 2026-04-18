using UnityEngine;

/// <summary>
/// Reaper — The Grand Suite Guest
/// - 3 items (Luggage fixed + 2 random: Food/Drink/Soul)
/// - Heart: 5, decays -0.5 every 3s
/// - No events
/// - REQUIRES RoomType.Big — GuestRoomAssigner เช็กก่อน assign
/// servicePool ใน Inspector: Food, Drink, Soul
/// </summary>
public class ReaperGuest : GuestAI
{
    public override void Start()
    {
        base.Start();
        SetRequiredRoomType(RoomType.Big);
        serviceCount = 3;
        decaysHit = 0.5f;
    }
}
