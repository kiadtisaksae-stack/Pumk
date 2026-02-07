using UnityEngine;
using UnityEngine.EventSystems;

public class InteractObjWalkTo : MonoBehaviour , IPointerClickHandler
{
    private ElevatorController hotelElevator;
    private Emplyee targetEmplyee;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetEmplyee = FindObjectOfType<Emplyee>();
        hotelElevator = FindObjectOfType<ElevatorController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            ProcessAssignment(eventData);
        }
    }

    private void ProcessAssignment(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

        if (hit.collider != null && hit.collider.TryGetComponent<CanInteractObj>(out var obj))
        {
           
            if (targetEmplyee != null)
            {
                Debug.Log($"<color=green>พนักงาน {targetEmplyee.name}</color> เดินทาง");

                targetEmplyee.StartTravel(obj.interactObjData, hotelElevator);
            }
        }
        else
        {
            Debug.Log("ลากไปตกนอกห้อง");
            //transform.localPosition = _originalPosition;
        }
    }
}
