using System.Collections;
using UnityEngine;

/// <summary>
/// Werewolf
/// - 2 services (luggage + 1 random)
/// - Anger stack drains over time; when empty, punish player inventory.
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
        serviceCount = 2;
        decaysHit = 1.0f;
        _player = FindAnyObjectByType<Player>();
    }

    public override void OnServiceStart(ItemSO service)
    {
        StopAngerStack();
        CurrentAngerBars = maxAngerBars;
        _angerCoroutine = StartCoroutine(AngerStackRoutine());
        guestUI?.OnAngerStart();
    }

    public override void OnServiceSuccess(ItemSO service)
    {
        base.OnServiceSuccess(service);
        StopAngerStack();
    }

    public override void OnServiceFail(ItemSO service, Room room)
    {
        base.OnServiceFail(service, room);
        StopAngerStack();
    }

    public override void OnCheckOut(bool isAnger)
    {
        StopAngerStack();
    }

    private IEnumerator AngerStackRoutine()
    {
        while (CurrentAngerBars > 0)
        {
            yield return new WaitForSeconds(barDrainInterval);
            CurrentAngerBars--;

            if (CurrentAngerBars <= 0)
            {
                TriggerAngerPunish();
            }
        }
    }

    private void TriggerAngerPunish()
    {
        if (_player == null) return;

        _player.ClearNonProtectedItems();
        Debug.Log("<color=orange>Werewolf โกรธ! ล้างเฉพาะไอเท็มทั่วไป (ไม่ลบ Luggage/Laundry)</color>");
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
