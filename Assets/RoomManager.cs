using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomManager : MonoBehaviour
{
    public List<RoomData> allRooms = new List<RoomData>();

    // ฟังก์ชันช่วยหาห้องที่ว่างตามประเภท
    public RoomData GetAvailableRoom(RoomType type)
    {
        return allRooms.FirstOrDefault(r => r.roomType == type && !r.isOccupied);
    }

    // จัดเก็บห้องทั้งหมดที่มีใน Scene อัตโนมัติ (เลือกใช้ได้)
    [ContextMenu("Refresh Room List")]
    public void RefreshRooms()
    {
        allRooms = FindObjectsOfType<RoomData>().ToList();
    }
}