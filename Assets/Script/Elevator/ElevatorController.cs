using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum ElevatorDirection { Idle, Up, Down }
#region FloorQueue
[System.Serializable]
public class FloorQueue
{
    public List<Transform> waitSlots;
    private Dictionary<int, MoveHandleAI> occupiedSlots = new Dictionary<int, MoveHandleAI>();

    public Transform GetAvailableSlot(MoveHandleAI guest, out int slotIndex)
    {
        for (int i = 0; i < waitSlots.Count; i++)
        {
            if (!occupiedSlots.ContainsKey(i) || occupiedSlots[i] == null)
            {
                occupiedSlots[i] = guest;
                slotIndex = i;
                return waitSlots[i];
            }
        }
        slotIndex = -1;
        return null;
    }

    public void ReleaseSlot(int index)
    {
        if (occupiedSlots.ContainsKey(index))
            occupiedSlots.Remove(index);
    }
}
#endregion


/* FLOW à¸ªà¸£à¸¸à¸›à¸‚à¸­à¸‡à¸£à¸°à¸šà¸šà¸¥à¸´à¸Ÿà¸•à¹Œ (SCAN Algorithm):
   1. Register: AI à¹€à¸”à¸´à¸™à¸–à¸¶à¸‡à¸ˆà¸¸à¸”à¸£à¸­à¹€à¸£à¸µà¸¢à¸ RegisterGuestReady -> à¸¥à¸´à¸Ÿà¸•à¹Œà¸šà¸±à¸™à¸—à¸¶à¸à¸„à¸™à¸£à¸­à¸¥à¸‡ readyAIsOnFloor à¹à¸¥à¸°à¹€à¸£à¸´à¹ˆà¸¡ Loop
   2. Decision (SCAN): DetermineNextTargetSmart à¸„à¸³à¸™à¸§à¸“à¹€à¸›à¹‰à¸²à¸«à¸¡à¸²à¸¢à¸•à¸²à¸¡à¸—à¸´à¸¨à¸—à¸²à¸‡ (Direction Priority)
      - à¸‚à¸²à¸‚à¸¶à¹‰à¸™ (UP): à¸§à¸´à¹ˆà¸‡à¸‚à¸¶à¹‰à¸™à¹„à¸›à¸«à¸²à¸Šà¸±à¹‰à¸™à¸—à¸µà¹ˆà¸¡à¸µà¸‡à¸²à¸™ à¸£à¸±à¸šà¹€à¸‰à¸žà¸²à¸°à¸„à¸™à¸—à¸µà¹ˆ "à¸ˆà¸°à¸‚à¸¶à¹‰à¸™" (à¸„à¸™à¸ˆà¸°à¸¥à¸‡à¸•à¹‰à¸­à¸‡à¸£à¸­à¸‚à¸²à¸à¸¥à¸±à¸š à¸¢à¸à¹€à¸§à¹‰à¸™à¹€à¸›à¹‡à¸™à¸ˆà¸¸à¸”à¸à¸¥à¸±à¸šà¸£à¸–)
      - à¸‚à¸²à¸¥à¸‡ (DOWN): à¸§à¸´à¹ˆà¸‡à¸¥à¸‡à¹„à¸›à¹€à¸à¹‡à¸šà¸„à¸™à¸—à¸µà¹ˆ "à¸ˆà¸°à¸¥à¸‡"
      - à¸ˆà¸¸à¸”à¸à¸¥à¸±à¸šà¸£à¸– (Turning Point): à¹€à¸¡à¸·à¹ˆà¸­à¸ªà¸¸à¸”à¸—à¸²à¸‡à¹à¸¥à¸°à¹„à¸¡à¹ˆà¸¡à¸µà¸‡à¸²à¸™à¸•à¹ˆà¸­à¹ƒà¸™à¸—à¸´à¸¨à¹€à¸”à¸´à¸¡ à¸ˆà¸°à¸£à¸±à¸šà¸—à¸¸à¸à¸„à¸™à¹à¸¥à¸°à¹€à¸›à¸¥à¸µà¹ˆà¸¢à¸™à¸—à¸´à¸¨
   3. Movement: MoveToFloor à¹€à¸„à¸¥à¸·à¹ˆà¸­à¸™à¸—à¸µà¹ˆà¹„à¸›à¸Šà¸±à¹‰à¸™à¹€à¸›à¹‰à¸²à¸«à¸¡à¸²à¸¢
   4. OpenDoors: 
      - à¸ªà¹ˆà¸‡à¸„à¸™à¸­à¸­à¸ (ExitElevator)
      - à¸£à¸±à¸šà¸„à¸™à¹€à¸‚à¹‰à¸²: à¹€à¸¥à¸·à¸­à¸à¸£à¸±à¸šà¹€à¸‰à¸žà¸²à¸°à¸„à¸™à¸—à¸µà¹ˆà¹„à¸›à¸—à¸´à¸¨à¹€à¸”à¸µà¸¢à¸§à¸à¸±à¸šà¸¥à¸´à¸Ÿà¸•à¹Œ (à¸«à¸£à¸·à¸­à¸£à¸±à¸šà¸«à¸¡à¸”à¸–à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œà¸§à¹ˆà¸²à¸‡/à¸à¸¥à¸±à¸šà¸£à¸–)
      - **Anti-Ghost & Distance**: à¸£à¸­à¸ˆà¸™à¸à¸§à¹ˆà¸² AI à¸ˆà¸°à¹€à¸”à¸´à¸™à¸–à¸¶à¸‡à¸«à¸™à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œ (Distance < 0.8f) à¹à¸¥à¸°à¸£à¸­à¹ƒà¸«à¹‰ AI à¸¢à¹‰à¸²à¸¢ Parent à¹€à¸ªà¸£à¹‡à¸ˆà¸ˆà¸£à¸´à¸‡à¸à¹ˆà¸­à¸™à¸›à¸´à¸”à¸›à¸£à¸°à¸•à¸¹
*/

