using UnityEngine;

/// <summary>
/// จุดที่ Franken เดินมาตอน sleepwalk
/// Player click ที่นี่ → StartTravel มาหา → OnTriggerEnter2D → WakeUp Franken
/// alpha = 0 ไว้ ระบบเปิด/ปิด SetActive จัดการ visibility
/// </summary>
public class FFSleepwalk : CanInteractObj
{
    // Franken ที่กำลัง sleepwalk อยู่ที่จุดนี้ — FrankenGuest set ให้ตอน StartTravel
    [HideInInspector] public FrankenGuest owner;

    public override void  Interact(MoveHandleAI actor)
    {
        if (owner == null || !owner.IsSleepwalking) return;
        actor.StartTravel(interactObjData);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (owner == null || !owner.IsSleepwalking) return;
        if (!collision.CompareTag("Player")) return;

        owner.WakeUp();
    }
}