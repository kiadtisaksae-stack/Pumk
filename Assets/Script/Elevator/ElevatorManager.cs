using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ElevatorManager : MonoBehaviour
{
    public List<ElevatorController> elevators = new List<ElevatorController>();

    void Start()
    {
        // ค้นหาลิฟต์ที่มีทั้งหมดใน Scene นี้ตอนเริ่มด่าน
        RefreshElevatorList();
    }

    public void RefreshElevatorList()
    {
        elevators = Object.FindObjectsByType<ElevatorController>(FindObjectsSortMode.None).ToList();
    }

    /// <summary>
    /// หาลิฟต์ที่เหมาะสม:
    /// 1. เรียงตาม physical distance จาก actor ไปหาลิฟต์ (ใกล้สุดก่อน)
    /// 2. ลิฟต์ใกล้สุด → ถ้ามี slot ว่างที่ชั้นนั้น ใช้เลย
    /// 3. ถ้าไม่มี slot ว่าง → ไปตัวถัดไปตามระยะ
    /// 4. ถ้าทุกตัวไม่ว่าง → ใช้ตัวที่ใกล้สุดแล้วรอ (RegisterGuestReady จะจัดการ)
    /// </summary>
    public ElevatorController GetBestElevator(int fromFloor, int toFloor, Vector3 actorPosition)
    {
        if (elevators.Count == 0) return null;

        // เรียงลิฟต์จากใกล้สุด → ไกลสุด ตาม physical distance
        var sorted = elevators
            .Where(e => e != null)
            .OrderBy(e => Vector2.Distance(
                new Vector2(actorPosition.x, actorPosition.y),
                new Vector2(e.transform.position.x, e.transform.position.y)))
            .ToList();

        // รอบแรก: หาลิฟต์ที่ใกล้และมี slot ว่างที่ชั้น fromFloor
        foreach (var elevator in sorted)
        {
            if (elevator.passengers.Count >= elevator.maxCapacity) continue;
            if (elevator.floorQueues == null) continue;

            // แปลง world floor → local array index
            // ลิฟต์ตัวที่ 2 (minFloor=1): ชั้น 1 → index 0, ชั้น 2 → index 1
            int localIndex = fromFloor - elevator.minFloor;
            if (localIndex < 0 || localIndex >= elevator.floorQueues.Length) continue;

            // เช็กว่ามี slot ว่างไหม (ไม่จองยัง ให้ AssignElevator จอง)
            int dummy;
            Transform slot = elevator.floorQueues[localIndex].GetAvailableSlot(null, out dummy);
            if (slot != null)
            {
                elevator.floorQueues[localIndex].ReleaseSlot(dummy);
                return elevator;
            }
        }

        // รอบสอง: ทุกตัวไม่มี slot ว่าง → ใช้ตัวที่ใกล้สุด ปล่อยให้รอ
        return sorted.FirstOrDefault();
    }

    /// <summary>Overload เดิม — ใช้ actorPosition = Vector3.zero (fallback)</summary>
    public ElevatorController GetBestElevator(int fromFloor, int toFloor)
        => GetBestElevator(fromFloor, toFloor, Vector3.zero);
}