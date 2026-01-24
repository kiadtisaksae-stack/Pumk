using UnityEngine;

public enum GuestType
{
    A, B, C
}

public class GuestAI : MoveHandleAI
{
    public GuestType guestType = GuestType.A;
    public Guestphase guestPhase = Guestphase.CheckingIn;

    // เพิ่ม Logic เฉพาะแขก เช่น ความพึงพอใจ หรือการจ่ายเงิน
    public override void ExitElevator()
    {
        base.ExitElevator();
        Debug.Log($"<color=green>แขก {gameObject.name}: ถึงชั้นเป้าหมายแล้ว กำลังเดินไปห้อง</color>");
    }
}