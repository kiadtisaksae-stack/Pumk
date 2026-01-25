using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [Header("All Rooms in Hotel")]
    [SerializeField]
    private List<Room> allRooms = new List<Room>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #region Public API

    /// <summary>
    /// หาห้องว่างตาม Type
    /// </summary>
    public Room GetAvailableRoom(RoomType type)
    {
        return allRooms.FirstOrDefault(r =>
            !r.RoomData.isOccupied &&
            r.RoomData.roomType == type
        );
    }

    /// <summary>
    /// หาห้องว่างตาม Type + Level
    /// </summary>
    public Room GetAvailableRoom(RoomType type, RoomLevel level)
    {
        return allRooms.FirstOrDefault(r =>
            !r.RoomData.isOccupied &&
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
            .Where(r => !r.RoomData.isOccupied)
            .ToList();
    }

    #endregion

    #region Editor Only

    [ContextMenu("Refresh Room List")]
    private void RefreshRooms()
    {
        allRooms = FindObjectsOfType<Room>(true).ToList();
        Debug.Log($"🏨 Found {allRooms.Count} rooms");
    }

    #endregion
}
