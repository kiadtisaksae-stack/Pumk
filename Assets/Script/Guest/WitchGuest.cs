using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Witch & Cat — Two-in-One Guest
/// - Witch: 2 items, Cat: 2 items
/// - ทั้งคู่ request พร้อมกันในรอบเดียวกัน (Witch #1 + Cat #1 at 5s, #2 + #2 at 10s)
/// - Heart: 5, decays -0.5 every 3s
/// - No special events
/// - Reward: ~120 coins
///
/// DESIGN: ServiceManager loop เดินทีละ service ตามปกติ
/// WitchGuest inject catCurrentService ให้แสดง popup แยก
/// Room.cs ต้องเช็ค catCurrentService เพิ่มเติมใน OnTriggerEnter2D
/// </summary>
public class WitchGuest : GuestAI
{
    [Header("Cat Settings")]
    public List<ItemSO> catServicePool = new List<ItemSO>();
    public int catServiceCount = 2;

    [Header("Cat UI")]
    public Button catServiceButton;

    // service ที่แมวกำลังขออยู่ — Room.cs ใช้เช็ค RequestCheck
    public ItemSO catCurrentService { get; private set; }
    public bool isCatSuccess { get; set; } = false;

    private List<ItemSO> _catList = new List<ItemSO>();
    private int _catIndex = 0;

    public override void Start()
    {
        base.Start();
        serviceCount = 2;
        decaysHit = 0.5f;

        if (catServiceButton != null)
            catServiceButton.gameObject.SetActive(false);
    }

    public override void OnCheckIn()
    {
        _catList = BuildCatList();
        _catIndex = 0;
    }

    public override void OnServiceStart(ItemSO witchService)
    {
        // แสดง cat service พร้อมกัน
        if (_catIndex < _catList.Count)
        {
            catCurrentService = _catList[_catIndex];
            _catIndex++;
            isCatSuccess = false;

            if (catServiceButton != null)
            {
                catServiceButton.gameObject.SetActive(true);
                catServiceButton.image.sprite = catCurrentService.itemIcon;
            }
        }
    }

    public override void OnServiceSuccess(ItemSO service)
    {
        base.OnServiceSuccess(service);
        HideCatPopup();
    }

    public override void OnServiceFail(ItemSO service)
    {
        base.OnServiceFail(service);
        HideCatPopup();
    }

    private void HideCatPopup()
    {
        catCurrentService = null;
        isCatSuccess = false;
        if (catServiceButton != null)
            catServiceButton.gameObject.SetActive(false);
    }

    private List<ItemSO> BuildCatList()
    {
        List<ItemSO> pool = new List<ItemSO>(catServicePool);
        List<ItemSO> result = new List<ItemSO>();

        for (int i = 0; i < catServiceCount && pool.Count > 0; i++)
        {
            int r = Random.Range(0, pool.Count);
            result.Add(pool[r]);
            pool.RemoveAt(r);
        }
        return result;
    }
}
