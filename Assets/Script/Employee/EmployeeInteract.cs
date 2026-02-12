using UnityEngine;
using UnityEngine.EventSystems;


public class EmployeeInteract : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private Employee targetEmployee;

    [Header("Settings")]
    [SerializeField] private float dragAlpha = 0.6f;
    [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);

    private Vector3 _originalPosition;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _originalPosition = transform.localPosition;
        _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        if (targetEmployee == null)
            Debug.LogError("GuestRoomAssigner: ไม่ได้กำหนด targetGuest!");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _canvasGroup.alpha = dragAlpha;
        _canvasGroup.blocksRaycasts = false;
        transform.localScale = hoverScale;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPoint;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            (RectTransform)transform.parent,
            eventData.position,
            eventData.pressEventCamera,
            out worldPoint
        );

        transform.position = worldPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
        transform.localScale = Vector3.one;

        ProcessAssignment(eventData);
    }

    private void ProcessAssignment(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray);

        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            // ข้าม Player
            if (hit.collider.CompareTag("Player"))
                continue;
            // ข้าม Employee ด้วย
            if (hit.collider.CompareTag("Employee"))
                continue;
            // เจอ Object ที่ Interact ได้
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(targetEmployee);
                // กลับตำแหน่ง icon
                transform.localPosition = _originalPosition;
                return;
            }
        }
        // ❌ ถ้าไม่เจออะไรเลย
        Debug.Log("ลากไปตกนอกห้อง");
        transform.localPosition = _originalPosition;
    }
}
