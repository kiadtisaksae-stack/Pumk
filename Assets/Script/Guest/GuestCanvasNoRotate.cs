using UnityEngine;

[DisallowMultipleComponent]
public class GuestCanvasNoRotate : MonoBehaviour
{
    [Header("Rotation Lock")]
    [Tooltip("If true, force canvas world X rotation to 0.")]
    [SerializeField] private bool lockWorldX = true;

    [Tooltip("If true, force canvas world Y rotation to 0 (prevents flip when guest turns).")]
    [SerializeField] private bool lockWorldY = true;

    private void LateUpdate()
    {
        Vector3 euler = transform.rotation.eulerAngles;

        if (lockWorldX) euler.x = 0f;
        if (lockWorldY) euler.y = 0f;

        transform.rotation = Quaternion.Euler(euler);
    }
}
