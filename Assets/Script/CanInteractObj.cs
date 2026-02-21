using NUnit.Framework.Interfaces;
using UnityEngine;
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(InteractObjWalkTo))]

public class CanInteractObj : MonoBehaviour ,IInteractable
{
    [SerializeField]
    private InteractObjData _interactObjData;
    public InteractObjData interactObjData => _interactObjData;

    protected virtual void Awake()
    {
        interactObjData.ObjPosition = this.transform;
        interactObjData.objCollider = GetComponent<Collider2D>();
        interactObjData.objCollider.isTrigger = true;
        
    }

    public virtual void Interact(MoveHandleAI actor)
    {
        
    }



}
