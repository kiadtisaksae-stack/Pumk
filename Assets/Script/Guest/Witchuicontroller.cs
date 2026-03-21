using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// extend GuestUIController สำหรับ WitchGuest โดยเพิ่ม cat bubble
/// ติดแทน GuestUIController บน WitchGuest Prefab
/// </summary>
public class WitchUIController : GuestUIController
{
    [Header("Cat Bubble")]
    public GameObject catServiceBubble;
    public Image catServiceIcon;

    private Vector3 _catBubbleOriginalScale;

    public new void Init(GuestAI guest)
    {
        base.Init(guest);

        if (catServiceBubble != null)
        {
            _catBubbleOriginalScale = catServiceBubble.transform.localScale;
            catServiceBubble.SetActive(false);
        }
    }

    public void ShowCatBubble(ItemSO service)
    {
        if (catServiceBubble == null) return;
        if (catServiceIcon != null) catServiceIcon.sprite = service.itemIcon;

        catServiceBubble.transform.DOKill();
        catServiceBubble.transform.localScale = Vector3.zero;
        catServiceBubble.SetActive(true);
        catServiceBubble.transform
            .DOScale(_catBubbleOriginalScale, 0.25f)
            .SetEase(Ease.OutBack);
    }

    public void HideCatBubble()
    {
        if (catServiceBubble == null) return;
        catServiceBubble.transform.DOKill();
        catServiceBubble.transform
            .DOScale(Vector3.zero, 0.2f)
            .SetEase(Ease.InBack)
            .OnComplete(() => catServiceBubble.SetActive(false));
    }
}