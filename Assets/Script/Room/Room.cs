using UnityEngine;
using UnityEngine.UI;

public class Room : CanInteractObj
{
    [SerializeField]
    private RoomData roomData;
    public RoomData RoomData => roomData;

    [Header("Interaction")]
    public Button roomServiceButton;
    public Button upgradeRoomButton;

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
        serviceManager = FindAnyObjectByType<ServiceManager>();

    }
    private void Update()
    {
       
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out GuestAI guest)) return;
        if (roomData.isAvailable)
        {
            return;
        }
        roomData.isAvailable = true;
        guestInRoom = guest;
        roomServiceButton.gameObject.SetActive(true);
        
        
     
    }

    public void AssignGuest()
    {
        
       
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

    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position);
    }
}
