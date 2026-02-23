using UnityEngine;

public enum RoomType { Small, Big }
public enum RoomLevel { Standard, Deluxe, Suite }

public enum RoomState { Available, OnUse , Dirty }

[System.Serializable]
public class RoomData
{
    [Header("Room Info")]
    public RoomType roomType;
    public RoomLevel roomLevel;
    public int RoomID;

    [Header("Runtime")]
    public bool isUnAvailable;
    public ItemSO currentServiceRequest;
}
