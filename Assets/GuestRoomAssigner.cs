using UnityEngine;
using UnityEngine.EventSystems;

public class GuestRoomAssigner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private GuestAI targetGuest;
    private ElevatorController hotelElevator;

    [Header("Settings")]
    [SerializeField] private float dragAlpha = 0.6f;
    [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);

    private Vector3 _originalPosition;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _originalPosition = transform.localPosition;
        hotelElevator = FindObjectOfType<ElevatorController>();
        _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        if (targetGuest == null)
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
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null && hit.collider.TryGetComponent<RoomData>(out var room))
        {
            if (targetGuest != null)
            {
                Debug.Log($"<color=green>กำหนดห้องให้แขก {targetGuest.name}</color>");
                targetGuest.StartTravel(room, hotelElevator);
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("GuestRoomAssigner: targetGuest เป็น null!");
            }
        }
        else
        {
            Debug.Log("GuestRoomAssigner: ลากไปตกนอกห้อง กลับสู่ตำแหน่งเดิม");
            transform.localPosition = _originalPosition;
        }
    }
}