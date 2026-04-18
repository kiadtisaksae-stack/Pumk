using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class FrankenGuest : GuestAI
{
    [Header("Sleepwalk Settings")]
    public float heartLossPerSecond = 0.3f;
    public float maxExtraDelay = 5f;

    [HideInInspector] public List<FFSleepwalk> sleepwalkPoints = new List<FFSleepwalk>();

    public bool IsSleepwalking { get; private set; } = false;

    private bool _sleepwalkDone = false;
    private Coroutine _sleepwalkDamageCoroutine;
    private FFSleepwalk _activePoint;

    // เก็บ targetIObj ของห้องไว้ก่อน sleepwalk เพื่อกลับมาหลังตื่น
    private InteractObjData _savedRoomTarget;

    // ─────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────

    public override void Start()
    {
        base.Start();
        serviceCount = 2;
        decaysHit = 0.5f;

        sleepwalkPoints.Clear();
        foreach (var sw in Object.FindObjectsByType<FFSleepwalk>(FindObjectsSortMode.None))
        {
            sleepwalkPoints.Add(sw);
            sw.gameObject.SetActive(false);
        }
    }

    public override void OnCheckIn()
    {
        _sleepwalkDone = false;
        _activePoint = null;
        _savedRoomTarget = null;
    }

    // ─────────────────────────────────────────────
    //  Hook: cooldown หลัง service ชิ้นแรก
    // ─────────────────────────────────────────────

    protected override IEnumerator OnBetweenServices(int completedSlotIndex)
    {
        if (_sleepwalkDone || IsSleepwalking) yield break;

        float delay = Random.Range(0f, maxExtraDelay);
        if (delay > 0f) yield return new WaitForSeconds(delay);

        _sleepwalkDone = true;
        yield return SleepwalkRoutine();
    }

    // ─────────────────────────────────────────────
    //  Sleepwalk internals
    // ─────────────────────────────────────────────

    private IEnumerator SleepwalkRoutine()
    {
        IsSleepwalking = true;


        // คืน scale visual ก่อนเดิน เพราะ AnimateEnterRoom DOScale(zero) ค้างไว้
        List<GameObject> visuals = GetCharacterVisuals();
        if (visuals.Count > 0)
        {
            if (_originalVisualScales.Count != visuals.Count)
            {
                CacheCharacterVisualScales();
                visuals = GetCharacterVisuals();
            }

            for (int i = 0; i < visuals.Count; i++)
            {
                GameObject visual = visuals[i];
                if (visual == null) continue;

                Vector3 scale = i < _originalVisualScales.Count ? _originalVisualScales[i] : _originalScale;
                visual.transform.DOKill();
                visual.transform.localScale = scale;
                visual.SetActive(true);
            }
        }

        // บันทึก targetIObj ของห้องปัจจุบันก่อน override
        _savedRoomTarget = targetIObj;

        if (sleepwalkPoints.Count > 0)
        {
            _activePoint = sleepwalkPoints[Random.Range(0, sleepwalkPoints.Count)];
            _activePoint.owner = this;
            _activePoint.gameObject.SetActive(true);

            StartTravel(_activePoint.interactObjData);
        }

        _sleepwalkDamageCoroutine = StartCoroutine(SleepwalkDamageRoutine());

        yield return new WaitUntil(() => !IsSleepwalking);

        if (_sleepwalkDamageCoroutine != null)
        {
            StopCoroutine(_sleepwalkDamageCoroutine);
            _sleepwalkDamageCoroutine = null;
        }

        // ปิด point
        if (_activePoint != null)
        {
            _activePoint.owner = null;
            _activePoint.gameObject.SetActive(false);
            _activePoint = null;
        }



        // กลับห้องเดิม
        if (_savedRoomTarget != null)
        {
            StartTravel(_savedRoomTarget);
            _savedRoomTarget = null;
        }
    }

    private IEnumerator SleepwalkDamageRoutine()
    {
        while (IsSleepwalking)
        {
            yield return new WaitForSeconds(1f);
            heart -= heartLossPerSecond;

            if (heart <= 0) break;
        }
    }

    // ─────────────────────────────────────────────
    //  WakeUp  (FFSleepwalk.OnTriggerEnter2D เรียก)
    // ─────────────────────────────────────────────

    public void WakeUp()
    {
        if (!IsSleepwalking) return;
        IsSleepwalking = false;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        heart = 5f; // ฟื้นเลือดกลับมาเต็ม

    }
}
