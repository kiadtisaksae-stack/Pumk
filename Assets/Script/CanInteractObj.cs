using UnityEngine;

public class CanInteractObj : MonoBehaviour
{
    [SerializeField]
    private InteractObjData _interactObjData;
    public InteractObjData interactObjData => _interactObjData;

}