public class ElevatorController : MonoBehaviour
{
    [Header("Elevator Settings")]
    public int maxCapacity = 4;
    public float speed = 3f;           // ความเร็วการเคลื่อนที่ของลิฟต์
    [SerializeField] private float baseSpeed = 3f;
    public int currentFloor = 0;       // à¸Šà¸±à¹‰à¸™à¸›à¸±à¸ˆà¸ˆà¸¸à¸šà¸±à¸™à¸—à¸µà¹ˆà¸¥à¸´à¸Ÿà¸•à¹Œà¸­à¸¢à¸¹à¹ˆ
    public float distacneWaitslottolift = 3f; // à¸£à¸°à¸¢à¸°à¸«à¹ˆà¸²à¸‡à¸ªà¸¹à¸‡à¸ªà¸¸à¸”à¸—à¸µà¹ˆ AI à¸ˆà¸°à¸¢à¸­à¸¡à¸‚à¸¶à¹‰à¸™à¸¥à¸´à¸Ÿà¸•à¹Œà¹„à¸”à¹‰
    [Header("StartFloor")]
    public int minFloor = 0;
    public List<int> disabledFloors = new List<int>();

    /// <summary>
    /// à¹€à¸Šà¹‡à¸à¸§à¹ˆà¸²à¸¥à¸´à¸Ÿà¸•à¹Œà¸•à¸±à¸§à¸™à¸µà¹‰à¹ƒà¸«à¹‰à¸šà¸£à¸´à¸à¸²à¸£à¸Šà¸±à¹‰à¸™à¸”à¸±à¸‡à¸à¸¥à¹ˆà¸²à¸§à¸«à¸£à¸·à¸­à¹„à¸¡à¹ˆ 
    /// (à¸£à¸§à¸¡à¸–à¸¶à¸‡à¹€à¸Šà¹‡à¸à¸Šà¹ˆà¸§à¸‡ minFloor à¹à¸¥à¸°à¸£à¸²à¸¢à¸à¸²à¸£à¸Šà¸±à¹‰à¸™à¸—à¸µà¹ˆà¸–à¸¹à¸à¸ªà¸±à¹ˆà¸‡à¸›à¸´à¸”)
    /// </summary>
    public bool IsFloorServed(int floor)
    {
        if (floor < 0 || floor >= floorTargets.Length) return false;
        
        // à¸•à¸£à¸§à¸ˆà¸ªà¸­à¸šà¸Šà¹ˆà¸§à¸‡à¸Šà¸±à¹‰à¸™à¸—à¸µà¹ˆà¸¥à¸´à¸Ÿà¸•à¹Œà¸•à¸±à¸§à¸™à¸µà¹‰à¸£à¸­à¸‡à¸£à¸±à¸š (à¸­à¸´à¸‡à¸•à¸²à¸¡ minFloor à¹à¸¥à¸°à¸ˆà¸³à¸™à¸§à¸™ Queue à¸—à¸µà¹ˆà¸§à¸²à¸‡à¹„à¸§à¹‰)
        int localIndex = floor - minFloor;
        if (localIndex < 0 || localIndex >= floorQueues.Length) return false;
        
        // à¸•à¸£à¸§à¸ˆà¸ªà¸­à¸šà¸§à¹ˆà¸²à¸Šà¸±à¹‰à¸™à¸™à¸µà¹‰à¸–à¸¹à¸à¸ªà¸±à¹ˆà¸‡à¸›à¸´à¸”à¸£à¸²à¸¢à¸Šà¸±à¹‰à¸™à¸«à¸£à¸·à¸­à¹„à¸¡à¹ˆ
        if (disabledFloors != null && disabledFloors.Contains(floor)) return false;
        
        return true;
    }

    [Header("Floor Settings")]
    public Transform[] floorTargets;   // à¸•à¸³à¹à¸«à¸™à¹ˆà¸‡à¸žà¸´à¸à¸±à¸” Y à¸‚à¸­à¸‡à¹à¸•à¹ˆà¸¥à¸°à¸Šà¸±à¹‰à¸™
    public List<GameObject> imagelift;
    public List<GameObject> floorLights; // à¸¥à¸²à¸ GameObject à¸¥à¸¹à¸à¹„à¸Ÿà¸‚à¸­à¸‡à¹à¸•à¹ˆà¸¥à¸°à¸Šà¸±à¹‰à¸™à¹ƒà¸ªà¹ˆà¸—à¸µà¹ˆà¸™à¸µà¹ˆ (Index 0 = à¸Šà¸±à¹‰à¸™ 0)
    public float lightThreshold = 0.5f;
    public FloorQueue[] floorQueues;   // à¸£à¸°à¸šà¸šà¸ˆà¸±à¸”à¸à¸²à¸£à¸„à¸´à¸§ Slot à¸«à¸™à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œà¹à¸•à¹ˆà¸¥à¸°à¸Šà¸±à¹‰à¸™

    [Header("Debug")]
    public bool isDebugMode = true;

    // à¸ªà¸–à¸²à¸™à¸°à¸ à¸²à¸¢à¹ƒà¸™
    public List<int> destinationQueue = new List<int>(); // à¸„à¸´à¸§à¹€à¸¥à¸‚à¸Šà¸±à¹‰à¸™à¸—à¸µà¹ˆà¸¥à¸´à¸Ÿà¸•à¹Œà¸•à¹‰à¸­à¸‡à¹„à¸›
    public List<MoveHandleAI> passengers = new List<MoveHandleAI>(); // à¸£à¸²à¸¢à¸Šà¸·à¹ˆà¸­ AI à¸—à¸µà¹ˆà¸­à¸¢à¸¹à¹ˆà¹ƒà¸™à¸¥à¸´à¸Ÿà¸•à¹Œ
    public Dictionary<int, List<MoveHandleAI>> readyAIsOnFloor = new Dictionary<int, List<MoveHandleAI>>(); // AI à¸—à¸µà¹ˆà¸¡à¸²à¸¢à¸·à¸™à¸£à¸­à¸—à¸µà¹ˆà¸ˆà¸¸à¸”à¹€à¸£à¸µà¸¢à¸à¹à¸¥à¹‰à¸§à¸ˆà¸£à¸´à¸‡à¹†

    public ElevatorDirection currentDirection = ElevatorDirection.Idle;
    public bool isMoving = false;
    private Coroutine elevatorRoutine;

