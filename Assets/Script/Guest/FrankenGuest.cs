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

    private bool _sleepwalkDone;
    private Coroutine _sleepwalkDamageCoroutine;
    private FFSleepwalk _activePoint;
    private bool _isReturningToRoom;

    // Save room target before sleepwalk so we can return first, then resume services.
    private InteractObjData _savedRoomTarget;

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
        _isReturningToRoom = false;
    }

    protected override IEnumerator OnBetweenServices(int completedSlotIndex)
    {
        if (_sleepwalkDone || IsSleepwalking || _isReturningToRoom) yield break;

        float delay = Random.Range(0f, maxExtraDelay);
        if (delay > 0f) yield return new WaitForSeconds(delay);

        _sleepwalkDone = true;
        yield return SleepwalkRoutine();
    }

    private IEnumerator SleepwalkRoutine()
    {
        IsSleepwalking = true;

        // Restore visuals before walking out (they may be hidden after room-entry animation).
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

        if (_activePoint != null)
        {
            _activePoint.owner = null;
            _activePoint.gameObject.SetActive(false);
            _activePoint = null;
        }

        if (_savedRoomTarget != null)
        {
            _isReturningToRoom = true;
            StartTravel(_savedRoomTarget);
            yield return ReturnToRoomThenHideVisualRoutine(_savedRoomTarget);
            _isReturningToRoom = false;
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

    public void WakeUp()
    {
        if (!IsSleepwalking) return;

        IsSleepwalking = false;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        heart = 5f;
    }

    private IEnumerator ReturnToRoomThenHideVisualRoutine(InteractObjData roomTarget)
    {
        if (roomTarget == null || roomTarget.ObjPosition == null)
        {
            yield break;
        }

        // Let NavMeshAgent receive the destination first.
        yield return null;

        float timeout = 16f;
        float arriveTolerance = Mathf.Max(0.2f, agent.stoppingDistance + 0.15f);
        bool reachedRoom = false;

        while (timeout > 0f)
        {
            if (isExit) yield break;

            float distance = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(roomTarget.ObjPosition.position.x, roomTarget.ObjPosition.position.y));
            bool reachedByDistance = distance <= arriveTolerance;

            if (reachedByDistance)
            {
                reachedRoom = true;
                break;
            }

            if (!agent.pathPending)
            {
                bool reachedByAgent = agent.remainingDistance <= arriveTolerance &&
                                      agent.velocity.sqrMagnitude <= 0.01f;
                if (reachedByAgent)
                {
                    reachedRoom = true;
                    break;
                }
            }

            timeout -= Time.deltaTime;
            yield return null;
        }

        if (!reachedRoom)
        {
            // Fallback: avoid hard-stalling the service loop forever.
            Debug.LogWarning($"{name}: ReturnToRoom timed out before precise arrival.");
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        travelState = TravelState.stayRoom;

        // Force same punch(yoyo) + hide sequence as first room entry.
        PlayReEnterRoomVisualSequence();
        yield return new WaitForSeconds(0.9f);
    }

    private void PlayReEnterRoomVisualSequence()
    {
        List<GameObject> visuals = GetCharacterVisuals();
        if (visuals.Count == 0) return;

        if (_originalVisualScales.Count != visuals.Count || _originalVisualScales.Count == 0)
        {
            CacheCharacterVisualScales();
            visuals = GetCharacterVisuals();
        }

        for (int i = 0; i < visuals.Count; i++)
        {
            GameObject visual = visuals[i];
            if (visual == null) continue;

            Vector3 scale = i < _originalVisualScales.Count ? _originalVisualScales[i] : _originalScale;
            Transform visualTransform = visual.transform;

            visual.SetActive(true);
            visualTransform.DOKill();
            visualTransform.localScale = scale;
            visualTransform
                .DOPunchScale(scale * 0.2f, 0.3f, 5, 1f)
                .OnComplete(() =>
                {
                    if (visualTransform == null) return;
                    visualTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);
                });
        }
    }
}
