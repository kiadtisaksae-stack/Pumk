using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GuestUIController : MonoBehaviour
{
    [Header("Service Bubble")]
    public GameObject serviceBubble;
    public Image serviceIcon;

    [Header("Heart Bar")]
    public GameObject heartBarGroup;
    public Image heartBarFill;

    [Header("Anger Bars (Werewolf)")]
    public GameObject angerBarGroup;
    public Image[] angerBarSlots;

    [Header("Sleep Indicator (Franken)")]
    public GameObject sleepIndicator;

    [Header("Delivery Count")]
    public TextMeshProUGUI deliveryCountText;  // แสดง x2, x3 ... ซ่อนเมื่อ <=1

    private GuestAI _guest;
    private WerewolfGuest _werewolf;
    private FrankenGuest _franken;

    private float _maxHeart;
    private Vector3 _bubbleOriginalScale;
    private bool _uiActive = false;  // เปิด UI เฉพาะหลัง check-in

    // ─────────────────────────────────────────────
    //  Init  (GuestAI.Start เรียก)
    // ─────────────────────────────────────────────

    public virtual void Init(GuestAI guest)
    {
        _guest = guest;
        _werewolf = guest as WerewolfGuest;
        _franken = guest as FrankenGuest;
        _maxHeart = guest.heart;

        // ซ่อนทุกอย่างตั้งแต่ต้น
        if (serviceBubble != null) serviceBubble.SetActive(false);
        if (heartBarGroup != null) heartBarGroup.SetActive(false);
        if (angerBarGroup != null) angerBarGroup.SetActive(false);
        if (sleepIndicator != null) sleepIndicator.SetActive(false);
        if (deliveryCountText != null) deliveryCountText.text = "";

        if (serviceBubble != null)
            _bubbleOriginalScale = serviceBubble.transform.localScale;
    }

    // ─────────────────────────────────────────────
    //  Lifecycle hooks  (GuestAI เรียก)
    // ─────────────────────────────────────────────

    /// <summary>เรียกตอน guest เข้าห้อง → เปิด heart bar (และ anger ถ้าเป็น Werewolf)</summary>
    public virtual void OnCheckIn()
    {
        _uiActive = true;

        if (heartBarGroup != null)
        {
            heartBarGroup.SetActive(true);
            heartBarGroup.transform.DOKill();
            heartBarGroup.transform.localScale = Vector3.zero;
            heartBarGroup.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        }

        // Anger bar เปิดเฉพาะ Werewolf และจะเปิดอีกครั้งตอน OnServiceStart
        if (_werewolf != null && angerBarGroup != null)
            angerBarGroup.SetActive(false);  // รอให้ service เริ่มก่อน
    }

    /// <summary>เรียกตอน guest check-out → ซ่อนทุกอย่าง</summary>
    public virtual void OnCheckOut()
    {
        _uiActive = false;
        HideBubble();
        if (heartBarGroup != null) heartBarGroup.SetActive(false);
        if (angerBarGroup != null) angerBarGroup.SetActive(false);
        if (sleepIndicator != null) sleepIndicator.SetActive(false);
        if (deliveryCountText != null) deliveryCountText.text = "";
    }

    /// <summary>Werewolf เรียกตอน service เริ่ม → เปิด anger bar</summary>
    public virtual void OnAngerStart()
    {
        if (angerBarGroup == null) return;
        angerBarGroup.SetActive(true);
        angerBarGroup.transform.DOKill();
        angerBarGroup.transform.localScale = Vector3.zero;
        angerBarGroup.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
    }

    /// <summary>Werewolf เรียกตอน service จบ → ซ่อน anger bar</summary>
    public virtual void OnAngerEnd()
    {
        if (angerBarGroup == null) return;
        angerBarGroup.transform.DOKill();
        angerBarGroup.transform
            .DOScale(Vector3.zero, 0.15f)
            .SetEase(Ease.InBack)
            .OnComplete(() => angerBarGroup.SetActive(false));
    }

    // ─────────────────────────────────────────────
    //  Update
    // ─────────────────────────────────────────────

    private void Update()
    {
        if (!_uiActive || _guest == null) return;
        UpdateHeartBar();
        UpdateAngerBars();
        UpdateSleepIndicator();
    }

    private void UpdateHeartBar()
    {
        if (heartBarFill == null) return;
        float ratio = Mathf.Clamp01(_guest.heart / _maxHeart);
        heartBarFill.fillAmount = ratio;

        // เขียว → เหลือง → แดง
        heartBarFill.color = ratio > 0.5f
            ? Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f)
            : Color.Lerp(Color.red, Color.yellow, ratio * 2f);
    }

    private void UpdateAngerBars()
    {
        if (_werewolf == null || angerBarSlots == null) return;
        if (!angerBarGroup.activeSelf) return;

        for (int i = 0; i < angerBarSlots.Length; i++)
        {
            if (angerBarSlots[i] == null) continue;
            bool active = i < _werewolf.CurrentAngerBars;
            angerBarSlots[i].color = active && _werewolf.CurrentAngerBars == 1
                ? Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 4f, 1f))
                : active ? new Color(1f, 0.35f, 0f) : new Color(0.3f, 0.3f, 0.3f);
        }
    }

    private void UpdateSleepIndicator()
    {
        if (_franken == null || sleepIndicator == null) return;
        bool sleeping = _franken.IsSleepwalking;
        if (sleepIndicator.activeSelf != sleeping)
            sleepIndicator.SetActive(sleeping);
    }

    // ─────────────────────────────────────────────
    //  Bubble API
    // ─────────────────────────────────────────────

    public void ShowBubble(ItemSO service)
    {
        if (serviceBubble == null) return;
        if (serviceIcon != null) serviceIcon.sprite = service.itemIcon;

        serviceBubble.transform.DOKill();
        serviceBubble.transform.localScale = Vector3.zero;
        serviceBubble.SetActive(true);
        serviceBubble.transform
            .DOScale(_bubbleOriginalScale, 0.25f)
            .SetEase(Ease.OutBack);
    }

    public void HideBubble()
    {
        if (serviceBubble == null) return;
        serviceBubble.transform.DOKill();
        serviceBubble.transform
            .DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => serviceBubble.SetActive(false));
    }

    // ─────────────────────────────────────────────
    //  Delivery Count
    // ─────────────────────────────────────────────

    /// <summary>count > 1 → แสดง "x{count}" / count <= 1 หรือ 0 → ล้าง text</summary>
    public void SetDeliveryCount(int count)
    {
        if (deliveryCountText == null) return;
        deliveryCountText.text = count > 1 ? $"x{count}" : "";
    }
}