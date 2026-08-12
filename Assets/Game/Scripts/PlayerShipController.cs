using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public sealed class PlayerShipController : MonoBehaviour
{
    [Header("Marcha adelante")]

    [Tooltip("Velocidad máxima hacia adelante.")]
    [SerializeField, Min(0f)]
    private float maximumForwardSpeed = 10f;

    [Tooltip("Segundos necesarios para pasar de 0 a velocidad máxima.")]
    [SerializeField, Min(0.01f)]
    private float timeToMaximumForwardSpeed = 10f;

    [Tooltip("Segundos necesarios para frenar desde velocidad máxima hasta 0 usando F.")]
    [SerializeField, Min(0.01f)]
    private float forwardBrakeTime = 5f;


    [Header("Reversa")]

    [Tooltip("Velocidad máxima hacia atrás.")]
    [SerializeField, Min(0f)]
    private float maximumReverseSpeed = 3f;

    [Tooltip("Segundos necesarios para alcanzar la reversa máxima.")]
    [SerializeField, Min(0.01f)]
    private float timeToMaximumReverseSpeed = 2f;

    [Tooltip("Tiempo que tarda en detener la reversa al soltar F.")]
    [SerializeField, Min(0.01f)]
    private float reverseStopTime = 0.75f;


    [Header("Movimiento vertical")]

    [SerializeField, Min(0f)]
    private float maximumVerticalSpeed = 5f;

    [SerializeField, Min(0.01f)]
    private float verticalAccelerationTime = 1f;

    [SerializeField, Min(0.01f)]
    private float verticalStopTime = 0.5f;


    [Header("Tiempo de giro")]

    [Tooltip("Segundos necesarios para realizar un giro horizontal de 180 grados.")]
    [SerializeField, Min(0.01f)]
    private float yaw180TurnTime = 4f;

    [Tooltip("Segundos necesarios para realizar un giro vertical de 180 grados.")]
    [SerializeField, Min(0.01f)]
    private float pitch180TurnTime = 4f;

    [Tooltip("Segundos necesarios para realizar un roll completo de 360 grados.")]
    [SerializeField, Min(0.01f)]
    private float roll360TurnTime = 4f;


    [Header("Estado actual")]

    [SerializeField]
    private float currentForwardSpeed;

    [SerializeField]
    private float currentVerticalSpeed;


    private Rigidbody shipRigidbody;

    private bool forwardPressed;
    private bool reversePressed;

    private float verticalInput;
    private float pitchInput;
    private float yawInput;
    private float rollInput;


    public float CurrentForwardSpeed => currentForwardSpeed;


    private void Awake()
    {
        shipRigidbody = GetComponent<Rigidbody>();

        shipRigidbody.useGravity = false;
        shipRigidbody.isKinematic = false;

        shipRigidbody.interpolation =
            RigidbodyInterpolation.Interpolate;

        shipRigidbody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        // Nosotros controlaremos la velocidad.
        shipRigidbody.linearDamping = 0f;

        // La rotación también será controlada por el script.
        shipRigidbody.angularDamping = 0f;

        float maximumMovementSpeed =
            Mathf.Max(
                maximumForwardSpeed,
                maximumReverseSpeed
            ) + maximumVerticalSpeed;

        shipRigidbody.maxLinearVelocity =
            maximumMovementSpeed;
    }


    private void Update()
    {
        ReadKeyboardInput();
    }


    private void FixedUpdate()
    {
        UpdateForwardSpeed();
        UpdateVerticalSpeed();

        Quaternion nextRotation =
            CalculateNextRotation();

        ApplyRotation(nextRotation);
        ApplyMovement(nextRotation);
    }


    private void ReadKeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            ResetInput();
            return;
        }

        // R = acelerar hacia adelante.
        forwardPressed =
            keyboard.rKey.isPressed;

        // F = frenar / reversa.
        reversePressed =
            keyboard.fKey.isPressed;


        // Flechas = subir / bajar.
        verticalInput = ReadAxis(
            keyboard.upArrowKey.isPressed,
            keyboard.downArrowKey.isPressed
        );


        // A / D = giro horizontal.
        yawInput = ReadAxis(
            keyboard.dKey.isPressed,
            keyboard.aKey.isPressed
        );


        // W / S = inclinación vertical.
        pitchInput = ReadAxis(
            keyboard.wKey.isPressed,
            keyboard.sKey.isPressed
        );


        // Q / E = roll.
        rollInput = ReadAxis(
            keyboard.qKey.isPressed,
            keyboard.eKey.isPressed
        );
    }


    private void UpdateForwardSpeed()
    {
        float deltaTime =
            Time.fixedDeltaTime;


        // ---------------------------
        // MARCHA ADELANTE
        // ---------------------------

        if (forwardPressed && !reversePressed)
        {
            float acceleration =
                maximumForwardSpeed /
                Mathf.Max(
                    timeToMaximumForwardSpeed,
                    0.01f
                );

            currentForwardSpeed =
                Mathf.MoveTowards(
                    currentForwardSpeed,
                    maximumForwardSpeed,
                    acceleration * deltaTime
                );

            return;
        }


        // ---------------------------
        // FRENO Y REVERSA
        // ---------------------------

        if (reversePressed && !forwardPressed)
        {
            // Todavía estamos avanzando.
            // Primero frenamos hasta llegar a 0.
            if (currentForwardSpeed > 0f)
            {
                float brakingSpeed =
                    maximumForwardSpeed /
                    Mathf.Max(
                        forwardBrakeTime,
                        0.01f
                    );

                currentForwardSpeed =
                    Mathf.MoveTowards(
                        currentForwardSpeed,
                        0f,
                        brakingSpeed * deltaTime
                    );
            }

            // Ya estamos detenidos o retrocediendo.
            else
            {
                float reverseAcceleration =
                    maximumReverseSpeed /
                    Mathf.Max(
                        timeToMaximumReverseSpeed,
                        0.01f
                    );

                currentForwardSpeed =
                    Mathf.MoveTowards(
                        currentForwardSpeed,
                        -maximumReverseSpeed,
                        reverseAcceleration * deltaTime
                    );
            }

            return;
        }


        // ---------------------------
        // SIN TECLAS
        // ---------------------------

        // Si vamos hacia adelante:
        // conservamos la velocidad.
        if (currentForwardSpeed >= 0f)
        {
            return;
        }


        // Si estamos en reversa:
        // al soltar F regresamos automáticamente a 0.
        float reverseStoppingSpeed =
            maximumReverseSpeed /
            Mathf.Max(
                reverseStopTime,
                0.01f
            );

        currentForwardSpeed =
            Mathf.MoveTowards(
                currentForwardSpeed,
                0f,
                reverseStoppingSpeed * deltaTime
            );
    }


    private void UpdateVerticalSpeed()
    {
        float targetVerticalSpeed =
            verticalInput * maximumVerticalSpeed;

        float movementTime;

        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            movementTime =
                verticalAccelerationTime;
        }
        else
        {
            movementTime =
                verticalStopTime;
        }

        float verticalAcceleration =
            maximumVerticalSpeed /
            Mathf.Max(
                movementTime,
                0.01f
            );

        currentVerticalSpeed =
            Mathf.MoveTowards(
                currentVerticalSpeed,
                targetVerticalSpeed,
                verticalAcceleration *
                Time.fixedDeltaTime
            );
    }


    private Quaternion CalculateNextRotation()
    {
        float yawSpeed =
            180f /
            Mathf.Max(
                yaw180TurnTime,
                0.01f
            );

        float pitchSpeed =
            180f /
            Mathf.Max(
                pitch180TurnTime,
                0.01f
            );

        float rollSpeed =
            360f /
            Mathf.Max(
                roll360TurnTime,
                0.01f
            );


        Vector3 rotationThisFrame =
            new Vector3(
                pitchInput *
                pitchSpeed *
                Time.fixedDeltaTime,

                yawInput *
                yawSpeed *
                Time.fixedDeltaTime,

                rollInput *
                rollSpeed *
                Time.fixedDeltaTime
            );


        Quaternion deltaRotation =
            Quaternion.Euler(
                rotationThisFrame
            );


        return
            shipRigidbody.rotation *
            deltaRotation;
    }


    private void ApplyRotation(
        Quaternion nextRotation
    )
    {
        shipRigidbody.MoveRotation(
            nextRotation
        );
    }


    private void ApplyMovement(
        Quaternion nextRotation
    )
    {
        // Dirección de la nave después del giro actual.
        Vector3 forwardDirection =
            nextRotation *
            Vector3.forward;

        Vector3 verticalDirection =
            nextRotation *
            Vector3.up;


        // La velocidad queda alineada siempre
        // con la orientación actual de la nave.
        Vector3 desiredVelocity =
            forwardDirection *
            currentForwardSpeed
            +
            verticalDirection *
            currentVerticalSpeed;


        shipRigidbody.linearVelocity =
            desiredVelocity;
    }


    private static float ReadAxis(
        bool positive,
        bool negative
    )
    {
        float positiveValue =
            positive ? 1f : 0f;

        float negativeValue =
            negative ? 1f : 0f;

        return
            positiveValue -
            negativeValue;
    }


    private void ResetInput()
    {
        forwardPressed = false;
        reversePressed = false;

        verticalInput = 0f;
        pitchInput = 0f;
        yawInput = 0f;
        rollInput = 0f;
    }
}