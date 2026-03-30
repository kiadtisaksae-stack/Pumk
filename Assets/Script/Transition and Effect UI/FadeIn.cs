using System.Collections;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    public float duration = 1.0f; // ระยะเวลาที่ต้องการให้ Fade
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // ฟังก์ชันนี้จะทำงานทุกครั้งที่ Object ถูกสั่ง SetActive(true)
    private void OnEnable()
    {
        StartCoroutine(DoFadeIn());
    }

    IEnumerator DoFadeIn()
    {
        float counter = 0;
        canvasGroup.alpha = 0;

        while (counter < duration)
        {
            counter += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, counter / duration);
            yield return null;
        }
    }
}