    private void Awake()
    {
        baseSpeed = speed;
        // à¹€à¸£à¸´à¹ˆà¸¡à¸•à¹‰à¸™ Dictionary à¸ªà¸³à¸«à¸£à¸±à¸šà¹€à¸à¹‡à¸šà¸„à¸™à¸£à¸­à¹ƒà¸™à¹à¸•à¹ˆà¸¥à¸°à¸Šà¸±à¹‰à¸™
        for (int i = 0; i < floorTargets.Length; i++)
        {
            readyAIsOnFloor[i] = new List<MoveHandleAI>();
        }
    }
    
    public void ApplySpeedMultiplier(float speedMultiplier)
    {
        speed = Mathf.Max(0.1f, baseSpeed * Mathf.Max(0.01f, speedMultiplier));
    }
    private void Update()
    {
        UpdateFloorLights();
    }
    private void UpdateFloorLights()
    {
        if (floorLights == null || floorLights.Count == 0) return;

        for (int i = 0; i < floorTargets.Length; i++)
        {
            if (floorLights[i] == null) continue;

            // à¸„à¸³à¸™à¸§à¸“à¸£à¸°à¸¢à¸°à¸«à¹ˆà¸²à¸‡à¸£à¸°à¸«à¸§à¹ˆà¸²à¸‡à¸¥à¸´à¸Ÿà¸•à¹Œà¸à¸±à¸šà¸ˆà¸¸à¸”à¹€à¸›à¹‰à¸²à¸«à¸¡à¸²à¸¢à¸‚à¸­à¸‡à¹à¸•à¹ˆà¸¥à¸°à¸Šà¸±à¹‰à¸™à¹ƒà¸™à¹à¸™à¸§à¸•à¸±à¹‰à¸‡ (Y)
            float distanceToFloor = Mathf.Abs(transform.position.y - floorTargets[i].position.y);

            // à¸–à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œà¸ˆà¸­à¸”à¸­à¸¢à¸¹à¹ˆà¸—à¸µà¹ˆà¸Šà¸±à¹‰à¸™à¸™à¸µà¹‰ (isMoving à¹€à¸›à¹‡à¸™ false) à¹ƒà¸«à¹‰à¹„à¸Ÿà¸•à¸´à¸”à¸„à¹‰à¸²à¸‡
            if (!isMoving && currentFloor == i)
            {
                floorLights[i].SetActive(true);
            }
            // à¸–à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œà¸à¸³à¸¥à¸±à¸‡à¸§à¸´à¹ˆà¸‡à¸œà¹ˆà¸²à¸™ (Distance à¸™à¹‰à¸­à¸¢à¸à¸§à¹ˆà¸²à¸„à¹ˆà¸²à¸—à¸µà¹ˆà¸à¸³à¸«à¸™à¸”) à¹ƒà¸«à¹‰à¹„à¸Ÿà¸ªà¸§à¹ˆà¸²à¸‡
            else if (distanceToFloor < lightThreshold)
            {
                floorLights[i].SetActive(true);
            }
            // à¸–à¹‰à¸²à¹€à¸¥à¸¢à¹„à¸›à¹à¸¥à¹‰à¸§ à¹ƒà¸«à¹‰à¹„à¸Ÿà¸”à¸±à¸š
            else
            {
                floorLights[i].SetActive(false);
            }
        }
    }
    /// <summary>
    /// AI à¸ˆà¸°à¹€à¸£à¸µà¸¢à¸à¸Ÿà¸±à¸‡à¸à¹Œà¸Šà¸±à¸™à¸™à¸µà¹‰à¹€à¸¡à¸·à¹ˆà¸­à¹€à¸”à¸´à¸™à¸–à¸¶à¸‡à¸£à¸°à¸¢à¸°à¸—à¸µà¹ˆà¸à¸³à¸«à¸™à¸”à¸«à¸™à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œ (WaitSlot)
    /// à¹€à¸›à¹‡à¸™à¸•à¸±à¸§à¸ˆà¸¸à¸”à¸Šà¸™à¸§à¸™à¹ƒà¸«à¹‰à¸¥à¸´à¸Ÿà¸•à¹Œà¹€à¸£à¸´à¹ˆà¸¡à¸—à¸³à¸‡à¸²à¸™ (Process)
    /// </summary>
    public void RegisterGuestReady(MoveHandleAI character)
    {
        int floor = character.currentFloor;
        //AddDestination(floor); // à¸¡à¸±à¹ˆà¸™à¹ƒà¸ˆà¸§à¹ˆà¸²à¸Šà¸±à¹‰à¸™à¸™à¸µà¹‰à¸­à¸¢à¸¹à¹ˆà¹ƒà¸™à¸„à¸´à¸§

        if (!readyAIsOnFloor.ContainsKey(floor))
            readyAIsOnFloor[floor] = new List<MoveHandleAI>();

        if (!readyAIsOnFloor[floor].Contains(character))
        {
            readyAIsOnFloor[floor].Add(character);
  

        }
        // à¹à¸à¹‰à¹„à¸‚à¸•à¸£à¸‡à¸™à¸µà¹‰: à¸–à¹‰à¸²à¸¢à¸±à¸‡à¹„à¸¡à¹ˆà¸¡à¸µ Loop à¸—à¸³à¸‡à¸²à¸™à¸­à¸¢à¸¹à¹ˆ à¸«à¸£à¸·à¸­ Loop à¹€à¸à¹ˆà¸²à¸ˆà¸šà¹„à¸›à¹à¸¥à¹‰à¸§ à¹ƒà¸«à¹‰à¹€à¸£à¸´à¹ˆà¸¡à¹ƒà¸«à¸¡à¹ˆ
        if (elevatorRoutine == null)
        {
            elevatorRoutine = StartCoroutine(ProcessElevatorLoop());
        }

    }
    public void UnregisterGuest(MoveHandleAI character)
    {
        int floor = character.currentFloor;
        if (readyAIsOnFloor.ContainsKey(floor))
        {
            if (readyAIsOnFloor[floor].Contains(character))
            {
                readyAIsOnFloor[floor].Remove(character);
     
            }
        }
    }

