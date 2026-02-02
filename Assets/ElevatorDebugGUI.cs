using UnityEngine;
using System.Collections.Generic;

public class ElevatorDebugGUI : MonoBehaviour
{
    public ElevatorController elevator;
    private GUIStyle boxStyle;
    private GUIStyle labelStyle;
    private GUIStyle headerStyle;

    void Start()
    {
        if (elevator == null)
            elevator = FindObjectOfType<ElevatorController>();
    }

    void OnGUI()
    {
        if (elevator == null) return;

        // Setup Styles
        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.textColor = Color.white;
            boxStyle.fontSize = 12;
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.padding = new RectOffset(10, 10, 10, 10);

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 12;
            labelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 13;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.yellow;
        }

        // เริ่มวาดกรอบ
        GUILayout.BeginArea(new Rect(20, 20, 400, 700));

        // --- 1. สถานะทั่วไป ---
        GUILayout.BeginVertical("box");
        GUILayout.Label("🚀 ELEVATOR STATUS", headerStyle);
        GUILayout.Space(5);
        GUILayout.Label($"Current Floor: <color=white>{elevator.currentFloor}</color>", labelStyle);
        GUILayout.Label($"Direction: <color=white>{elevator.currentDirection}</color>", labelStyle);
        GUILayout.Label($"Moving: {(elevator.isMoving ? "<color=green>YES</color>" : "<color=red>NO</color>")}", labelStyle);
        GUILayout.Label($"Queue (Floors): [{string.Join(", ", elevator.destinationQueue)}]", labelStyle);
        GUILayout.EndVertical();

        GUILayout.Space(10);

        // --- 2. คนที่ค้างอยู่ในจุดรอ (Waiting at Slots) ---
        GUILayout.BeginVertical("box");
        GUILayout.Label("📊 1. WAITING GUESTS (At WaitSlots)", headerStyle);
        GUILayout.Space(5);

        bool hasWaiting = false;
        if (elevator.readyAIsOnFloor != null)
        {
            // วนลูปเช็คทุกชั้น
            for (int i = 0; i < elevator.floorTargets.Length; i++)
            {
                if (elevator.readyAIsOnFloor.ContainsKey(i) && elevator.readyAIsOnFloor[i].Count > 0)
                {
                    hasWaiting = true;
                    List<MoveHandleAI> guests = elevator.readyAIsOnFloor[i];

                    GUILayout.Label($"Floor {i}: <color=cyan>{guests.Count} People</color>", labelStyle);
                    foreach (var g in guests)
                    {
                        GUILayout.Label($"   - {g.name} (Target: {g.targetFloor})", labelStyle);
                    }
                }
            }
        }

        if (!hasWaiting)
        {
            GUILayout.Label("   <color=grey>No one waiting.</color>", labelStyle);
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);

        // --- 3. คนที่ค้างอยู่ในลิฟต์ (Passengers Inside) ---
        GUILayout.BeginVertical("box");
        GUILayout.Label($"🛗 2. PASSENGERS INSIDE ({elevator.passengers.Count}/{elevator.maxCapacity})", headerStyle);
        GUILayout.Space(5);

        if (elevator.passengers.Count > 0)
        {
            foreach (var p in elevator.passengers)
            {
                // แสดงชื่อและชั้นที่จะไป
                GUILayout.Label($"   • <color=green>{p.name}</color> --> Going to Floor: <color=yellow>{p.targetFloor}</color>", labelStyle);
            }
        }
        else
        {
            GUILayout.Label("   <color=grey>Elevator is empty.</color>", labelStyle);
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);

        // ปุ่มบังคับเริ่ม (เผื่อใช้ Test)
        if (GUILayout.Button("Force Start Elevator", GUILayout.Height(30)) && !elevator.isMoving)
        {
            elevator.StartCoroutine(elevator.ProcessElevatorLoop());
        }

        GUILayout.EndArea();
    }
}