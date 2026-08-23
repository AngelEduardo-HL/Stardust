using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ShipCameraFollow : MonoBehaviour
{
    [Header("Objetivo")]

    [SerializeField]
    private Transform cameraPivot;


    [Header("Distancia")]

    [SerializeField, Min(0.1f)]
    private float cameraDistance = 15f;

    [SerializeField, Min(0.1f)]
    private float minimumDistance = 5f;

    [SerializeField, Min(0.1f)]
    private float maximumDistance = 30f;

    [SerializeField, Min(0f)]
    private float zoomSensitivity = 0.015f;

    [SerializeField, Min(0.01f)]
    private float zoomSmoothTime = 0.08f;

    [SerializeField, Min(0.01f)]
    private float positionSmoothTime = 0.05f;


    [Header("Orbita")]

    [SerializeField, Min(0f)]
    private float horizontalSensitivity = 0.15f;

    [SerializeField, Min(0f)]
    private float verticalSensitivity = 0.15f;

    [SerializeField]
    private float startingPitch = 10f;

    [SerializeField]
    private float minimumPitch = -35f;

    [SerializeField]
    private float maximumPitch = 65f;


    [Header("Al soltar click derecho")]

    [SerializeField]
    private bool returnToCenter = false;

    [SerializeField, Min(0f)]
    private float returnSpeed = 90f;


    [Header("Suavizado de rotacion")]

    [SerializeField, Min(0f)]
    private float rotationSmoothSpeed = 15f;


    private float orbitYaw;
    private float orbitPitch;

    private float currentDistance;
    private float targetDistance;

    private float zoomVelocity;

    private Vector3 positionVelocity;

    private bool wasFreeLooking;


    private void Start()
    {
        orbitPitch = startingPitch;

        cameraDistance =
            Mathf.Clamp(
                cameraDistance,
                minimumDistance,
                maximumDistance
            );

        currentDistance = cameraDistance;
        targetDistance = cameraDistance;
    }

    public void SetCameraPivot(Transform newCameraPivot)
    {
        cameraPivot =
        newCameraPivot;


        // Reinicia la vista para que al
        // cambiar de nave
        orbitYaw = 0f;

        orbitPitch =
            startingPitch;


        currentDistance =
            Mathf.Clamp(
                cameraDistance,
                minimumDistance,
                maximumDistance
            );


        targetDistance =
            currentDistance;


        positionVelocity =
            Vector3.zero;

        zoomVelocity = 0f; ;
    }


    private void LateUpdate()
    {
        if (cameraPivot == null)
        {
            return;
        }

        bool freeLook =
            Mouse.current != null &&
            Mouse.current.rightButton.isPressed;


        HandleMouseState(freeLook);

        HandleZoom();


        if (freeLook)
        {
            ReadCameraInput();
        }
        else if (returnToCenter)
        {
            ReturnCameraToCenter();
        }


        UpdateZoom();

        UpdateCameraPosition();
    }


    private void HandleMouseState(
    bool freeLook
)
    {

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible =
            true;


        wasFreeLooking =
            freeLook;
    }


    private void ReadCameraInput()
    {
        if (Mouse.current == null)
        {
            return;
        }


        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();


        orbitYaw +=
            mouseDelta.x *
            horizontalSensitivity;


        orbitPitch -=
            mouseDelta.y *
            verticalSensitivity;


        orbitPitch =
            Mathf.Clamp(
                orbitPitch,
                minimumPitch,
                maximumPitch
            );
    }


    private void HandleZoom()
    {
        if (Mouse.current == null)
        {
            return;
        }


        float scroll =
            Mouse.current.scroll.ReadValue().y;


        if (Mathf.Abs(scroll) < 0.01f)
        {
            return;
        }


        targetDistance -=
            scroll *
            zoomSensitivity;


        targetDistance =
            Mathf.Clamp(
                targetDistance,
                minimumDistance,
                maximumDistance
            );
    }


    private void UpdateZoom()
    {
        currentDistance =
            Mathf.SmoothDamp(
                currentDistance,
                targetDistance,
                ref zoomVelocity,
                zoomSmoothTime
            );
    }


    private void ReturnCameraToCenter()
    {
        orbitYaw =
            Mathf.MoveTowardsAngle(
                orbitYaw,
                0f,
                returnSpeed *
                Time.deltaTime
            );


        orbitPitch =
            Mathf.MoveTowards(
                orbitPitch,
                startingPitch,
                returnSpeed *
                Time.deltaTime
            );
    }


    private void UpdateCameraPosition()
    {
        Quaternion orbitRotation =
            cameraPivot.rotation
            *
            Quaternion.Euler(
                orbitPitch,
                orbitYaw,
                0f
            );


        Vector3 desiredPosition =
            cameraPivot.position
            -
            orbitRotation *
            Vector3.forward *
            currentDistance;


        transform.position =
            Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref positionVelocity,
                positionSmoothTime
            );


        Vector3 lookDirection =
            cameraPivot.position -
            transform.position;


        if (lookDirection.sqrMagnitude
            <= 0.0001f)
        {
            return;
        }


        Quaternion desiredRotation =
            Quaternion.LookRotation(
                lookDirection,
                orbitRotation *
                Vector3.up
            );


        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotationSmoothSpeed *
                Time.deltaTime
            );
    }


    private void OnDisable()
    {
        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;
    }
}