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
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
        
        if (hit.collider != null && hit.collider.TryGetComponent<Room>(out var room))
        {
            
            if (room.RoomData.isUnAvailable)
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
                    // ถ้าเป็น Reaper แต่ห้องที่ลากไปใส่ไม่ใช่ RoomType.Big
                    if (room.RoomData.roomType != RoomType.Big)
                    {
                        Debug.Log("<color=red>❌ Reaper ต้องการห้อง Big เท่านั้น!</color>");
                        transform.localPosition = _originalPosition; // ส่งกลับที่เดิม
                        LevelUI levelUI = FindAnyObjectByType<LevelUI>();
                        levelUI.Notify("Need a Big Room Only");
                        return; // หยุดการทำงาน ไม่ให้เข้าห้องนี้
                    }
                }
                SoundManager.Instance.PlaySFX(onDragtoRoom);
                Debug.Log($"<color=green>กำหนดห้องให้แขก {targetGuest.name}</color>");
                room.RoomData.isUnAvailable = true; // จองห้องไว้ก่อน ป้องกัน guest อื่นมาแย่ง
                targetGuest.finalRoomData = room.RoomData;
                targetGuest.StartTravel(room.interactObjData);
                gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("ลากไปตกนอกห้อง");
            transform.localPosition = _originalPosition;
        }
    }
}