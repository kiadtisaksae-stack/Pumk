using UnityEngine;
using DG.Tweening;

public class UIbounceOnActibe : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 0.5f;
    public Ease easeType = Ease.OutBack; // OutBack จะเด้งเกินแล้วหดกลับ 

    private void OnEnable()
    {
        // 1. ตั้งค่า Scale เริ่มต้นเป็น 0 (หดตัว)
        transform.localScale = Vector3.zero;

        // 2. สั่งให้เด้งออกมา (ขยายไปที่ 1)
        // ใช้ SetUpdate(true) เพื่อให้เล่นได้แม้จะกด Pause เกมอยู่
        transform.DOScale(Vector3.one, duration)
                 .SetEase(easeType)
                 .SetUpdate(true);
    }

    private void OnDisable()
    {
        // เคลียร์ Tween ทิ้งเมื่อ Object ปิด เพื่อป้องกัน Memory Leak หรือ Error
        transform.DOKill();
    }
}