    /// <summary>
    /// à¸•à¸£à¸£à¸à¸°à¸•à¸±à¸”à¸ªà¸´à¸™à¹ƒà¸ˆà¹€à¸¥à¸·à¸­à¸à¸Šà¸±à¹‰à¸™à¸–à¸±à¸”à¹„à¸› (Smart Logic)
    /// à¹‚à¸”à¸¢à¸ˆà¸°à¹„à¸¥à¹ˆà¹€à¸à¹‡à¸šà¸„à¸™à¸•à¸²à¸¡à¸—à¸´à¸¨à¸—à¸²à¸‡ (Up/Down) à¸à¹ˆà¸­à¸™à¸„à¸¥à¹‰à¸²à¸¢à¸¥à¸´à¸Ÿà¸•à¹Œà¸ˆà¸£à¸´à¸‡
    /// </summary>
    private int DetermineNextTargetSmart()
    {
        // 1. à¸–à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œà¸§à¹ˆà¸²à¸‡à¹à¸¥à¸°à¹„à¸¡à¹ˆà¸¡à¸µà¸„à¸´à¸§ à¹ƒà¸«à¹‰à¸ˆà¸šà¸‡à¸²à¸™
        if (passengers.Count == 0 && !HasAnyWaitingGuests())
        {
            currentDirection = ElevatorDirection.Idle;
            return -1;
        }

        // 2. à¸–à¹‰à¸²à¸ªà¸–à¸²à¸™à¸°à¹€à¸›à¹‡à¸™ Idle à¹ƒà¸«à¹‰à¹€à¸¥à¸·à¸­à¸à¸—à¸´à¸¨à¸—à¸²à¸‡à¸•à¸²à¸¡à¸„à¸™à¸—à¸µà¹ˆà¹ƒà¸à¸¥à¹‰à¸—à¸µà¹ˆà¸ªà¸¸à¸”
        if (currentDirection == ElevatorDirection.Idle)
        {
            int closestFloor = GetClosestRequestFloor();
            if (closestFloor != -1)
            {
                currentDirection = (closestFloor >= currentFloor) ? ElevatorDirection.Up : ElevatorDirection.Down;
                return closestFloor;
            }
            return -1;
        }

        // 3. SCAN Logic: à¸§à¸´à¹ˆà¸‡à¹„à¸›à¸•à¸²à¸¡à¸—à¸´à¸¨à¸—à¸²à¸‡à¹€à¸”à¸´à¸¡à¸ˆà¸™à¸ªà¸¸à¸”à¸—à¸²à¸‡
        if (currentDirection == ElevatorDirection.Up)
        {
            // à¸«à¸²à¸Šà¸±à¹‰à¸™à¸—à¸µà¹ˆ "à¸ªà¸¹à¸‡à¸à¸§à¹ˆà¸²à¸«à¸£à¸·à¸­à¹€à¸—à¹ˆà¸²à¸à¸±à¸š" à¸›à¸±à¸ˆà¸ˆà¸¸à¸šà¸±à¸™ à¸—à¸µà¹ˆà¸ˆà¸³à¹€à¸›à¹‡à¸™à¸•à¹‰à¸­à¸‡à¸ˆà¸­à¸”
            // à¸•à¹‰à¸­à¸‡à¸ˆà¸­à¸”à¸–à¹‰à¸²: 
            // A. à¸¡à¸µà¸„à¸™à¹ƒà¸™à¸¥à¸´à¸Ÿà¸•à¹Œà¸ˆà¸°à¸¥à¸‡à¸Šà¸±à¹‰à¸™à¸™à¸±à¹‰à¸™
            // B. à¸¡à¸µà¸„à¸™à¸£à¸­à¸—à¸µà¹ˆà¸Šà¸±à¹‰à¸™à¸™à¸±à¹‰à¸™ à¹à¸¥à¸°à¸•à¹‰à¸­à¸‡à¸à¸²à¸£à¸ˆà¸° "à¸‚à¸¶à¹‰à¸™" (à¸—à¸´à¸¨à¹€à¸”à¸µà¸¢à¸§à¸à¸±à¸šà¸¥à¸´à¸Ÿà¸•à¹Œ)
            // C. à¹€à¸›à¹‡à¸™à¸Šà¸±à¹‰à¸™à¸—à¸µà¹ˆà¹„à¸à¸¥à¸—à¸µà¹ˆà¸ªà¸¸à¸”à¸—à¸µà¹ˆà¸¡à¸µà¸„à¸™à¸£à¸­ (à¹à¸¡à¹‰à¹€à¸‚à¸²à¸ˆà¸°à¸¥à¸‡) à¸à¸£à¸“à¸µà¹„à¸¡à¹ˆà¸¡à¸µà¸‡à¸²à¸™à¸­à¸·à¹ˆà¸™à¸—à¸µà¹ˆà¸ªà¸¹à¸‡à¸à¸§à¹ˆà¸²à¸™à¸µà¹‰à¹à¸¥à¹‰à¸§ (à¸ˆà¸¸à¸”à¸§à¸à¸à¸¥à¸±à¸š)

            for (int f = currentFloor; f < floorTargets.Length; f++)
            {
                if (!IsFloorServed(f)) continue; // à¸‚à¹‰à¸²à¸¡à¸Šà¸±à¹‰à¸™à¸—à¸µà¹ˆà¹„à¸¡à¹ˆà¹€à¸ªà¸´à¸£à¹Œà¸Ÿ
                if (f == currentFloor && IsDoorOpening()) continue; // à¸‚à¹‰à¸²à¸¡à¸–à¹‰à¸²à¸à¸³à¸¥à¸±à¸‡à¹€à¸›à¸´à¸”à¸›à¸£à¸°à¸•à¸¹à¸­à¸¢à¸¹à¹ˆà¹à¸¥à¹‰à¸§

                bool someoneGettingOff = passengers.Any(p => p.targetFloor == f);
                bool someoneWantingUp = HasGuestGoing(f, ElevatorDirection.Up);

                if (someoneGettingOff || someoneWantingUp) return f;
            }

            // à¸–à¹‰à¸²à¹„à¸¡à¹ˆà¸¡à¸µà¸‡à¸²à¸™à¸‚à¹‰à¸²à¸‡à¸šà¸™à¹à¸¥à¹‰à¸§ à¹€à¸Šà¹‡à¸„à¸§à¹ˆà¸²à¸¡à¸µà¹ƒà¸„à¸£à¸£à¸­à¸­à¸¢à¸¹à¹ˆà¸‚à¹‰à¸²à¸‡à¸šà¸™à¸ªà¸¸à¸”à¹„à¸«à¸¡ (à¸ˆà¸¸à¸”à¸à¸¥à¸±à¸šà¸£à¸–)
            // à¸«à¸£à¸·à¸­à¸–à¹‰à¸²à¹„à¸¡à¹ˆà¸¡à¸µà¹€à¸¥à¸¢ à¹ƒà¸«à¹‰à¸à¸¥à¸±à¸šà¸—à¸´à¸¨
            if (HasAnyRequestAbove(currentFloor))
            {
                // à¸§à¸´à¹ˆà¸‡à¹„à¸›à¸«à¸²à¸Šà¸±à¹‰à¸™à¸šà¸™à¸ªà¸¸à¸”à¸—à¸µà¹ˆà¸¡à¸µà¸„à¸™à¸£à¸­ (à¹à¸¡à¹‰à¹€à¸‚à¸²à¸ˆà¸°à¸¥à¸‡)
                for (int f = currentFloor + 1; f < floorTargets.Length; f++)
                {
                    if (IsFloorServed(f) && readyAIsOnFloor.ContainsKey(f) && readyAIsOnFloor[f].Count > 0) return f;
                }
            }

            // à¸«à¸¡à¸”à¸‡à¸²à¸™à¸‚à¸²à¸‚à¸¶à¹‰à¸™ -> à¹€à¸›à¸¥à¸µà¹ˆà¸¢à¸™à¹€à¸›à¹‡à¸™à¸‚à¸²à¸¥à¸‡
            currentDirection = ElevatorDirection.Down;
            return DetermineNextTargetSmart(); // à¹€à¸£à¸µà¸¢à¸à¸‹à¹‰à¸³à¸”à¹‰à¸§à¸¢à¸—à¸´à¸¨à¹ƒà¸«à¸¡à¹ˆ
        }
        else // ElevatorDirection.Down
        {
            for (int f = currentFloor; f >= 0; f--)
            {
                if (!IsFloorServed(f)) continue; // à¸‚à¹‰à¸²à¸¡à¸Šà¸±à¹‰à¸™à¸—à¸µà¹ˆà¹„à¸¡à¹ˆà¹€à¸ªà¸´à¸£à¹Œà¸Ÿ
                if (f == currentFloor && IsDoorOpening()) continue;

                bool someoneGettingOff = passengers.Any(p => p.targetFloor == f);
                bool someoneWantingDown = HasGuestGoing(f, ElevatorDirection.Down);

                if (someoneGettingOff || someoneWantingDown) return f;
            }

            if (HasAnyRequestBelow(currentFloor))
            {
                for (int f = currentFloor - 1; f >= 0; f--)
                {
                if (IsFloorServed(f) && readyAIsOnFloor.ContainsKey(f) && readyAIsOnFloor[f].Count > 0) return f;
                }
            }

            currentDirection = ElevatorDirection.Up;
            return DetermineNextTargetSmart();
        }
    }
    // Helpers à¸ªà¸³à¸«à¸£à¸±à¸š Logic à¹ƒà¸«à¸¡à¹ˆ
    private bool IsDoorOpening() => false; // à¹ƒà¸Šà¹‰à¹€à¸Šà¹‡à¸„à¸ªà¸–à¸²à¸™à¸°à¸¥à¸°à¹€à¸­à¸µà¸¢à¸”à¸–à¹‰à¸²à¸ˆà¸³à¹€à¸›à¹‡à¸™

