using NUnit.Framework.Interfaces;
using UnityEngine;
[RequireComponent(typeof(Collider2D))]

public class CanInteractObj : MonoBehaviour 
{
    [SerializeField]
    private InteractObjData _interactObjData;
    public InteractObjData interactObjData => _interactObjData;
    [SerializeField]
    protected Emplyee targetEmplyee;
    public bool checkDistance;

    protected virtual void Awake()
    {
        interactObjData.ObjPosition = this.transform;
        interactObjData.objCollider = GetComponent<Collider2D>();
        interactObjData.objCollider.isTrigger = false;
    }


    public void AddEmployee(Emplyee emplyee)
    {
        targetEmplyee = emplyee;
        checkDistance = true;
        AcitvateTrigger();
    }

    public void AcitvateTrigger()
    {
        interactObjData.objCollider.isTrigger = true;
    }


    protected virtual void Update()
    {
        if (targetEmplyee == null) return;
        float distSqr = (targetEmplyee.transform.position - transform.position).sqrMagnitude;

        // เอาค่าระยะที่ต้องการมายกกำลังสองก่อนเทียบ
        if (distSqr <= (0.2 * 0.2) && checkDistance)
        {
            Debug.Log("ถึงเป้าหมาย");
            checkDistance = false;
            targetEmplyee = null;
            return;
        }
    }


}
