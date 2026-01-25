using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    [SerializeField]
    private RoomData roomData;

    public RoomData RoomData => roomData;

    [Header("Interaction")]
    public Button roomServiceButton;
    public Button upgradeRoomButton;

    private void Awake()
    {
        if (upgradeRoomButton != null)
            upgradeRoomButton.onClick.AddListener(UpgradeRoom);

        if (roomServiceButton != null)
            roomServiceButton.onClick.AddListener(StartService);

        UpdateUpgradeButton();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out GuestAI guest)) return;
        
        if (roomData.isOccupied)
        {
            return;
        }
        AssignGuest(guest);
    }

    public void AssignGuest(GuestAI guest)
    {
        roomData.isOccupied = true;
        

        Debug.Log($"🏨 {name} รับแขก {guest.name}");
    }

    public void ClearRoom()
    {
        roomData.isOccupied = false;
        roomData.currentServiceRequest = null;
    }
    public void UpgradeRoom()
    {
        if (roomData.isOccupied)
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
            !roomData.isOccupied &&
            roomData.roomLevel != RoomLevel.Suite;
    }
    public void StartService()
    {

    }

    private void OnDrawGizmos()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position);
    }
}
