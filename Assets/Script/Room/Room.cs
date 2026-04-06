using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Room คุยกับ GuestAI โดยตรง ไม่ผ่าน ServiceManager
/// - BuildQueue / CheckDelivery ย้ายมาอยู่ที่นี่
/// - GuestAI.ProcessRequests รับ Room โดยตรง
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Room : CanInteractObj, IInteractable
{
    public RoomData RoomData => interactObjData.roomData;
    public RoomState roomState;
    public ObjColor roomColor;

    [Header("Interaction")]
    public Button upgradeRoomButton;

    [Header("Service State")]
    public float serviceCooldown = 5f;
    public bool isDelivered;
    public int deliveryCount;   // นับชิ้นที่ส่งแล้วใน slot นี้
    public Counter counter;

    [SerializeField] private GuestAI guestInRoom;
    public GameObject unCleanObj;
    public GameObject dirtyIcon; // ไอคอนที่โชว์เมื่อห้องสกปรก
    [Header("Upgrade Settings")]
    public int upgradeRoomCost = 1;
    public int roomServiceBonusCost = 100;

    [HideInInspector] public List<ItemSO> serviceQueue = new List<ItemSO>();

    // ─────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        if (upgradeRoomButton != null)
            upgradeRoomButton.onClick.AddListener(UpgradeRoom);
        UpdateUpgradeButton();
    }

    public override void Start()
    {
        upgradeRoomCost = 1; // เริ่มต้นที่ 1 ดาว
        base.Start();
        roomState = RoomState.Available;
        unCleanObj.SetActive(false);
        if (dirtyIcon != null) dirtyIcon.SetActive(false);
        if (counter == null) counter = FindAnyObjectByType<Counter>();
        UpdateUpgradeButton();
    }

    // ─────────────────────────────────────────────
    //  Trigger Zone
    // ─────────────────────────────────────────────

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (player.finalRoomData != RoomData) return;

            // เคลียร์เป้าหมายเพื่อให้สามารถคลิกห้องกดย้ำซ้ำได้อีกรอบ เมื่อยืนอยู่ใน Collider
            player.finalRoomData = null;
            player.targetIObj = null;

            if (guestInRoom != null && guestInRoom.currentService != null)
                if (CheckDelivery(guestInRoom.currentService, player))
                {
                    deliveryCount++;
                    if (deliveryCount >= guestInRoom.deliveryPerSlot)
                        isDelivered = true;
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
                if (CheckDelivery(guestInRoom.currentService, employee))
                {
                    deliveryCount++;
                    if (deliveryCount >= guestInRoom.deliveryPerSlot)
                        isDelivered = true;
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

            // Guard: ถ้าห้องมี guest อยู่แล้ว (เช่น Franken กลับห้องหลัง sleepwalk)
            // ไม่ต้อง StartService ใหม่ — guestInRoom ยังคง set อยู่ตลอดที่ guest ยังพักในห้องนั้น
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
            UpdateUpgradeButton();
            guest.travelState = TravelState.stayRoom;
            guest.AnimateEnterRoom();
            guest.tip += 10;

            StartService(guest);
            RoomManager.Instance.OnInstacneLuggage();
        }
    }

    // ─────────────────────────────────────────────
    //  Service
    // ─────────────────────────────────────────────

    public void StartService(GuestAI guest)
    {
        guestInRoom = guest;
        serviceQueue = BuildQueue(guest.servicePool, guest.serviceCount);
        guest.OnCheckIn();
        guest.guestUI?.OnCheckIn();
        guest.StartProcessing(this);
    }

    // ─────────────────────────────────────────────
    //  BuildQueue  (เคยอยู่ใน ServiceManager)
    // ─────────────────────────────────────────────

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
            result.RemoveRange(serviceCount, result.Count - serviceCount);

        return result;
    }

    // ─────────────────────────────────────────────
    //  CheckDelivery  (เคยอยู่ใน ServiceManager)
    // ─────────────────────────────────────────────

    private bool CheckDelivery(ItemSO service, MoveHandleAI actor)
    {
        if (service == null) return false;

        List<ItemSO> inv = GetInventory(actor);
        if (inv == null) return false;

        ItemSO found = inv.Find(x => x == service);
        if (found == null)
        {
            Debug.Log("<color=red>ไม่มีไอเทมที่ลูกค้าต้องการ</color>");
            return false;
        }

        RemoveFromInventory(actor, found);

        return true;
    }

    // ─────────────────────────────────────────────
    //  Room State
    // ─────────────────────────────────────────────

    public void AssignGuest(GuestAI guest) => guestInRoom = guest;

    public bool TryCleanRoom(MoveHandleAI actor)
    {
        List<ItemSO> inv = GetInventory(actor);
        if (inv == null) return false;

        foreach (ItemSO item in inv)
        {
            if (item.requiredForService != ServiceRequestType.Laundry) continue;

            RemoveFromInventory(actor, item);
            guestInRoom = null;
            roomState = RoomState.Available;
            RoomData.isUnAvailable = false;
            unCleanObj.SetActive(false);
            if (dirtyIcon != null) dirtyIcon.SetActive(false);
            RoomData.currentServiceRequest = null;
            UpdateUpgradeButton();

            return true;
        }
        return false;
    }

    public void DirtyRoom()
    {
        guestInRoom = null;
        roomState = RoomState.Dirty;
        RoomData.isUnAvailable = true;
        UpdateUpgradeButton();
        unCleanObj.SetActive(true);
        if (dirtyIcon != null) dirtyIcon.SetActive(true);
        Debug.Log($"<color=orange>ห้อง {name} สกปรกแล้ว!</color>");
    }

    // ─────────────────────────────────────────────
    //  Upgrade
    // ─────────────────────────────────────────────

    public void UpgradeRoom()
    {
        if (RoomData.isUnAvailable) { Debug.Log("❌ ไม่สามารถอัปเกรดได้ ขณะมีแขก"); return; }
        if (RoomData.roomLevel == RoomLevel.Suite) { Debug.Log("❌ Suite แล้ว"); return; }
        
        if (GameManager.Instance == null) return;
        
        if (GameManager.Instance.Star < upgradeRoomCost)
        {
            LevelUI levelUI = FindAnyObjectByType<LevelUI>();
            if (levelUI != null) levelUI.Notify("Not Enough Stars!");
            return;
        }

        GameManager.Instance.RemoveStar(upgradeRoomCost);
        RoomData.roomLevel++;
        roomServiceBonusCost += 100;
        upgradeRoomCost += 1; // เพิ่มราคาดาวในการอัปเกรดขั้นต่อไป

        UpdateUpgradeButton();
        
        // อัปเดต UI พอยต์ดาวให้แสดงค่าล่าสุด
        LevelUI ui = FindAnyObjectByType<LevelUI>();
        if (ui != null) ui.UpdateStarUI();
    }

    private void UpdateUpgradeButton()
    {
        if (upgradeRoomButton == null) return;
        
        // โชว์เฉพาะตอนว่าง หายไปเลยเมื่อมีแขกหรือห้องสกปรก
        upgradeRoomButton.gameObject.SetActive(!RoomData.isUnAvailable);

        upgradeRoomButton.interactable = RoomData.roomLevel != RoomLevel.Suite;
    }

    // ─────────────────────────────────────────────
    //  IInteractable
    // ─────────────────────────────────────────────

    public override void Interact(MoveHandleAI actor)
    {
        base.Interact(actor);
        actor.finalRoomData = RoomData;
        actor.StartTravel(interactObjData);
    }

    // ─────────────────────────────────────────────
    //  Inventory Helpers
    // ─────────────────────────────────────────────

    private List<ItemSO> GetInventory(MoveHandleAI actor)
    {
        if (actor is Player p) return p.inventory;
        if (actor is Employee e) return e.inventory;
        return null;
    }

    private void RemoveFromInventory(MoveHandleAI actor, ItemSO item)
    {
        if (actor is Player p) { p.RemoveItem(item); return; }
        if (actor is Employee e) { e.RemoveItem(item); return; }
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