using System.Collections;
using UnityEngine;

/// <summary>
/// Frankenstein — The Sleepwalker
/// - Requests 2 items (Luggage fixed + 1 random)
/// - Heart: 5, decays -0.5 every 3s
/// - EVENT: Sleepwalk เกิดครั้งเดียวใน stay (ที่ 10s หรือ 15s สุ่ม)
///   - Guest เดินออกจากห้อง
///   - Player ต้อง click ที่ตัว Guest เพื่อ wake
///   - ยิ่งช้า = เสีย heart เพิ่ม
/// - Reward: ~105 coins
/// </summary>
public class FrankenGuest : GuestAI
{
    [Header("Sleepwalk Settings")]
    public float heartLossPerSecond = 0.3f;
    public Transform sleepwalkTarget;       // จุดที่เดินไป (ตั้งใน Inspector หรือหา ExitDoor)

    public bool IsSleepwalking { get; private set; } = false;

    private bool _sleepwalkDone = false;
    private Coroutine _sleepwalkDamageCoroutine;

    public override void Start()
    {
        base.Start();
        serviceCount = 2;
        decaysHit = 0.5f;
    }

    public override void OnCheckIn()
    {
        _sleepwalkDone = false;
    }

    // ─────────────────────────────────────────────
    //  ServiceManager เรียก hook นี้ระหว่าง cooldown
    //  Franken ใช้ช่วงนี้ trigger sleepwalk
    //  (ServiceManager ต้อง call OnBetweenServices ใน cooldown loop)
    // ─────────────────────────────────────────────
    public override void OnServiceStart(ItemSO service)
    {
        // sleepwalk อาจเกิดก่อน service #2 (index 1)
        // ให้ ServiceManager handle timing ผ่าน TrySleepwalk() ที่เรียกจาก cooldown
    }

    /// <summary>
    /// ServiceManager เรียกระหว่าง cooldown ก่อน service ถัดไป
    /// คืน true ถ้าเริ่ม sleepwalk (ServiceManager ควรรอจนกว่าจะ wake)
    /// </summary>
    public bool TrySleepwalk()
    {
        if (_sleepwalkDone || IsSleepwalking) return false;

        _sleepwalkDone = true;
        StartCoroutine(SleepwalkRoutine());
        return true;
    }

    private IEnumerator SleepwalkRoutine()
    {
        IsSleepwalking = true;
        Debug.Log("<color=cyan>Franken เริ่ม Sleepwalk!</color>");

        // เดินออกจากห้องไปที่ target
        if (sleepwalkTarget != null)
            MoveTo(sleepwalkTarget.position, TravelState.WalkToTarget);

        // เริ่มหัก heart ทุกวิ
        _sleepwalkDamageCoroutine = StartCoroutine(SleepwalkDamageRoutine());

        // รอจนกว่า Player จะ click ปลุก (IsSleepwalking จะเป็น false เมื่อ Wake)
        yield return new WaitUntil(() => !IsSleepwalking);

        if (_sleepwalkDamageCoroutine != null)
        {
            StopCoroutine(_sleepwalkDamageCoroutine);
            _sleepwalkDamageCoroutine = null;
        }
        Debug.Log("<color=cyan>Franken ตื่นแล้ว!</color>");
    }

    private IEnumerator SleepwalkDamageRoutine()
    {
        while (IsSleepwalking)
        {
            yield return new WaitForSeconds(1f);
            heart -= heartLossPerSecond;
            Debug.Log($"<color=cyan>Franken sleepwalk heart: {heart}</color>");
            if (heart <= 0) break;
        }
    }

    /// <summary>
    /// Player คลิกที่ตัว Franken เพื่อปลุก — ต้องเชื่อมกับ Interact หรือ OnMouseDown
    /// </summary>
    public void WakeUp()
    {
        if (!IsSleepwalking) return;
        IsSleepwalking = false;
        agent.isStopped = true;
        Debug.Log("<color=cyan>Player ปลุก Franken สำเร็จ!</color>");
    }
}
