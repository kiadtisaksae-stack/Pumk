using UnityEngine;

public class ExitDoor : CanInteractObj
{
    private int currentGuestExitCount;
    private LevelManager levelManager;

    public override void Start()
    {
        base.Start();
        levelManager = FindAnyObjectByType<LevelManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<GuestAI>(out var guest))
        {
            if (guest.guestPhase == Guestphase.CheckingOut) 
            {

                RefreshExitCount();
                Destroy(guest.gameObject);
            }
        }
    }

    public void RefreshExitCount()
    {
        currentGuestExitCount++;
        if (currentGuestExitCount == levelManager.guestQuitCount)
        {
            levelManager.EndLevel();
        }
    }
}
