using UnityEngine;

public class ExitDoor : CanInteractObj
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<GuestAI>(out var guest))
        {
            if (guest.guestPhase == Guestphase.CheckingOut) 
            {
                Debug.Log(guest + " ออกไปแบบไม่จ่ายตัง");
                Destroy(guest.gameObject);
            }
        }
    }
}
