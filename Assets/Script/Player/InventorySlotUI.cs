using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private int slotIndex;
    [SerializeField] private float doubleClickInterval = 0.3f;
    [SerializeField] private bool allowRightClickRemove = true;

    private Player owner;
    private float lastClickTime = -10f;

    public int SlotIndex => slotIndex;

    public void Setup(Player player, int index)
    {
        owner = player;
        slotIndex = index;
    }

    public bool IsOwnedBy(Player player)
    {
        return owner == null || owner == player;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null) return;
        if (owner == null) return;

        // PC: allow right-click to remove item immediately.
        if (allowRightClickRemove && eventData.button == PointerEventData.InputButton.Right)
        {
            owner.DestroyItemAtSlot(slotIndex);
            lastClickTime = -10f;
            return;
        }

        // Mobile/Touch and default flow: left-button double click.
        if (eventData.button != PointerEventData.InputButton.Left) return;

        bool isDoubleClick = eventData.clickCount >= 2;
        if (!isDoubleClick)
        {
            float now = Time.unscaledTime;
            isDoubleClick = (now - lastClickTime) <= doubleClickInterval;
            lastClickTime = now;
        }
        else
        {
            lastClickTime = -10f;
        }

        if (isDoubleClick)
        {
            owner.DestroyItemAtSlot(slotIndex);
        }
    }
}
