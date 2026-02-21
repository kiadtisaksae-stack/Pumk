using TMPro;
using UnityEngine;
using DG.Tweening;
public class Counter : CanInteractObj
{
    public TextMeshProUGUI BounceText;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<GuestAI>(out var guest))
        {
            if (guest.guestPhase != Guestphase.CheckingOut) return;
            Debug.Log(guest + " check out get Money " + guest.rentNet);
            LevelManager gameManager = FindAnyObjectByType<LevelManager>();
            ShowMoneyPopup(guest.rentNet);
            // เพิ่มเงิน
            gameManager.AddMoney(guest.rentNet);
            gameManager.AddServicePoint(guest.servicePoint);
            // แสดง popup
            
            Destroy(guest.gameObject);
        }
    }

    private void ShowMoneyPopup(int amount)
    {
        if (BounceText == null) return;

        BounceText.gameObject.SetActive(true);

        BounceText.text = $"+{amount}";
        BounceText.alpha = 1f;

        Transform t = BounceText.transform;

        Vector3 startPos = t.localPosition;
        Vector3 baseScale = t.localScale;

        t.DOKill(true);

        Sequence seq = DOTween.Sequence();

        // ลอยขึ้น
        seq.Append(t.DOLocalMoveY(startPos.y + 0.5f, 1f)
            .SetEase(Ease.OutQuad));

        // Fade out
        seq.Join(BounceText.DOFade(0f, 1f));

        // เด้ง scale แบบไม่พังสัดส่วน
        seq.Insert(0f, t.DOScale(baseScale * 1.2f, 0.2f)
            .SetEase(Ease.OutBack));

        seq.OnComplete(() =>
        {
            BounceText.text = "";
            t.localPosition = startPos;
            t.localScale = baseScale;
        });
    }
}
