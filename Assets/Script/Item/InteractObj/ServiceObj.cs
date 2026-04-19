using UnityEngine;

public class ServiceObj : CanInteractObj
{
    public ItemSO serviceItem;

    [Header("Special Mode")]
    public bool isLaundryDropOff;

    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player == null) return;
            if (player.targetIObj != interactObjData) return;

            player.targetIObj = null;

            if (isLaundryDropOff)
            {
                RemoveAllLaundryFromPlayer(player);
                player.travelState = TravelState.Idle;
                return;
            }

            player.AddItem(serviceItem);
            player.travelState = TravelState.Idle;
        }

        if (other.CompareTag("Employee"))
        {
            Employee employee = other.GetComponent<Employee>();
            if (employee == null) return;
            if (employee.targetIObj != interactObjData) return;

            employee.targetIObj = null;

            if (isLaundryDropOff)
            {
                RemoveAllLaundryFromEmployee(employee);
                employee.travelState = TravelState.Idle;
                return;
            }

            employee.AddItem(serviceItem);
            employee.travelState = TravelState.Idle;
        }
    }

    public override void Interact(MoveHandleAI actor)
    {
        base.Interact(actor);
        actor.StartTravel(interactObjData);
    }

    private void RemoveAllLaundryFromPlayer(Player player)
    {
        if (player == null || player.inventory == null) return;

        bool removed = false;
        for (int i = player.inventory.Count - 1; i >= 0; i--)
        {
            ItemSO item = player.inventory[i];
            if (item != null && item.requiredForService == ServiceRequestType.Laundry)
            {
                player.inventory.RemoveAt(i);
                removed = true;
            }
        }

        if (removed)
        {
            player.RefreshInventoryUI();
        }
    }

    private void RemoveAllLaundryFromEmployee(Employee employee)
    {
        if (employee == null || employee.inventory == null) return;

        for (int i = employee.inventory.Count - 1; i >= 0; i--)
        {
            ItemSO item = employee.inventory[i];
            if (item != null && item.requiredForService == ServiceRequestType.Laundry)
            {
                employee.inventory.RemoveAt(i);
            }
        }
    }
}
