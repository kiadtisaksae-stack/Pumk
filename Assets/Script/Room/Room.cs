using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(ServiceManager))]
public class Room : CanInteractObj,IInteractable
{

    public RoomData RoomData => interactObjData.roomData;

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
    public override void Start()
    {
        base.Start();
        serviceManager = GetComponent<ServiceManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Actor Standing at the Door ");
            Player player = collision.GetComponent<Player>();

            if (player.finalRoomData != RoomData) return;

            if (guestInRoom != null)
            {
                serviceManager.RequestCheck(guestInRoom.currentService, player.inventory);
            }
            player.travelState = TravelState.Idle;

        }
        if( (collision.CompareTag("Employee")))
        {
            Debug.Log("Employee Standing at the Door ");
            Employee emplyee = collision.GetComponent<Employee>();
            if (emplyee.finalRoomData != RoomData) return;
            if (guestInRoom != null)
            {
                serviceManager.RequestCheck(guestInRoom.currentService, emplyee.inventory);
            }
            
        }
        if (collision.CompareTag("Guest"))
        {
            Debug.Log("Guest entered the zone!");
            GuestAI guest = collision.GetComponent<GuestAI>();
            if(guest.guestPhase == Guestphase.CheckingOut) return;
            if (guest.finalRoomData != RoomData) return;
            RoomData.isAvailable = true;
            guest.travelState = TravelState.stayRoom;
            guest.AnimateEnterRoom();
            StartService();

        }
    }

    public void AssignGuest(GuestAI guest)
    {
        guestInRoom = guest;
    }

    public void ClearRoom()
    {
        RoomData.isAvailable = false;
        RoomData.currentServiceRequest = null;
    }
    public void UpgradeRoom()
    {
        if (RoomData.isAvailable)
        {
            Debug.Log("❌ ไม่สามารถอัปเกรดได้ ขณะมีแขก");
            return;
        }

        if (RoomData.roomLevel == RoomLevel.Suite)
        {
            Debug.Log("❌ ห้องนี้เป็น Suite แล้ว");
            return;
        }

        RoomData.roomLevel++;
        Debug.Log($"⬆️ อัปเกรดห้อง {name} เป็น {RoomData.roomLevel}");

        UpdateUpgradeButton();
    }
    private void UpdateUpgradeButton()
    {
        if (upgradeRoomButton == null) return;

        upgradeRoomButton.interactable =
            !RoomData.isAvailable &&
            RoomData.roomLevel != RoomLevel.Suite;
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

    public override void Interact(MoveHandleAI actor)
    {
        base.Interact(actor);
        actor.finalRoomData = RoomData;
        actor.StartTravel(interactObjData);
    }
}
