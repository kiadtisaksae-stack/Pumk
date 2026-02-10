using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

[System.Serializable]
public class EmplyeeEvent : UnityEvent<Emplyee> { }
public class InteractObjWalkTo : MonoBehaviour , IPointerClickHandler
{
    private ElevatorController hotelElevator;
    private Emplyee targetEmplyee;

    public EmplyeeEvent onTravel;

    [Header("Visual Settings")]
    [SerializeField] private float bounceAmount = 0.5f;
    [SerializeField] private float duration = 0.25f;
    private Vector3 originalScale;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetEmplyee = FindAnyObjectByType<Emplyee>();
        hotelElevator = FindAnyObjectByType<ElevatorController>();
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ProcessAssignment(eventData);
        }
    }

    private void ProcessAssignment(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null && hit.collider.TryGetComponent<CanInteractObj>(out var obj))
        {
            Tweening();
            if (targetEmplyee != null)
            {
                Debug.Log($"<color=green>พนักงาน {targetEmplyee.name}</color> เดินทาง");

                onTravel.Invoke(targetEmplyee);
                targetEmplyee.StartTravel(obj.interactObjData, hotelElevator);
            }
        }
        else
        {
            Debug.Log("ลากไปตกนอกห้อง");
            //transform.localPosition = _originalPosition;
        }
    }
    public void Tweening()
    {
        // ล้าง Tween เก่า (ถ้ามี) เพื่อไม่ให้บัคเวลาคลิกรัวๆ
        transform.DOKill();
        transform.localScale = originalScale;

        // เล่นเอฟเฟกต์เด้งแบบ Yoyo
        transform.DOPunchScale(new Vector3(bounceAmount, bounceAmount, 0), duration, 5, 1f)
            .OnComplete(() => transform.localScale = originalScale);

        Debug.Log($"[Castle] {gameObject.name} was interacted!");

        // ตรงนี้สามารถใส่ logic เพิ่มเติมได้ เช่น เปิดหน้าต่าง MarketManager
    }
}