    private bool HasGuestGoing(int floor, ElevatorDirection dir)
    {
        if (!IsFloorServed(floor) || !readyAIsOnFloor.ContainsKey(floor)) return false;
        foreach (var g in readyAIsOnFloor[floor])
        {
            // à¸„à¸™à¸—à¸µà¹ˆà¸£à¸­à¸­à¸¢à¸¹à¹ˆ à¸•à¹‰à¸­à¸‡à¸à¸²à¸£à¹„à¸›à¸—à¸´à¸¨à¹€à¸”à¸µà¸¢à¸§à¸à¸±à¸šà¸—à¸µà¹ˆà¹€à¸Šà¹‡à¸„à¸«à¸£à¸·à¸­à¹„à¸¡à¹ˆ à¹à¸¥à¸°à¸Šà¸±à¹‰à¸™à¹€à¸›à¹‰à¸²à¸«à¸¡à¸²à¸¢à¸•à¹‰à¸­à¸‡à¹€à¸ªà¸´à¸£à¹Œà¸Ÿà¸”à¹‰à¸§à¸¢
            if (dir == ElevatorDirection.Up && g.targetFloor > floor && IsFloorServed(g.targetFloor)) return true;
            if (dir == ElevatorDirection.Down && g.targetFloor < floor && IsFloorServed(g.targetFloor)) return true;
        }
        return false;
    }

    private bool HasAnyRequestAbove(int floor)
    {
        // à¸¡à¸µà¸„à¸™à¸£à¸­à¸­à¸¢à¸¹à¹ˆà¸Šà¸±à¹‰à¸™à¸—à¸µà¹ˆà¸ªà¸¹à¸‡à¸à¸§à¹ˆà¸²à¸™à¸µà¹‰à¹„à¸«à¸¡
        for (int f = floor + 1; f < floorTargets.Length; f++)
        {
            if (IsFloorServed(f) && readyAIsOnFloor.ContainsKey(f) && readyAIsOnFloor[f].Count > 0) return true;
        }
        return false;
    }

    private bool HasAnyRequestBelow(int floor)
    {
        for (int f = floor - 1; f >= 0; f--)
        {
            if (IsFloorServed(f) && readyAIsOnFloor.ContainsKey(f) && readyAIsOnFloor[f].Count > 0) return true;
        }
        return false;
    }

