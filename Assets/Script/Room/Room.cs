using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Room : CanInteractObj
{
    [SerializeField]
    private RoomData roomData;
    public RoomData RoomData => roomData;

    [Header("Interaction")]
    public Button roomServiceButton;
    public Button upgradeRoomButton;

    [SerializeField]
    private GuestAI guestInRoom;
    public ServiceManager serviceManager;

    private void Awake()
    {
        if (upgradeRoomButton != null)
            upgradeRoomButton.onClick.AddListener(UpgradeRoom);

        if (roomServiceButton != null)
            roomServiceButton.onClick.AddListener(StartService);

        UpdateUpgradeButton();
    }
    private void Start()
    {
        roomServiceButton.gameObject.SetActive(false);
        serviceManager = GetComponent<ServiceManager>();
    }
    private void Update()
    {
       if (guestInRoom == null) return;
       float distSqr = (guestInRoom.transform.position - transform.position).sqrMagnitude;

        // เอาค่าระยะที่ต้องการมายกกำลังสองก่อนเทียบ
        if (distSqr <= (0.2 * 0.2))
        {
            Debug.Log("ถึงห้องแล้ววว");
            roomData.isAvailable = true;
            StartService();
            return;
        }
    }
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (!collision.TryGetComponent(out GuestAI guest)) return;
    //    if (roomData.isAvailable)
    //    {
    //        return;
    //    }
    //    roomData.isAvailable = true;
    //    guestInRoom = guest;
    //    roomServiceButton.gameObject.SetActive(true);
          
    //}

    public void AssignGuest(GuestAI guest)
    {
        guestInRoom = guest;
    }

    public void ClearRoom()
    {
        roomData.isAvailable = false;
        roomData.currentServiceRequest = null;
    }
    public void UpgradeRoom()
    {
        if (roomData.isAvailable)
        {
            Debug.Log("❌ ไม่สามารถอัปเกรดได้ ขณะมีแขก");
            return;
        }

        if (roomData.roomLevel == RoomLevel.Suite)
        {
            Debug.Log("❌ ห้องนี้เป็น Suite แล้ว");
            return;
        }

        roomData.roomLevel++;
        Debug.Log($"⬆️ อัปเกรดห้อง {name} เป็น {roomData.roomLevel}");

        UpdateUpgradeButton();
    }
    private void UpdateUpgradeButton()
    {
        if (upgradeRoomButton == null) return;

        upgradeRoomButton.interactable =
            !roomData.isAvailable &&
            roomData.roomLevel != RoomLevel.Suite;
    }
    public void StartService()
    {
        guestInRoom.TriggerEvents(Guestphase.InRoom);
        guestInRoom.RequestService(serviceManager);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position);
    }
}
