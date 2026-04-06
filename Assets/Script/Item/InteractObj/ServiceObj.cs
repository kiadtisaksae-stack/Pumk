using UnityEngine;

public class ServiceObj : CanInteractObj
{
    public ItemSO serviceItem;

    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player == null) return;
            // เช็กว่า player เดินมาหาตัวนี้จริงไหม
            if (player.targetIObj != interactObjData) return;

            // สำคัญมาก: เปลี่ยน target ให้ว่าง เพื่อกันไม่ให้เก็บรัวๆ แบบอัตโนมัติ แต่อนุญาตให้คลิกเพื่อเก็บแบบเจาะจงซ้ำได้
            player.targetIObj = null;

            player.AddItem(serviceItem);
            player.travelState = TravelState.Idle;
        }

        if (other.CompareTag("Employee"))
        {
            Employee employee = other.GetComponent<Employee>();
            if (employee == null) return;
            if (employee.targetIObj != interactObjData) return;

            employee.targetIObj = null;
            employee.AddItem(serviceItem);
            employee.travelState = TravelState.Idle;
        }
    }

    public override void Interact(MoveHandleAI actor)
    {
        base.Interact(actor);
        actor.StartTravel(interactObjData);
    }
}