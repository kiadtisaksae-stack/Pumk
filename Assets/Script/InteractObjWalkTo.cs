using UnityEngine;
using DG.Tweening;


public class InteractObjWalkTo : MonoBehaviour 
{
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    if (eventData.button == PointerEventData.InputButton.Left)
    //    {
    //        ProcessAssignment(eventData);
    //    }
    //}

    //private void ProcessAssignment(PointerEventData eventData)
    //{
    //    Ray ray = Camera.main.ScreenPointToRay(eventData.position);
    //    RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

    //    if (hit.collider != null && hit.collider.TryGetComponent<CanInteractObj>(out var obj))
    //    {
    //        Tweening();
    //        if (targetEmplyee != null)
    //        {
    //            targetEmplyee.finalRoomData = obj.interactObjData.roomData;
    //            Debug.Log($"<color=green>พนักงาน {targetEmplyee.name}</color> เดินทาง");

    //            onTravel.Invoke(targetEmplyee);
    //            targetEmplyee.StartTravel(obj.interactObjData);
    //        }
    //    }
    //    else
    //    {
    //        Debug.Log("ลากไปตกนอกห้อง");
    //        //transform.localPosition = _originalPosition;
    //    }
    //}
    
}