    private bool HasAnyWaitingGuests()
    {
        foreach (var list in readyAIsOnFloor.Values) if (list.Count > 0) return true;
        return false;
    }
    private int GetClosestRequestFloor()
    {
        int closest = -1;
        int minDist = int.MaxValue;

        // à¹€à¸Šà¹‡à¸„à¸„à¸™à¸£à¸­à¸‚à¹‰à¸²à¸‡à¸™à¸­à¸
        for (int f = 0; f < floorTargets.Length; f++)
        {
            if (IsFloorServed(f) && readyAIsOnFloor.ContainsKey(f) && readyAIsOnFloor[f].Count > 0)
            {
                int dist = Mathf.Abs(currentFloor - f);
                if (dist < minDist) { minDist = dist; closest = f; }
            }
        }
        return closest;
    }

    /// <summary>
    /// Coroutine à¸«à¸¥à¸±à¸à¸—à¸µà¹ˆà¸„à¸§à¸šà¸„à¸¸à¸¡à¸§à¸‡à¸ˆà¸£à¸à¸²à¸£à¸—à¸³à¸‡à¸²à¸™à¸‚à¸­à¸‡à¸¥à¸´à¸Ÿà¸•à¹Œà¸•à¸£à¸²à¸šà¹ƒà¸”à¸—à¸µà¹ˆà¸¢à¸±à¸‡à¸¡à¸µà¸Šà¸±à¹‰à¸™à¸­à¸¢à¸¹à¹ˆà¹ƒà¸™à¸„à¸´à¸§
    /// à¸ˆà¸°à¸ªà¸±à¹ˆà¸‡à¹ƒà¸«à¹‰à¸¥à¸´à¸Ÿà¸•à¹Œà¹€à¸„à¸¥à¸·à¹ˆà¸­à¸™à¸—à¸µà¹ˆ (Move) à¹à¸¥à¸° à¹€à¸›à¸´à¸”à¸›à¸£à¸°à¸•à¸¹ (OpenDoors) à¸§à¸™à¹„à¸›à¹€à¸£à¸·à¹ˆà¸­à¸¢à¹†
    /// </summary>
    public IEnumerator ProcessElevatorLoop()
    {
        // à¸à¸±à¸™à¹€à¸«à¸™à¸µà¸¢à¸§: à¸–à¹‰à¸²à¹€à¸‚à¹‰à¸²à¸¡à¸²à¹à¸¥à¹‰à¸§ isMoving à¹€à¸›à¹‡à¸™ true à¸­à¸¢à¸¹à¹ˆà¹à¸¥à¹‰à¸§à¹ƒà¸«à¹‰à¸ˆà¸š (à¸›à¹‰à¸­à¸‡à¸à¸±à¸™à¸‹à¹‰à¸­à¸™)
        if (isMoving)
        {
            elevatorRoutine = null;
            yield break;
        }
        isMoving = true;

        // Loop à¸•à¸£à¸²à¸šà¹ƒà¸”à¸—à¸µà¹ˆà¸¡à¸µà¸„à¸´à¸§ à¸«à¸£à¸·à¸­ à¸¡à¸µà¸œà¸¹à¹‰à¹‚à¸”à¸¢à¸ªà¸²à¸£à¸„à¹‰à¸²à¸‡à¸­à¸¢à¸¹à¹ˆ
        while (true)
        {
            // Update Destination Queue for safety (à¸à¸±à¸™à¹€à¸«à¸™à¸µà¸¢à¸§)
            if (passengers.Count > 0)
            {
                foreach (var p in passengers) AddDestination(p.targetFloor);
            }

            int targetFloor = DetermineNextTargetSmart();

            // à¸–à¹‰à¸²à¹„à¸¡à¹ˆà¸¡à¸µà¹€à¸›à¹‰à¸²à¸«à¸¡à¸²à¸¢à¹à¸¥à¹‰à¸§ à¹ƒà¸«à¹‰à¸ˆà¸šà¸à¸²à¸£à¸—à¸³à¸‡à¸²à¸™
            if (targetFloor == -1) break;

            if (currentFloor != targetFloor)
            {
                yield return StartCoroutine(MoveToFloor(targetFloor));
                currentFloor = targetFloor;
            }

            yield return StartCoroutine(OpenDoors());
            yield return new WaitForSeconds(0.2f);
        }

        isMoving = false;
        currentDirection = ElevatorDirection.Idle; 
        elevatorRoutine = null; 
    }

    /// <summary>
    /// à¸„à¸§à¸šà¸„à¸¸à¸¡à¸à¸²à¸£à¹€à¸„à¸¥à¸·à¹ˆà¸­à¸™à¸—à¸µà¹ˆà¸‚à¸­à¸‡à¸•à¸±à¸§à¸¥à¸´à¸Ÿà¸•à¹Œà¹ƒà¸™à¹à¸™à¸§à¸•à¸±à¹‰à¸‡ (Y-Axis) à¹„à¸›à¸¢à¸±à¸‡à¸•à¸³à¹à¸«à¸™à¹ˆà¸‡à¸‚à¸­à¸‡à¸Šà¸±à¹‰à¸™à¹€à¸›à¹‰à¸²à¸«à¸¡à¸²à¸¢
    /// </summary>
    private IEnumerator MoveToFloor(int floor)
    {
        if (isDebugMode) Debug.Log($"<color=blue>Elevator: à¹€à¸„à¸¥à¸·à¹ˆà¸­à¸™à¸—à¸µà¹ˆà¹„à¸›à¸Šà¸±à¹‰à¸™ {floor}</color>");
        float targetY = floorTargets[floor].position.y;
        Vector3 targetPos = new Vector3(transform.position.x, targetY, transform.position.z);

        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
    }

