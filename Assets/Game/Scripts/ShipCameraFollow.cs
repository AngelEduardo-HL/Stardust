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

    private Vector3 positionVelocity;

    private bool wasFreeLooking;


    private void Start()
    {
        orbitPitch =
            startingPitch;
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


        HandleMouseState(
            freeLook
        );


        if (freeLook)
        {
            ReadCameraInput();
        }
        else if (returnToCenter)
        {
            ReturnCameraToCenter();
        }


        UpdateCameraPosition();
    }


    private void HandleMouseState(
        bool freeLook
    )
    {
        if (freeLook &&
            !wasFreeLooking)
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible =
                false;
        }


        if (!freeLook &&
            wasFreeLooking)
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible =
                true;
        }


        wasFreeLooking =
            freeLook;
    }


    private void ReadCameraInput()
    {
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
            cameraDistance;


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

        Cursor.visible =
            true;
    }
}