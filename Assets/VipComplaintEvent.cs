using UnityEngine;

[CreateAssetMenu(menuName = "Hotel/GuestEvent/VIP Complaint")]
public class VipComplaintEvent : GuestEventSO
{
    public ItemSO requiredItem;

    public override void Execute(GuestAI guest)
    {
        Debug.Log($"😡 VIP {guest.name} ร้องเรียน!");

        guest.RequestService(requiredItem);

        // ลดความพึงพอใจ / เพิ่มค่าปรับ
    }
}