    /// <summary>
    /// à¸ˆà¸±à¸”à¸à¸²à¸£à¹€à¸«à¸•à¸¸à¸à¸²à¸£à¸“à¹Œà¹€à¸¡à¸·à¹ˆà¸­à¸›à¸£à¸°à¸•à¸¹à¸¥à¸´à¸Ÿà¸•à¹Œà¹€à¸›à¸´à¸”: 
    /// 1. à¸›à¸¥à¹ˆà¸­à¸¢à¸œà¸¹à¹‰à¹‚à¸”à¸¢à¸ªà¸²à¸£à¸—à¸µà¹ˆà¸•à¹‰à¸­à¸‡à¸à¸²à¸£à¸¥à¸‡à¸Šà¸±à¹‰à¸™à¸™à¸µà¹‰à¸­à¸­à¸ 
    /// 2. à¸•à¸£à¸§à¸ˆà¸ªà¸­à¸šà¸£à¸°à¸¢à¸°à¸«à¹ˆà¸²à¸‡à¸‚à¸­à¸‡à¸„à¸™à¸£à¸­ à¹à¸¥à¸°à¸£à¸±à¸šà¸œà¸¹à¹‰à¹‚à¸”à¸¢à¸ªà¸²à¸£à¹ƒà¸«à¸¡à¹ˆà¹€à¸‚à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œ
    /// </summary>
    private IEnumerator OpenDoors()
    {
        if (imagelift != null && currentFloor < imagelift.Count)
        {
            if (imagelift[currentFloor] != null)
                imagelift[currentFloor].SetActive(false);
        }
        if (isDebugMode) Debug.Log($"<color=orange>Elevator: à¹€à¸›à¸´à¸”à¸›à¸£à¸°à¸•à¸¹à¸Šà¸±à¹‰à¸™ {currentFloor}</color>");

        // 1. à¸ªà¹ˆà¸‡à¸„à¸™à¸­à¸­à¸
        for (int i = passengers.Count - 1; i >= 0; i--)
        {
            if (passengers[i].targetFloor == currentFloor)
            {
                passengers[i].ExitElevator();
                passengers.RemoveAt(i);
                yield return new WaitForSeconds(0.3f);
            }
        }

        // 2. à¸£à¸±à¸šà¸„à¸™à¹€à¸‚à¹‰à¸² (à¹€à¸‰à¸žà¸²à¸°à¸„à¸™à¸—à¸µà¹ˆà¹„à¸›à¸—à¸²à¸‡à¹€à¸”à¸µà¸¢à¸§à¸à¸±à¸™ à¸«à¸£à¸·à¸­à¸–à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œà¸§à¹ˆà¸²à¸‡/à¹€à¸›à¸¥à¸µà¹ˆà¸¢à¸™à¸—à¸´à¸¨à¸à¹‡à¸£à¸±à¸šà¸«à¸¡à¸”)
        if (readyAIsOnFloor.ContainsKey(currentFloor))
        {
            List<MoveHandleAI> waitingList = readyAIsOnFloor[currentFloor];

            // à¸§à¸™à¸¥à¸¹à¸›à¸¢à¹‰à¸­à¸™à¸«à¸¥à¸±à¸‡à¹€à¸žà¸·à¹ˆà¸­à¸„à¸§à¸²à¸¡à¸›à¸¥à¸­à¸”à¸ à¸±à¸¢à¹ƒà¸™à¸à¸²à¸£ Remove
            for (int i = waitingList.Count - 1; i >= 0; i--)
            {
                if (passengers.Count >= maxCapacity) break;

                MoveHandleAI character = waitingList[i];

                // --- Logic à¹€à¸¥à¸·à¸­à¸à¸£à¸±à¸šà¸„à¸™ ---
                bool shouldPickUp = false;

                // à¸–à¹‰à¸²à¸™à¸µà¹ˆà¸„à¸·à¸­à¸ˆà¸¸à¸”à¸à¸¥à¸±à¸šà¸£à¸– à¸«à¸£à¸·à¸­à¸¥à¸´à¸Ÿà¸•à¹Œà¸§à¹ˆà¸²à¸‡ -> à¸£à¸±à¸šà¸«à¸¡à¸”
                bool isTurningPoint = !HasAnyRequestAbove(currentFloor) && currentDirection == ElevatorDirection.Up;
                if (currentDirection == ElevatorDirection.Down && !HasAnyRequestBelow(currentFloor)) isTurningPoint = true;

                if (isTurningPoint || passengers.Count == 0)
                {
                    shouldPickUp = true;
                    // à¸­à¸±à¸›à¹€à¸”à¸•à¸—à¸´à¸¨à¸—à¸²à¸‡à¸¥à¸´à¸Ÿà¸•à¹Œà¸•à¸²à¸¡à¸„à¸™à¹à¸£à¸à¸—à¸µà¹ˆà¸£à¸±à¸šà¸–à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œà¸§à¹ˆà¸²à¸‡
                    if (passengers.Count == 0)
                    {
                        currentDirection = (character.targetFloor > currentFloor) ? ElevatorDirection.Up : ElevatorDirection.Down;
                    }
                }
                else
                {
                    // à¸£à¸±à¸šà¹€à¸‰à¸žà¸²à¸°à¸„à¸™à¹„à¸›à¸—à¸²à¸‡à¹€à¸”à¸µà¸¢à¸§à¸à¸±à¸™
                    if (currentDirection == ElevatorDirection.Up && character.targetFloor > currentFloor) shouldPickUp = true;
                    else if (currentDirection == ElevatorDirection.Down && character.targetFloor < currentFloor) shouldPickUp = true;
                }

                if (shouldPickUp)
                {
                    if (character.assignedElevator != this)
                    {
                        continue; // à¸–à¹‰à¸²à¹„à¸¡à¹ˆà¹ƒà¸Šà¹ˆà¸¥à¸´à¸Ÿà¸•à¹Œà¸—à¸µà¹ˆà¸Šà¸±à¹‰à¸™à¸ªà¹ˆà¸‡à¹„à¸›à¹€à¸£à¸µà¸¢à¸ à¸«à¹‰à¸²à¸¡à¸£à¸±à¸š!
                    }
                    // à¸£à¸­à¸ˆà¸™à¸à¸§à¹ˆà¸² AI à¸ˆà¸°à¹€à¸”à¸´à¸™à¸¡à¸²à¸–à¸¶à¸‡à¸«à¸™à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œà¸ˆà¸£à¸´à¸‡à¹† (à¹à¸à¹‰à¸šà¸±à¸„à¸•à¸±à¸§à¸—à¸´à¸žà¸¢à¹Œ)
                    float waitTime = 3f;
                    yield return new WaitUntil(() => {
                        waitTime -= Time.deltaTime;
                        // à¸–à¹‰à¸² AI à¹€à¸›à¸¥à¸µà¹ˆà¸¢à¸™à¹€à¸›à¹‰à¸²à¸«à¸¡à¸²à¸¢à¹„à¸›à¹à¸¥à¹‰à¸§ à¹ƒà¸«à¹‰à¸¢à¸à¹€à¸¥à¸´à¸à¸à¸²à¸£à¸£à¸­
                        if (character.assignedElevator != this || character.travelState != TravelState.WaitAtSlot) return true;
                        float dist = Vector2.Distance(character.transform.position, transform.position);
                        return dist < distacneWaitslottolift || waitTime <= 0;
                    });

                    // à¹€à¸Šà¹‡à¸„à¸­à¸µà¸à¸„à¸£à¸±à¹‰à¸‡à¹€à¸œà¸·à¹ˆà¸­à¸à¸£à¸“à¸µà¹€à¸›à¸¥à¸µà¹ˆà¸¢à¸™à¹€à¸›à¹‰à¸²à¸«à¸¡à¸²à¸¢à¸£à¸°à¸«à¸§à¹ˆà¸²à¸‡à¸£à¸­
                    if (character.assignedElevator != this || character.travelState != TravelState.WaitAtSlot) continue;

                    if (isDebugMode) Debug.Log($"<color=green>Elevator: à¸£à¸±à¸š {character.name} (à¹„à¸› {character.targetFloor})</color>");

                    // à¸ªà¸±à¹ˆà¸‡à¹€à¸‚à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œà¹à¸¥à¸°à¸£à¸­à¸ˆà¸™à¸à¸§à¹ˆà¸²à¸ˆà¸° Parent à¹€à¸ªà¸£à¹‡à¸ˆ
                    character.EnterElevator(this.transform);
                    passengers.Add(character);
                    waitingList.RemoveAt(i);

                    // *** à¸ªà¸³à¸„à¸±à¸: à¸£à¸­à¹ƒà¸«à¹‰ AI à¸¢à¹‰à¸²à¸¢ Parent à¹€à¸ªà¸£à¹‡à¸ˆà¸ˆà¸£à¸´à¸‡à¹† ***
                    yield return new WaitForSeconds(0.4f);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        if (imagelift != null && currentFloor < imagelift.Count)
        {
            if (imagelift[currentFloor] != null)
                imagelift[currentFloor].SetActive(true);
        }
    }

    public void RequestElevator(MoveHandleAI character, int fromFloor, int toFloor)
    {
        // Function à¸™à¸µà¹‰à¹ƒà¸Šà¹‰à¹à¸„à¹ˆ Log à¹ƒà¸™à¸£à¸°à¸šà¸šà¹ƒà¸«à¸¡à¹ˆ à¹€à¸žà¸£à¸²à¸° Logic à¸­à¸¢à¸¹à¹ˆà¹ƒà¸™ RegisterGuestReady à¹à¸¥à¸° DetermineNextTarget à¸«à¸¡à¸”à¹à¸¥à¹‰à¸§
        if (isDebugMode) Debug.Log($"<color=yellow>Request: {character.name} {fromFloor}->{toFloor}</color>");
    }

    /// <summary>
    /// à¸Ÿà¸±à¸‡à¸à¹Œà¸Šà¸±à¸™à¸ à¸²à¸¢à¹ƒà¸™à¸ªà¸³à¸«à¸£à¸±à¸šà¸ˆà¸±à¸”à¸à¸²à¸£à¸à¸²à¸£à¹€à¸žà¸´à¹ˆà¸¡à¹€à¸¥à¸‚à¸Šà¸±à¹‰à¸™à¸¥à¸‡à¹ƒà¸™à¸„à¸´à¸§à¹à¸¥à¸°à¸—à¸³à¸à¸²à¸£à¹€à¸£à¸µà¸¢à¸‡à¸¥à¸³à¸”à¸±à¸šà¸Šà¸±à¹‰à¸™ (Sort) à¹ƒà¸«à¹‰à¸–à¸¹à¸à¸•à¹‰à¸­à¸‡
    /// </summary>
    private void AddDestination(int floor)
    {
        if (floor < 0 || floor >= floorTargets.Length || !IsFloorServed(floor)) return;
        if (!destinationQueue.Contains(floor))
        {
            destinationQueue.Add(floor);
            destinationQueue.Sort();
        }
    }

    /// <summary>
    /// à¸•à¸£à¸§à¸ˆà¸ªà¸­à¸šà¹à¸¥à¸°à¸ˆà¸­à¸‡à¸•à¸³à¹à¸«à¸™à¹ˆà¸‡à¸ˆà¸¸à¸”à¸£à¸­ (Wait Slot) à¸«à¸™à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œà¸ªà¸³à¸«à¸£à¸±à¸š AI à¹à¸•à¹ˆà¸¥à¸°à¸•à¸±à¸§
    /// </summary>
    public Transform RequestWaitSlot(MoveHandleAI character, int floor, out int slotIndex)
    {
        int localIndex = floor - minFloor;
        if (localIndex >= 0 && localIndex < floorQueues.Length)
            return floorQueues[localIndex].GetAvailableSlot(character, out slotIndex);
        slotIndex = -1;
        return null;
    }

    /// <summary>
    /// à¸„à¸·à¸™à¸ªà¸´à¸—à¸˜à¸´à¹Œà¸à¸²à¸£à¹ƒà¸Šà¹‰à¸‡à¸²à¸™à¸ˆà¸¸à¸”à¸£à¸­ (Wait Slot) à¹€à¸¡à¸·à¹ˆà¸­ AI à¸­à¸­à¸à¸ˆà¸²à¸à¸ˆà¸¸à¸”à¸™à¸±à¹‰à¸™à¸«à¸£à¸·à¸­à¹€à¸‚à¹‰à¸²à¸¥à¸´à¸Ÿà¸•à¹Œà¹„à¸›à¹à¸¥à¹‰à¸§
    /// </summary>
    public void ReleaseSlot(int floor, int index)
    {
        int localIndex = floor - minFloor;
        if (localIndex >= 0 && localIndex < floorQueues.Length)
            floorQueues[localIndex].ReleaseSlot(index);
    }
}

