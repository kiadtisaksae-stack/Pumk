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

    public ElevatorController GetBestElevator(int fromFloor, int toFloor)
    {
        if (elevators.Count == 0) return null;

        ElevatorController bestElevator = null;
        float bestScore = float.MaxValue;

        foreach (var elevator in elevators)
        {
            // คำนวณคะแนน: ระยะทาง + ความหนาแน่นของคน
            float score = Mathf.Abs(elevator.currentFloor - fromFloor);

            // ถ้าลิฟต์เต็ม ให้คะแนนติดลบหนักๆ (Score สูง)
            if (elevator.passengers.Count >= elevator.maxCapacity) score += 100;

            // ถ้าลิฟต์กำลังไปทิศเดียวกับเรา ให้โบนัสนิดหน่อย (Score ต่ำลง)
            bool isUp = (toFloor > fromFloor);
            if (elevator.currentDirection == ElevatorDirection.Up && isUp) score -= 1;
            if (elevator.currentDirection == ElevatorDirection.Down && !isUp) score -= 1;

            if (score < bestScore)
            {
                bestScore = score;
                bestElevator = elevator;
            }
        }
        return bestElevator;
    }
}