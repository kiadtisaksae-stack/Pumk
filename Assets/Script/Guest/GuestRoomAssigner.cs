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

        // หา GuestAI จาก parent ขึ้นไปเรื่อยๆ จนเจอ
        if (targetGuest == null)
            targetGuest = GetComponentInParent<GuestAI>();

        if (targetGuest == null)
            Debug.LogError("GuestRoomAssigner: หา GuestAI ใน parent ไม่เจอ!");
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

        // 1. ใช้ GetRayIntersectionAll เพื่อเก็บ Object ทั้งหมดที่ Ray ลากผ่าน
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray);

        Room targetRoom = null;

        // 2. วนลูปหาว่าในบรรดาที่ Hit มีตัวไหนเป็น Room บ้าง
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.TryGetComponent<Room>(out var room))
            {
                targetRoom = room;
                break; // เจอห้องแล้ว หยุดหาตัวอื่น
            }
        }

        // 3. เช็คเงื่อนไขหลังจากหาห้องเจอ
        if (targetRoom != null)
        {
            if (targetRoom.RoomData.isUnAvailable)
            {
                Debug.Log("❌ ห้องนี้มีแขกแล้ว");
                transform.localPosition = _originalPosition;
                return;
            }

            if (targetGuest != null)
            {
                ReaperGuest reaperGuest = targetGuest.GetComponent<ReaperGuest>();
                if (reaperGuest != null)
                {
                    if (targetRoom.RoomData.roomType != RoomType.Big)
                    {
                        Debug.Log("<color=red>❌ Reaper ต้องการห้อง Big เท่านั้น!</color>");
                        transform.localPosition = _originalPosition;
                        LevelUI levelUI = FindAnyObjectByType<LevelUI>();
                        levelUI.Notify("Need a Big Room Only");
                        return;
                    }
                }

                // --- การทำงานปกติเมื่อเงื่อนไขผ่าน ---
                SoundManager.Instance.PlaySFX(onDragtoRoom);
                targetRoom.RoomData.isUnAvailable = true;
                targetGuest.finalRoomData = targetRoom.RoomData;
                targetGuest.StartTravel(targetRoom.interactObjData);
                gameObject.SetActive(false);
            }
        }
        else
        {
            // ถ้าวนลูปจนจบแล้วยังไม่เจอ Room เลย
            Debug.Log("ลากไปตกนอกห้อง หรือไม่มี Collider ของ Room อยู่ในแนว Ray");
            transform.localPosition = _originalPosition;
        }
    }
}