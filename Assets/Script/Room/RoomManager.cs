using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [Header("All Rooms in Hotel")]
    [SerializeField]
    private List<Room> allRooms = new List<Room>();
    public GameObject luggagePrefab;
    public Transform pointSPluggage;
    public float offsetLuggage = 0.8f;
    private List<GameObject> listluggageShow;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (allRooms == null || allRooms.Count == 0)
        {
            RefreshRoomsRuntime();
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            ApplyPersistentRoomUpgrades(GameManager.Instance);
        }
    }

    #region Public API

    /// <summary>
    /// หาห้องว่างตาม Type
    /// </summary>
    public Room GetAvailableRoom(RoomType type)
    {
        return allRooms.FirstOrDefault(r =>
            !r.RoomData.isUnAvailable &&
            r.RoomData.roomType == type
        );
    }

    /// <summary>
    /// หาห้องว่างตาม Type + Level
    /// </summary>
    public Room GetAvailableRoom(RoomType type, RoomLevel level)
    {
        return allRooms.FirstOrDefault(r =>
            !r.RoomData.isUnAvailable &&
            r.RoomData.roomType == type &&
            r.RoomData.roomLevel == level
        );
    }

    /// <summary>
    /// คืน List ห้องว่างทั้งหมด (เอาไปสุ่ม / เลือกชั้น)
    /// </summary>
    public List<Room> GetAllAvailableRooms()
    {
        return allRooms
            .Where(r => !r.RoomData.isUnAvailable)
            .ToList();
    }

    public List<Room> GetAllRooms()
    {
        if (allRooms == null || allRooms.Count == 0)
        {
            RefreshRoomsRuntime();
        }

        return allRooms;
    }

    public void ApplyPersistentRoomUpgrades(GameManager gameManager)
    {
        if (gameManager == null) return;

        if (allRooms == null || allRooms.Count == 0)
        {
            RefreshRoomsRuntime();
        }

        for (int i = 0; i < allRooms.Count; i++)
        {
            Room room = allRooms[i];
            if (room == null || room.RoomData == null) continue;

            int roomId = room.RoomData.RoomID;
            int tier = gameManager.GetRoomUpgradeLevel(roomId);
            room.ApplyPersistentRoomLevel(tier);
        }
    }
    public void OnInstacneLuggage()
    {
        if (listluggageShow == null)
            listluggageShow = new List<GameObject>();

        int count = listluggageShow.Count;

        // คำนวณ offset
        float offset = offsetLuggage * count;

        // สมมติให้เรียงตามแกน X
        Vector3 spawnPos = pointSPluggage.position + new Vector3(offset, 0, 0);

        GameObject luggage = Instantiate(luggagePrefab, spawnPos, Quaternion.identity);

        listluggageShow.Add(luggage);
    }
    public void DeleteLuggage()
    {
        if (listluggageShow != null && listluggageShow.Count > 0)
        {
            GameObject first = listluggageShow[0];

            // ลบ object ใน scene
            Destroy(first);

            // ลบออกจาก list
            listluggageShow.RemoveAt(0);
        }
    }
    #endregion

    #region Editor Only

    [ContextMenu("Refresh Room List")]
    private void RefreshRooms()
    {
        allRooms = FindObjectsOfType<Room>(true).ToList();

    }

    private void RefreshRoomsRuntime()
    {
        allRooms = FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
    }

    #endregion
}
