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
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            OnClickPosition?.Invoke(mousePosition);

        }

    }
}
