using UnityEngine;

/// <summary>
/// Reaper — The Grand Suite Guest
/// - Requests 3 items (Luggage fixed + 2 random from Food/Drink/Soul)
/// - Heart: 5, decays -0.5 every 3s
/// - No special events
/// - REQUIRES Large Room (RoomType.Big) — GuestRoomAssigner ต้องเช็กก่อน assign
/// - Reward: ~105 coins
/// </summary>
public class ReaperGuest : GuestAI
{
    public override void Start()
    {
        base.Start();
        serviceCount = 3;
        decaysHit = 0.5f;
    }

    public override void OnCheckIn()
    {
        // ตรวจสอบว่าอยู่ห้อง Big ถ้าไม่ใช่ — log warning (GuestRoomAssigner ควรกรองก่อนถึงตรงนี้)
        if (finalRoomData != null && finalRoomData.roomType != RoomType.Big)
        {
            Debug.LogWarning($"<color=red>Reaper ต้องการ Large Room! ได้รับ {finalRoomData.roomType}</color>");
        }
    }
}
