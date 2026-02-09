using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Room : CanInteractObj
{
    [SerializeField]
    private RoomData roomData;
    public RoomData RoomData => roomData;

    [Header("Interaction")]

    public Button upgradeRoomButton;

    [SerializeField]
    private GuestAI guestInRoom;
    public ServiceManager serviceManager;

    protected override void Awake()
    {
        base.Awake();
        if (upgradeRoomButton != null)
            upgradeRoomButton.onClick.AddListener(UpgradeRoom);

        ////if (roomServiceButton != null)
        ////    roomServiceButton.onClick.AddListener(StartService);

        UpdateUpgradeButton();
    }
    private void Start()
    {
        serviceManager = GetComponent<ServiceManager>();
    }
    protected override void Update()
    {
        GuestCheckDistance();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player entered the zone!");
            Emplyee emplyee = collision.GetComponent<Emplyee>();
            if (guestInRoom != null)
            {
                serviceManager.RequestCheck(guestInRoom.currentService, emplyee.inventory);
            }
            interactObjData.objCollider.isTrigger = false;
        }
    }

    public void GuestCheckDistance()
    {
        if (guestInRoom == null) return;
        float distSqr = (guestInRoom.transform.position - transform.position).sqrMagnitude;

        // เอาค่าระยะที่ต้องการมายกกำลังสองก่อนเทียบ
        if (distSqr <= (0.2 * 0.2) && !roomData.isAvailable)
        {
            Debug.Log("ถึงห้องแล้ววว");
            roomData.isAvailable = true;
            StartService();
            return;
        }
    }
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
