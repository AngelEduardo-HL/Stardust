using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public sealed class CameraFOVZoom : MonoBehaviour
{
    [Header("FOV Zoom")]
    [SerializeField, Range(10f, 120f)] private float zoomedFOV = 35f;
    [SerializeField, Min(0.1f)] private float transitionSpeed = 8f;

    private Camera targetCamera;
    private float normalFOV;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        normalFOV = targetCamera.fieldOfView;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        bool zooming = Keyboard.current.leftShiftKey.isPressed ||
                       Keyboard.current.rightShiftKey.isPressed;

        float targetFOV = zooming ? zoomedFOV : normalFOV;

        targetCamera.fieldOfView = Mathf.Lerp(
            targetCamera.fieldOfView,
            targetFOV,
            transitionSpeed * Time.deltaTime
        );
    }
}