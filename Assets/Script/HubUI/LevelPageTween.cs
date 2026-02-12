using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelPageTween : MonoBehaviour
{
    [Header("Cards")]
    public List<RectTransform> levelCards = new List<RectTransform>();

    [Header("Center Point")]
    public RectTransform centerPoint;

    [Header("Tween Settings")]
    public float moveDuration = 0.5f;
    public float fadeDuration = 0.3f;
    public float slideOffset = 500f; // ระยะห่างที่ Card จะเริ่มวิ่งเข้ามา
    public Vector3 startScale = new Vector3(0.5f, 0.5f, 0.5f); // ขนาดเริ่มต้นก่อนขยาย

    private int currentIndex = 0;
    private bool isAnimating = false;
    public Button NextButton;
    public Button PreviousButton;

    void Start()
    {
        SetCardToCenterInstant();
        NextButton.onClick.AddListener(NextLevel);
        PreviousButton.onClick.AddListener(PreviousLevel);
    }

    public void NextLevel()
    {
        if (isAnimating || currentIndex + 1 >= levelCards.Count) return;
        SwitchTo(currentIndex + 1, true); // true = ไปข้างหน้า (มาเจากขวา)
    }

    public void PreviousLevel()
    {
        if (isAnimating || currentIndex - 1 < 0) return;
        SwitchTo(currentIndex - 1, false); // false = ย้อนกลับ (มาจากซ้าย)
    }

    void SwitchTo(int newIndex, bool isNext)
    {
        isAnimating = true;

        RectTransform oldCard = levelCards[currentIndex];
        RectTransform newCard = levelCards[newIndex];

        CanvasGroup oldGroup = GetOrAddCanvasGroup(oldCard);
        CanvasGroup newGroup = GetOrAddCanvasGroup(newCard);

        // จัดการ Card เก่า (Fade Out + Scale Down + Slide Out)
        float exitDirection = isNext ? -slideOffset : slideOffset; 

        oldGroup.DOFade(0f, fadeDuration);
        oldCard.DOAnchorPosX(exitDirection, moveDuration).SetEase(Ease.InCubic);
        oldCard.DOScale(startScale, moveDuration).SetEase(Ease.InCubic);

        // จัดการ Card ใหม่ (เตรียมตัวก่อนเข้า) 
        float entryDirection = isNext ? slideOffset : -slideOffset; 

        newCard.anchoredPosition = new Vector2(entryDirection, 0); // วางตำแหน่งเริ่มต้น
        newCard.localScale = startScale; // เริ่มที่ตัวเล็ก
        newGroup.alpha = 0f;
        newCard.gameObject.SetActive(true);

        // เล่น Animation ตัวใหม่ (Fade In + Scale Up + Slide In)
        newGroup.DOFade(1f, moveDuration);
        newCard.DOAnchorPos(Vector2.zero, moveDuration).SetEase(Ease.OutBack); // OutBack จะช่วยให้ดูมีแรงเด้งนิดๆ
        newCard.DOScale(Vector3.one, moveDuration).SetEase(Ease.OutBack);

        currentIndex = newIndex;

        // ปลดล็อคสถานะเมื่อจบ
        DOVirtual.DelayedCall(moveDuration, () =>
        {
            isAnimating = false;
            // ซ่อนตัวเก่าเพื่อประหยัด Resource
            foreach (var card in levelCards)
            {
                if (card != newCard) card.gameObject.SetActive(false);
            }
        });
    }

    void SetCardToCenterInstant()
    {
        for (int i = 0; i < levelCards.Count; i++)
        {
            CanvasGroup group = GetOrAddCanvasGroup(levelCards[i]);
            if (i == currentIndex)
            {
                levelCards[i].anchoredPosition = Vector2.zero;
                levelCards[i].localScale = Vector3.one;
                group.alpha = 1f;
                levelCards[i].gameObject.SetActive(true);
            }
            else
            {
                group.alpha = 0f;
                levelCards[i].gameObject.SetActive(false);
            }
        }
    }

    CanvasGroup GetOrAddCanvasGroup(RectTransform rect)
    {
        CanvasGroup cg = rect.GetComponent<CanvasGroup>();
        if (cg == null) cg = rect.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }
}