using UnityEngine;

public class LuggageService : ServiceObj
{
    protected override void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player == null) return;
            // เช็กว่า player เดินมาหาตัวนี้จริงไหม
            if (player.targetIObj != interactObjData) return;

            player.AddItem(serviceItem);
            player.travelState = TravelState.Idle;
            RoomManager.Instance.DeleteLuggage();
        }

        if (other.CompareTag("Employee"))
        {
            Employee employee = other.GetComponent<Employee>();
            if (employee == null) return;
            if (employee.targetIObj != interactObjData) return;

            employee.AddItem(serviceItem);
            employee.travelState = TravelState.Idle;
            RoomManager.Instance.DeleteLuggage();

        }
    }

    public override void Interact(MoveHandleAI actor)
    {
        base.Interact(actor);
        actor.StartTravel(interactObjData);
    }
}
