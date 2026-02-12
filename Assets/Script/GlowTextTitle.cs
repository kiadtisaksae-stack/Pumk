using UnityEngine;
using TMPro;
using DG.Tweening;

public class GlowTextTitle : MonoBehaviour
{
    public TextMeshProUGUI titleText;

    [Header("Glow Settings")]
    public float fadeDuration = 1f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;

    void Start()
    {
        // กัน null
        if (titleText == null)
            titleText = GetComponent<TextMeshProUGUI>();

        StartGlow();
    }

    void StartGlow()
    {
        // เริ่มที่สว่างสุด
        titleText.alpha = maxAlpha;
        // Loop Fade In-Out
        titleText.DOFade(minAlpha, fadeDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}