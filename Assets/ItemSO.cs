using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Hotel/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Item Information")]
    public string itemName = "Unnamed Item";
    public Sprite itemIcon;
    public GameObject itemPrefab; // สำหรับสร้างในเกม
    public string description = "";

    [Header("Gameplay Properties")]
    public ServiceRequestType requiredForService; // งานประเภทไหนใช้ไอเทมนี้
    public float pickupTime = 1.0f; // เวลาที่ใช้หยิบ
    public float deliverTime = 1.0f; // เวลาที่ใช้ส่ง

    [Header("Visual Feedback")]
    public Color itemColor = Color.white;
    public Vector3 holdOffset = new Vector3(0, 0.5f, 0); // ตำแหน่งที่ถือ
}