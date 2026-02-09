using UnityEngine;

public class ServiceObj : CanInteractObj
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ItemSO serviceItem;


    void Start()
    {

    }

    // Update is called once per frame
    protected override void Update()
    {
        if (targetEmplyee == null) return;
        float distSqr = (targetEmplyee.transform.position - transform.position).sqrMagnitude;

        // เอาค่าระยะที่ต้องการมายกกำลังสองก่อนเทียบ
        if (distSqr <= (0.2 * 0.2) && checkDistance)
        {
            Debug.Log("ถึงเป้าหมาย");
            PickUp(serviceItem);
            checkDistance = false;
            
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the zone!");
            interactObjData.objCollider.isTrigger = false;
        }
    }

    public virtual void PickUp(ItemSO item)
    {
        targetEmplyee.AddItem(item);
    }
}
