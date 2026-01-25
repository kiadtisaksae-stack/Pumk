using UnityEngine;

public enum RoomType { Small, Big }
public enum RoomLevel { Standard, Deluxe, Suite }

[System.Serializable]
public class RoomData
{
    [Header("Room Info")]
    public int floorNumber;
    public RoomType roomType;
    public RoomLevel roomLevel;
    public int RoomID;
    public Transform roomPosition;

    [Header("Runtime")]
    public bool isAvailable;
    public ItemSO currentServiceRequest;
}
