using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Room : CanInteractObj, IInteractable
{
    public RoomData RoomData => interactObjData.roomData;
    public RoomState roomState;
    public ObjColor roomColor;

    [Header("Service State")]
    public float serviceCooldown = 5f;
    public bool isDelivered;
    public int deliveryCount;
    public Counter counter;

    [SerializeField] private GuestAI guestInRoom;
    public GameObject dirtyIcon;
    [Header("Dirty Room Laundry Pickup")]
    public ItemSO laundryPickupItem;
    [Min(1)] public int laundryPickupAmount = 1;

    [HideInInspector] public List<ItemSO> serviceQueue = new List<ItemSO>();

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Start()
    {
        base.Start();
        roomState = RoomState.Available;

        if (dirtyIcon != null) dirtyIcon.SetActive(false);
        if (counter == null) counter = FindAnyObjectByType<Counter>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player.finalRoomData != RoomData) return;

            player.finalRoomData = null;
            player.targetIObj = null;

            if (guestInRoom != null && guestInRoom.currentService != null)
            {
                if (CheckDelivery(guestInRoom.currentService, player))
                {
                    guestInRoom.RestoreHeart();
                    deliveryCount++;
                    int requiredDeliveryCount = guestInRoom.GetRequiredDeliveryCountForService(guestInRoom.currentService);
                    int remainingCount = Mathf.Max(0, requiredDeliveryCount - deliveryCount);
                    guestInRoom.guestUI?.SetDeliveryCount(remainingCount);
                    if (deliveryCount >= requiredDeliveryCount)
                    {
                        isDelivered = true;
                    }
                }
            }

            if (roomState == RoomState.Dirty)
            {
                TryCleanRoom(player);
            }

            player.travelState = TravelState.Idle;
        }

        if (collision.CompareTag("Employee"))
        {
            Employee employee = collision.GetComponent<Employee>();
            if (employee.finalRoomData != RoomData) return;

            employee.finalRoomData = null;
            employee.targetIObj = null;

            if (guestInRoom != null && guestInRoom.currentService != null)
            {
                if (CheckDelivery(guestInRoom.currentService, employee))
                {
                    guestInRoom.RestoreHeart();
                    deliveryCount++;
                    int requiredDeliveryCount = guestInRoom.GetRequiredDeliveryCountForService(guestInRoom.currentService);
                    int remainingCount = Mathf.Max(0, requiredDeliveryCount - deliveryCount);
                    guestInRoom.guestUI?.SetDeliveryCount(remainingCount);
                    if (deliveryCount >= requiredDeliveryCount)
                    {
                        isDelivered = true;
                    }
                }
            }

            if (roomState == RoomState.Dirty)
            {
                TryCleanRoom(employee);
            }
        }

        if (collision.CompareTag("Guest"))
        {
            GuestAI guest = collision.GetComponent<GuestAI>();
            if (guest == null) return;
            if (guest.guestPhase == Guestphase.CheckingOut) return;
            if (guest.finalRoomData != RoomData) return;
            if (guestInRoom != null) return;

            if (guest.guestColor == roomColor)
            {
                FindAnyObjectByType<LevelManager>()?.AddCombo(1);
            }
            else
            {
                FindAnyObjectByType<LevelManager>()?.ResetCombo();
            }

            RoomData.isUnAvailable = true;
            roomState = RoomState.OnUse;
            guest.travelState = TravelState.stayRoom;
            guest.AnimateEnterRoom();
            guest.tip += 10;

            StartService(guest);
            RoomManager.Instance?.OnInstacneLuggage();
        }
    }

    public void StartService(GuestAI guest)
    {
        guestInRoom = guest;
        serviceQueue = BuildQueue(guest.servicePool, guest.serviceCount);
        guest.OnCheckIn();
        guest.guestUI?.OnCheckIn();
        guest.StartProcessing(this);
    }

    private List<ItemSO> BuildQueue(List<ItemSO> pool, int serviceCount)
    {
        List<ItemSO> result = new List<ItemSO>();
        List<ItemSO> working = new List<ItemSO>(pool);

        for (int i = 0; i < working.Count; i++)
        {
            if (working[i].requiredForService == ServiceRequestType.DeliveryLuggage)
            {
                result.Add(working[i]);
                working.RemoveAt(i);
                break;
            }
        }

        Shuffle(working);
        result.AddRange(working);

        if (result.Count > serviceCount)
        {
            result.RemoveRange(serviceCount, result.Count - serviceCount);
        }

        return result;
    }

    private bool CheckDelivery(ItemSO service, MoveHandleAI actor)
    {
        if (service == null) return false;

        List<ItemSO> inv = GetInventory(actor);
        if (inv == null) return false;

        ItemSO found = inv.Find(x => x == service);
        if (found == null)
        {
            Debug.Log("<color=red>Missing required item for service</color>");
            return false;
        }

        RemoveFromInventory(actor, found);
        return true;
    }

    public void AssignGuest(GuestAI guest) => guestInRoom = guest;

    public bool TryCleanRoom(MoveHandleAI actor)
    {
        if (laundryPickupItem == null)
        {
            Debug.LogWarning($"Room {name} has no laundryPickupItem assigned.");
            return false;
        }

        for (int i = 0; i < Mathf.Max(1, laundryPickupAmount); i++)
        {
            if (!TryAddItemToActor(actor, laundryPickupItem))
            {
                return false;
            }
        }

        guestInRoom = null;
        roomState = RoomState.Available;
        RoomData.isUnAvailable = false;
        if (dirtyIcon != null) dirtyIcon.SetActive(false);
        RoomData.currentServiceRequest = null;
        return true;
    }

    public void DirtyRoom()
    {
        guestInRoom = null;
        roomState = RoomState.Dirty;
        RoomData.isUnAvailable = true;

        if (dirtyIcon != null) dirtyIcon.SetActive(true);

        Debug.Log($"<color=orange>Room {name} is now dirty</color>");
    }

    public void ApplyPersistentRoomLevel(int upgradeTier)
    {
        if (interactObjData == null || interactObjData.roomData == null) return;

        int safeTier = Mathf.Clamp(upgradeTier, 0, (int)RoomLevel.Suite);
        interactObjData.roomData.roomLevel = (RoomLevel)safeTier;
    }

    public override void Interact(MoveHandleAI actor)
    {
        base.Interact(actor);
        actor.finalRoomData = RoomData;
        actor.StartTravel(interactObjData);
    }

    private List<ItemSO> GetInventory(MoveHandleAI actor)
    {
        if (actor is Player p) return p.inventory;
        if (actor is Employee e) return e.inventory;
        return null;
    }

    private void RemoveFromInventory(MoveHandleAI actor, ItemSO item)
    {
        if (actor is Player p)
        {
            p.RemoveItem(item);
            return;
        }

        if (actor is Employee e)
        {
            e.RemoveItem(item);
        }
    }

    private bool TryAddItemToActor(MoveHandleAI actor, ItemSO item)
    {
        if (item == null || actor == null) return false;
        if (actor is Player p) return p.AddItem(item);
        if (actor is Employee e) return e.AddItem(item);
        return false;
    }

    private void Shuffle(List<ItemSO> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
