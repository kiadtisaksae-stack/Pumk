using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public Action<Vector2> OnClickPosition;
    public Action<Vector2> OnDoubleClickPosition;

    private void OnClick(InputValue value)
    {
        if (!value.isPressed || Pointer.current == null) return;

        Vector2 pointerPosition = Pointer.current.position.ReadValue();
        OnClickPosition?.Invoke(pointerPosition);
    }

    private void OnDoubleClick(InputValue value)
    {
        if (!value.isPressed || Pointer.current == null) return;

        Vector2 pointerPosition = Pointer.current.position.ReadValue();
        OnDoubleClickPosition?.Invoke(pointerPosition);
    }
}
