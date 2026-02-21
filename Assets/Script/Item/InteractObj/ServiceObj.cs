using UnityEngine;

public class ServiceObj : CanInteractObj
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public ItemSO serviceItem;
    public bool isLuggage;

    void Start()
    {

    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.TryGetComponent<Player>(out var player);
            player.AddItem(serviceItem);
            if (isLuggage)
            {
                Destroy(gameObject);
            }
            Debug.Log("Player entered the zone!");
        }
        if(other.CompareTag("Employee"))
        {
            other.TryGetComponent<Employee>(out var employee);
            employee.AddItem(serviceItem);
            if (isLuggage)
            {
                Destroy(gameObject);
            }
            Debug.Log("NPC entered the zone!");
        }

    }

    public override void Interact(MoveHandleAI actor)
    {
        actor.StartTravel(interactObjData);
       
    }
}
