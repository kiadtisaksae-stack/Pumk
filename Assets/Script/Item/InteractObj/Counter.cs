using UnityEngine;

public class Counter : CanInteractObj
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int currentMoney;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<GuestAI>(out var guest))
        {
            Debug.Log(guest + "ชำระเงินเรียบร้อย");
            currentMoney += guest.rentNet;
            Destroy(guest.gameObject);
            interactObjData.objCollider.isTrigger = false;
        }
    }
}
