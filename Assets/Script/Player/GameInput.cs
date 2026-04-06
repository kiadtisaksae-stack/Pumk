using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public Action<Vector2> OnClickPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnClick(InputValue value)
    {
        if (value.isPressed)
        {
            // เปลี่ยนจาก Mouse เป็น Pointer เพื่อให้รองรับทั้ง นิ้วสัมผัส (Touch) และ เมาส์ (Mouse)
            Vector2 pointerPosition = Pointer.current.position.ReadValue();

            OnClickPosition?.Invoke(pointerPosition);
        }
    }
}
