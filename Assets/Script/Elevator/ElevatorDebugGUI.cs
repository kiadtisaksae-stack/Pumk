using UnityEngine;
using System.Collections.Generic;

public class ElevatorDebugGUI : MonoBehaviour
{
    // ไม่ต้องลากใส่แล้ว ระบบจะหาเองทุกตัวในฉาก
    private ElevatorController[] allElevators;
    private GUIStyle boxStyle;
    private GUIStyle labelStyle;
    private GUIStyle headerStyle;

    void Update()
    {
        // ค้นหาลิฟต์ใหม่ทุกเฟรม (หรือจะทำเป็นปุ่ม Refresh ก็ได้ถ้าลิฟต์เยอะมาก)
        allElevators = Object.FindObjectsByType<ElevatorController>(FindObjectsSortMode.None);
    }

    void OnGUI()
    {
        if (allElevators == null || allElevators.Length == 0)
        {
            GUI.Label(new Rect(20, 20, 200, 20), "<color=red>No Elevators Found!</color>");
            return;
        }

        // Setup Styles (ทำงานครั้งเดียว)
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            labelStyle.normal.textColor = Color.white;

            headerStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };
            headerStyle.normal.textColor = Color.yellow;
        }

        // วาดสถานะของลิฟต์แต่ละตัว
        for (int idx = 0; idx < allElevators.Length; idx++)
        {
            ElevatorController elevator = allElevators[idx];
            if (elevator == null) continue;

            // คำนวณตำแหน่งกรอบ (ขยับไปทางขวาตามจำนวนลิฟต์)
            float xPos = 20 + (idx * 310);
            GUILayout.BeginArea(new Rect(xPos, 20, 300, 800));

            // --- 1. สถานะทั่วไป ---
            GUILayout.BeginVertical("box");
            GUILayout.Label($"🛗 ELEVATOR: {elevator.gameObject.name}", headerStyle);
            GUILayout.Space(5);
            GUILayout.Label($"Floor: <color=cyan>{elevator.currentFloor}</color>", labelStyle);
            GUILayout.Label($"Dir: <color=white>{elevator.currentDirection}</color>", labelStyle);

            // แก้ไข Error: ตรวจสอบตัวแปร isMoving
            string movingText = elevator.isMoving ? "<color=green>MOVING</color>" : "<color=red>IDLE</color>";
            GUILayout.Label($"Status: {movingText}", labelStyle);

            // แสดงคิว (Floor Targets)
            string queueStr = (elevator.destinationQueue != null) ? string.Join(", ", elevator.destinationQueue) : "Empty";
            GUILayout.Label($"Queue: [{queueStr}]", labelStyle);
            GUILayout.EndVertical();

            // --- 2. คนรอที่ชั้น ---
            GUILayout.BeginVertical("box");
            GUILayout.Label("👥 WAITING AT SLOTS", headerStyle);
            bool hasWaiting = false;
            if (elevator.readyAIsOnFloor != null)
            {
                foreach (var floor in elevator.readyAIsOnFloor)
                {
                    if (floor.Value.Count > 0)
                    {
                        hasWaiting = true;
                        GUILayout.Label($"Floor {floor.Key}: {floor.Value.Count} ppl", labelStyle);
                    }
                }
            }
            if (!hasWaiting) GUILayout.Label("<color=grey>None</color>", labelStyle);
            GUILayout.EndVertical();

            // --- 3. คนในลิฟต์ ---
            GUILayout.BeginVertical("box");
            GUILayout.Label($"👨‍👩‍👧 PASSENGERS ({elevator.passengers.Count}/{elevator.maxCapacity})", headerStyle);
            if (elevator.passengers.Count > 0)
            {
                foreach (var p in elevator.passengers)
                {
                    if (p != null)
                        GUILayout.Label($"• {p.name} -> F{p.targetFloor}", labelStyle);
                }
            }
            else GUILayout.Label("<color=grey>Empty</color>", labelStyle);
            GUILayout.EndVertical();

            // ปุ่ม Debug
            if (GUILayout.Button("Force Loop"))
            {
                elevator.StartCoroutine(elevator.ProcessElevatorLoop());
            }

            GUILayout.EndArea();
        }
    }
}