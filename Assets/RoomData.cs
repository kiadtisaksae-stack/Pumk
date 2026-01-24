using UnityEngine;

public enum RoomType { Small, Big }
public enum RoomLevel { Standard, Deluxe, Suite }

[System.Serializable]
public class RoomData : MonoBehaviour
{
    [Header("ชั้นแรก = 0 ")]
    public int floorNumber = 0;
    public RoomType roomType = RoomType.Small;
    public RoomLevel roomLevel = RoomLevel.Standard;

    public Transform interactionPoint; // จุดที่ AI จะเดินไปหยุด (เช่น หน้าเตียง)
    public bool isOccupied = false; // ห้องมีแขกพักอยู่หรือไม่

    private void Start()
    {
        if (interactionPoint == null)
        {
            Debug.LogError($"RoomData {name}: ไม่ได้กำหนด interactionPoint!");
            interactionPoint = transform;
        }
    }

    private void OnDrawGizmos()
    {
        if (interactionPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(interactionPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, interactionPoint.position);
        }
    }
}