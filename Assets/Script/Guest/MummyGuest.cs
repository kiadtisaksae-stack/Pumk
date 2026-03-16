using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mummy — The Special-Request Guest
/// - Requests 3 items (Luggage fixed + 2 random)
/// - Heart: 5, decays -1.0 every 3s
/// - EVENT: Cloth request guaranteed ครั้งแรกที่ request #2 หรือ #3 (สุ่ม)
///   และหลังจากนั้น Cloth ถูกเพิ่มเข้า random pool ด้วย
///
/// DESIGN: override ServiceSetUp ผ่าน hook OnCheckIn
///   - ก่อนเริ่ม: Cloth ไม่อยู่ใน pool
///   - เมื่อถึง trigger slot: inject Cloth เป็น service นั้น (forced)
///   - หลัง triggered: Cloth อยู่ใน pool ปกติ
/// </summary>
public class MummyGuest : GuestAI
{
    [Header("Cloth Settings")]
    public ItemSO clothItem;    // ลาก Cloth ItemSO ใน Inspector

    // slot ที่ Cloth จะ forced (1 = request #2, 2 = request #3, นับจาก 0)
    private int _clothForcedSlot = -1;
    private bool _clothTriggered = false;

    // service ที่ถูก inject แทนที่ random (ใช้ใน ServiceManager override)
    public ItemSO ForcedNextService { get; private set; } = null;

    public override void Start()
    {
        base.Start();
        serviceCount = 3;
        decaysHit = 1.0f;
    }

    public override void OnCheckIn()
    {
        _clothTriggered = false;
        // สุ่มว่า Cloth จะ forced ที่ slot 1 หรือ 2 (request #2 หรือ #3)
        _clothForcedSlot = Random.Range(1, 3);
        Debug.Log($"<color=yellow>Mummy: Cloth forced ที่ slot {_clothForcedSlot}</color>");
    }

    public override void OnServiceStart(ItemSO service)
    {
        // ล้าง forced service หลังจาก ServiceManager หยิบไปแล้ว
        ForcedNextService = null;
    }

    /// <summary>
    /// ServiceManager เรียกก่อนเลือก service ถัดไปจาก list
    /// ถ้า return non-null = ใช้ ItemSO นี้แทน list item
    /// </summary>
    public ItemSO GetForcedServiceForSlot(int slotIndex)
    {
        if (!_clothTriggered && slotIndex == _clothForcedSlot && clothItem != null)
        {
            _clothTriggered = true;
            Debug.Log("<color=yellow>Mummy: Cloth forced!</color>");

            // หลัง triggered เพิ่ม Cloth เข้า pool ด้วย (ถ้ายังไม่มี)
            if (!serviceRequest_All.Contains(clothItem))
                serviceRequest_All.Add(clothItem);

            return clothItem;
        }
        return null;
    }
}