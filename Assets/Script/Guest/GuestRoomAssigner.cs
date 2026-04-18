using UnityEngine;
using UnityEngine.EventSystems;

public class GuestRoomAssigner : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private GuestAI targetGuest;

    [Header("Settings")]
    [SerializeField] private float dragAlpha = 0.6f;
    [SerializeField] private Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);

    [Header("Sound")]
    public AudioClip onBeginDrag;
    public AudioClip onDragtoRoom;

    private Vector3 _originalPosition;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _originalPosition = transform.localPosition;
        _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        if (targetGuest == null)
            targetGuest = GetComponentInParent<GuestAI>();

        if (targetGuest == null)
            Debug.LogError("GuestRoomAssigner: cannot find GuestAI in parent.");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySFX(onBeginDrag);
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

        Room targetRoom = null;
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent<Room>(out var room))
            {
                targetRoom = room;
                break;
            }
        }

        if (targetRoom == null)
        {
            transform.localPosition = _originalPosition;
            return;
        }

        if (targetRoom.RoomData.isUnAvailable)
        {
            transform.localPosition = _originalPosition;
            return;
        }

        if (targetGuest == null)
        {
            transform.localPosition = _originalPosition;
            return;
        }

        // Check room-size requirement here (before guest starts moving).
        if (!targetGuest.CanAssignToRoom(targetRoom))
        {
            transform.localPosition = _originalPosition;
            LevelUI levelUI = FindAnyObjectByType<LevelUI>();
            if (levelUI != null) levelUI.Notify(targetGuest.GetRoomRequirementFailMessage());
            Debug.Log("<color=red>Room type not allowed for this guest.</color>");
            return;
        }

        SoundManager.Instance.PlaySFX(onDragtoRoom);
        targetRoom.RoomData.isUnAvailable = true;
        targetGuest.finalRoomData = targetRoom.RoomData;
        targetGuest.StartTravel(targetRoom.interactObjData);
        gameObject.SetActive(false);
    }
}
