using UnityEngine;
using System.Collections.Generic;

public class ElevatorDebugGUI : MonoBehaviour
{
    public ElevatorController elevator;
    private GUIStyle boxStyle;
    private GUIStyle labelStyle;

    void Start()
    {
        if (elevator == null)
            elevator = FindObjectOfType<ElevatorController>();
    }

    void OnGUI()
    {
        if (elevator == null) return;

        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.textColor = Color.white;
            boxStyle.fontSize = 12;

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 11;
        }

        GUILayout.BeginArea(new Rect(10, 10, 350, 600));

        GUILayout.Box("🚀 Elevator Status (New System)", boxStyle, GUILayout.Width(340));

        GUILayout.Label($"Current Floor: {elevator.currentFloor}", labelStyle);
        GUILayout.Label($"State: {elevator.currentDirection}", labelStyle);
        GUILayout.Label($"Moving: {elevator.isMoving}", labelStyle);
        GUILayout.Label($"Passengers: {elevator.passengers.Count}/{elevator.maxCapacity}", labelStyle);

        GUILayout.Space(10);
        GUILayout.Label($"Destination Queue: [{string.Join(", ", elevator.destinationQueue)}]", labelStyle);

        GUILayout.Space(10);
        // แก้ไขส่วนแสดงผล Waiting Guests ให้ใช้ระบบใหม่ (readyAIsOnFloor)
        GUILayout.Label("📊 Ready Guests (At WaitSlots):", labelStyle);

        bool hasWaiting = false;

        // ดึงข้อมูลจากฟังก์ชันที่ต้องการ (เข้าถึง private field ผ่านการจำลองข้อมูลหรือปรับเป็น public)
        // เพื่อให้ DebugGUI ทำงานได้ ผมแนะนำให้ไปที่ ElevatorController แล้วเปลี่ยน readyAIsOnFloor เป็น public ครับ

        for (int i = 0; i < elevator.floorTargets.Length; i++)
        {
            // หมายเหตุ: คุณต้องกลับไปเปลี่ยน private Dictionary ใน ElevatorController 
            // ให้เป็น public Dictionary<int, List<MoveHandleAI>> readyAIsOnFloor เพื่อให้ Debug อ่านค่าได้

            // ในที่นี้สมมติว่าเปลี่ยนเป็น Public แล้ว:
            /* if (elevator.readyAIsOnFloor.ContainsKey(i) && elevator.readyAIsOnFloor[i].Count > 0)
            {
                GUILayout.Label($"  Floor {i}: {elevator.readyAIsOnFloor[i].Count} Ready to enter", labelStyle);
                hasWaiting = true;
            }
            */
        }

        if (!hasWaiting)
            GUILayout.Label("  No one ready at slots", labelStyle);

        GUILayout.Space(10);

        if (GUILayout.Button("Force Start Elevator") && !elevator.isMoving)
        {
            elevator.StartCoroutine(elevator.ProcessElevatorLoop());
        }

        GUILayout.EndArea();
    }
}