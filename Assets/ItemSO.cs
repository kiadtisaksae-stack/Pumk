using UnityEngine;

public enum ServiceRequestType
{
    None,
    Laundry,
    RoomService,
    Cleaning
}

[CreateAssetMenu(fileName = "NewItem", menuName = "Hotel/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea] public string description;
    public GameObject prefabItem;

    [Header("Service")]
    public ServiceRequestType requiredForService;
    public float pickupTime = 1f;
    public float deliverTime = 1f;
}
