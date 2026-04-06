using UnityEngine;

/// <summary>
/// Mummy — The Special-Request Guest
/// - 3 items (Luggage fixed + 2 random)
/// - Heart: 5, decays -1.0 every 3s
/// - EVENT: Cloth guaranteed at slot 1 OR 2 (random)
///   - ก่อน event: servicePool = Food/Drink/Soul  (ไม่มี Cloth)
///   - หลัง event: Cloth ถูกเพิ่มเข้า servicePool ด้วย (อาจโผล่ใน request ถัดไป)
///
/// servicePool ใน Inspector: Food, Drink, Soul  (อย่าใส่ Cloth — inject อัตโนมัติ)
/// clothItem ใน Inspector: ลาก Cloth ItemSO ใส่
/// </summary>
public class MummyGuest : GuestAI
{
    [Header("Cloth Settings")]
    public ItemSO clothItem;

    private int _clothForcedSlot = -1;
    private bool _clothTriggered = false;

    public override void Start()
    {
        base.Start();
        serviceCount = 3;       // Luggage + 2 random = 3 items
        decaysHit = 1.0f;
    }

    public override void OnCheckIn()
    {
        _clothTriggered = false;
        // สุ่มว่า Cloth จะ forced ที่ slot 1 หรือ slot 2  (0-indexed; slot 0 = Luggage)
        _clothForcedSlot = Random.Range(1, 3);

    }

    protected override ItemSO GetServiceForSlot(int slotIndex, ItemSO listItem)
    {
        if (!_clothTriggered && slotIndex == _clothForcedSlot && clothItem != null)
        {
            _clothTriggered = true;


            // หลัง event เพิ่ม Cloth เข้า pool → อาจโผล่ใน request ถัดไป
            if (!servicePool.Contains(clothItem))
                servicePool.Add(clothItem);

            return clothItem;
        }
        return listItem;
    }
}