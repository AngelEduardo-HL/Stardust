using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public sealed class PlayerShipController : MonoBehaviour
{
    [Header("Movimiento lineal")]

    [SerializeField, Min(0f)]
    private float forwardAcceleration = 20f;

    [SerializeField, Min(0f)]
    private float reverseAcceleration = 12f;

    [SerializeField, Min(0f)]
    private float verticalAcceleration = 10f;

    [SerializeField, Min(0f)]
    private float maximumSpeed = 50f;

    [Header("Movimiento angular")]

    [SerializeField, Min(0f)]
    private float pitchAcceleration = 4f;

    [SerializeField, Min(0f)]
    private float yawAcceleration = 3f;

    [SerializeField, Min(0f)]
    private float rollAcceleration = 5f;

    [SerializeField, Min(0f)]
    private float maximumAngularSpeed = 1.5f;

    [Header("Inercia y estabilización")]

    [SerializeField, Min(0f)]
    private float linearDamping = 0.15f;

    [SerializeField, Min(0f)]
    private float angularDamping = 2.5f;

    private Rigidbody shipRigidbody;

    private float thrustInput;
    private float verticalInput;
    private float pitchInput;
    private float yawInput;
    private float rollInput;

    private void Awake()
    {
        shipRigidbody = GetComponent<Rigidbody>();

        shipRigidbody.useGravity = false;
        shipRigidbody.isKinematic = false;
        shipRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        shipRigidbody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        shipRigidbody.linearDamping = linearDamping;
        shipRigidbody.angularDamping = angularDamping;

        shipRigidbody.maxLinearVelocity = maximumSpeed;
        shipRigidbody.maxAngularVelocity = maximumAngularSpeed;
    }

    private void Update()
    {
        ReadKeyboardInput();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyRotation();
    }

    private void ReadKeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            ResetInput();
            return;
        }

        // W avanza y S frena o retrocede.
        thrustInput = ReadAxis(
            keyboard.rKey.isPressed,
            keyboard.fKey.isPressed
        );

        // Espacio sube y Ctrl izquierdo baja.
        verticalInput = ReadAxis(
            keyboard.upArrowKey.isPressed,
            keyboard.downArrowKey.isPressed
        );

        // D gira a la derecha y A a la izquierda.
        yawInput = ReadAxis(
            keyboard.dKey.isPressed,
            keyboard.aKey.isPressed
        );

        // La flecha arriba levanta la nariz.
        pitchInput = ReadAxis(
            keyboard.wKey.isPressed,
            keyboard.sKey.isPressed
        );

        // Q inclina a la izquierda y E a la derecha.
        rollInput = ReadAxis(
            keyboard.qKey.isPressed,
            keyboard.eKey.isPressed
        );
    }

    private void ApplyMovement()
    {
        float selectedAcceleration =
            thrustInput >= 0f
                ? forwardAcceleration
                : reverseAcceleration;

        Vector3 localAcceleration = new Vector3(
            0f,
            verticalInput * verticalAcceleration,
            thrustInput * selectedAcceleration
        );

        shipRigidbody.AddRelativeForce(
            localAcceleration,
            ForceMode.Acceleration
        );
    }

    private void ApplyRotation()
    {
        Vector3 localAngularAcceleration = new Vector3(
            pitchInput * pitchAcceleration,
            yawInput * yawAcceleration,
            rollInput * rollAcceleration
        );

        shipRigidbody.AddRelativeTorque(
            localAngularAcceleration,
            ForceMode.Acceleration
        );
    }

    private static float ReadAxis(bool positive, bool negative)
    {
        float positiveValue = positive ? 1f : 0f;
        float negativeValue = negative ? 1f : 0f;

        return positiveValue - negativeValue;
    }

    private void ResetInput()
    {
        thrustInput = 0f;
        verticalInput = 0f;
        pitchInput = 0f;
        yawInput = 0f;
        rollInput = 0f;
    }
}