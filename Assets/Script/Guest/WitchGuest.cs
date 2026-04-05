using UnityEngine;

/// <summary>
/// Witch — 2 items (Luggage fixed + 1 random), decays -0.5 every 3s
/// EVENT: แต่ละ service slot ต้องส่งของ 2 ชิ้นพร้อมกัน (deliveryPerSlot = 2)
/// Room.CheckDelivery อ่านค่า deliveryPerSlot จาก guest ก่อนตัดสิน isDelivered
/// servicePool ใน Inspector: Food, Drink, Soul
/// </summary>
public class WitchGuest : GuestAI
{
    public override void Start()
    {
        base.Start();
        //serviceCount = 2;
        decaysHit = 0.5f;
        //deliveryPerSlot = 2;    // ต้องส่งครบ 2 ชิ้นต่อ 1 slot
    }
}