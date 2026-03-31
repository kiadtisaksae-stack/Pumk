using System.Collections;
using UnityEngine;

/// <summary>
/// Werewolf — The Furious Customer
/// - 2 items (Luggage fixed + 1 random: Food/Drink/Soul)  ← spec: 2 items
/// - Heart: 5, decays -1.0 every 3s
/// - EVENT: Anger Stack — 3 bars, drains 1 bar every 2.5s per request
///   Stack resets at each new request
///   Full stack loss = ClearItem() on Player inventory
/// servicePool ใน Inspector: Food, Drink, Soul
/// </summary>
public class WerewolfGuest : GuestAI
{
    [Header("Anger Stack Settings")]
    public int maxAngerBars = 3;
    public float barDrainInterval = 2.5f;

    public int CurrentAngerBars { get; private set; }

    private Coroutine _angerCoroutine;
    private Player _player;

    public override void Start()
    {
        base.Start();
        serviceCount = 2;       // Luggage + 1 random = 2 items
        decaysHit = 1.0f;
        _player = FindAnyObjectByType<Player>();
    }

    // ── เริ่ม Anger Stack ใหม่ทุก request ──
    public override void OnServiceStart(ItemSO service)
    {
        StopAngerStack();
        CurrentAngerBars = maxAngerBars;
        _angerCoroutine = StartCoroutine(AngerStackRoutine());
        guestUI?.OnAngerStart();
        Debug.Log($"<color=orange>Werewolf: Anger Stack เริ่ม ({maxAngerBars} bars)</color>");
    }

    public override void OnServiceSuccess(ItemSO service)
    {
        base.OnServiceSuccess(service);
        StopAngerStack();
    }

    public override void OnServiceFail(ItemSO service,Room room)
    {
        base.OnServiceFail(service,room);
        StopAngerStack();
    }

    public override void OnCheckOut(bool isAnger) => StopAngerStack();

    // ─────────────────────────────────────────────
    //  Anger internals
    // ─────────────────────────────────────────────

    private IEnumerator AngerStackRoutine()
    {
        while (CurrentAngerBars > 0)
        {
            yield return new WaitForSeconds(barDrainInterval);
            CurrentAngerBars--;
            Debug.Log($"<color=orange>Werewolf Anger: {CurrentAngerBars}/{maxAngerBars}</color>");

            if (CurrentAngerBars <= 0)
                TriggerAngerPunish();
        }
    }

    private void TriggerAngerPunish()
    {
        if (_player != null)
        {
            _player.ClearItem();
            Debug.Log("<color=orange>Werewolf โกรธ! ล้าง inventory ของ Player</color>");
        }
    }

    private void StopAngerStack()
    {
        if (_angerCoroutine != null)
        {
            StopCoroutine(_angerCoroutine);
            _angerCoroutine = null;
        }
        CurrentAngerBars = 0;
        guestUI?.OnAngerEnd();
    }
}