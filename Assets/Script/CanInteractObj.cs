using NUnit.Framework.Interfaces;
using UnityEngine;
using DG.Tweening;
[RequireComponent(typeof(Collider2D))]

public class CanInteractObj : MonoBehaviour ,IInteractable
{
    [Header("Visual Settings")]
    [SerializeField] private float bounceAmount = 0.5f;
    [SerializeField] private float duration = 0.25f;
    private Vector3 originalScale;
    [SerializeField]
    private InteractObjData _interactObjData;
    public InteractObjData interactObjData => _interactObjData;

    public AudioClip objSFX;

    protected virtual void Awake()
    {
        interactObjData.ObjPosition = this.transform;
        interactObjData.objCollider = GetComponent<Collider2D>();
        interactObjData.objCollider.isTrigger = true;
        
    }
    public virtual void Start()
    {
        originalScale = transform.localScale;
    }
    public void Tweening()
    {
        // ล้าง Tween เก่า (ถ้ามี) เพื่อไม่ให้บัคเวลาคลิกรัวๆ
        transform.DOKill();
        transform.localScale = originalScale;

        // เล่นเอฟเฟกต์เด้งแบบ Yoyo
        transform.DOPunchScale(new Vector3(bounceAmount, bounceAmount, 0), duration, 5, 1f)
            .OnComplete(() => transform.localScale = originalScale);

        Debug.Log($"[กดที่] {gameObject.name} was interacted!");

        // ตรงนี้สามารถใส่ logic เพิ่มเติมได้ เช่น เปิดหน้าต่าง MarketManager
    }
    public virtual void Interact(MoveHandleAI actor)
    {
        SoundManager.Instance.PlaySFX(objSFX);
        Tweening();
    }



}
