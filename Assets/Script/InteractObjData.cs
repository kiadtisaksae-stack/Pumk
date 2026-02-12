using UnityEngine;

[System.Serializable]
public class InteractObjData 
{
    [Header("Obj Info")]
    public int floorNumber;
    public Transform ObjPosition;
    public Collider2D objCollider;

    [Header("RoomData")]
    public RoomData roomData;

    [Header("Item")]
    public ItemSO itemData;

}
