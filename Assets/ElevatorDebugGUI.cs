using UnityEngine;

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

        GUILayout.BeginArea(new Rect(10, 10, 350, 500));

        GUILayout.Box("🚀 Elevator Status", boxStyle, GUILayout.Width(340));

        GUILayout.Label($"Current Floor: {elevator.currentFloor}", labelStyle);
        GUILayout.Label($"State: {elevator.currentDirection}", labelStyle);
        GUILayout.Label($"Moving: {elevator.isMoving}", labelStyle);
        GUILayout.Label($"Passengers: {elevator.passengers.Count}/{elevator.maxCapacity}", labelStyle);

        GUILayout.Space(10);
        GUILayout.Label($"Destination Queue: [{string.Join(", ", elevator.destinationQueue)}]", labelStyle);

        GUILayout.Space(10);
        GUILayout.Label("📊 Waiting Guests:", labelStyle);

        bool hasWaiting = false;
        foreach (var floor in elevator.floorWaitQueue.Keys)
        {
            if (elevator.floorWaitQueue[floor].Count > 0)
            {
                GUILayout.Label($"  Floor {floor}: {elevator.floorWaitQueue[floor].Count} waiting", labelStyle);
                hasWaiting = true;
            }
        }

        if (!hasWaiting)
            GUILayout.Label("  No one waiting", labelStyle);

        GUILayout.Space(10);

        if (GUILayout.Button("Force Start Elevator") && !elevator.isMoving)
        {
            // เรียก ProcessElevatorLoop โดยตรง
            elevator.StartCoroutine(elevator.ProcessElevatorLoop());
        }

        GUILayout.EndArea();
    }
}