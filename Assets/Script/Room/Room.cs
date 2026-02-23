using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(ServiceManager))]
public class Room : CanInteractObj,IInteractable
{

    public RoomData RoomData => interactObjData.roomData;
    public RoomState roomState;

    [Header("Interaction")]

    public Button upgradeRoomButton;

    [SerializeField]
    private GuestAI guestInRoom;

    public GameObject unCleanObj;
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
        roomState = RoomState.Available;
        unCleanObj.SetActive(false);
        serviceManager = GetComponent<ServiceManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player.finalRoomData != RoomData) return;

            if (guestInRoom != null && guestInRoom.currentService != null)
            {
                serviceManager.RequestCheck(guestInRoom.currentService, player);
            }
            if (roomState == RoomState.Dirty)
            {
                //เช็คของและเคลียร์
                ClearRoom(player);
                roomState = RoomState.Available;
            }

            player.travelState = TravelState.Idle;
        }

        if (collision.CompareTag("Employee"))
        {
            Employee employee = collision.GetComponent<Employee>();
            if (employee.finalRoomData != RoomData) return;

            if (guestInRoom != null && guestInRoom.currentService != null)
            {
                serviceManager.RequestCheck(guestInRoom.currentService, employee);
            }
        }
        if (collision.CompareTag("Guest"))
        {
            Debug.Log("Guest entered the zone!");
            GuestAI guest = collision.GetComponent<GuestAI>();
            if(guest.guestPhase == Guestphase.CheckingOut) return;
            if (guest.finalRoomData != RoomData) return;
            RoomData.isUnAvailable = true;
            roomState = RoomState.OnUse;
            guest.travelState = TravelState.stayRoom;
            guest.AnimateEnterRoom();
            StartService();

        }
    }

    public void AssignGuest(GuestAI guest)
    {
        guestInRoom = guest;
    }

    public void ClearRoom(MoveHandleAI actor)
    {
        List<ItemSO> actorInventory = null;
        Player playerActor = actor as Player;
        Employee employeeActor = actor as Employee;

        if (playerActor != null) actorInventory = playerActor.inventory;
        else if (employeeActor != null) actorInventory = employeeActor.inventory;

        foreach (ItemSO item in actorInventory)
        {
            if (item.requiredForService == ServiceRequestType.Laundry)
            {
                if (playerActor != null) playerActor.RemoveItem(item);
                else if (employeeActor != null) employeeActor.RemoveItem(item);

                guestInRoom = null;
                RoomData.isUnAvailable = false;
                unCleanObj.SetActive(false);
                RoomData.currentServiceRequest = null;
                break;
            }
        }
    }


    public void DirtyRoom()
    {
        roomState = RoomState.Dirty;
        unCleanObj.SetActive(true);
    }



    public void UpgradeRoom()
    {
        if (RoomData.isUnAvailable)
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
            !RoomData.isUnAvailable &&
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
