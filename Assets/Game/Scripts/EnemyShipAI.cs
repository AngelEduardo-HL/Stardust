using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public sealed class EnemyShipAI : MonoBehaviour
{
    private enum EnemyShipState
    {
        Arrival,
        Approach,
        Holding
    }

    [Header("Hiper espacio")]
    [SerializeField, Min(0.1f)] private float arrivalSpeed = 40f;
    [SerializeField, Min(1f)] private float arrivalBrakeStartDistance = 70f;
    [SerializeField, Min(0.1f)] private float arrivalPointTolerance = 5f;

    [Header("Movimiento")]
    [SerializeField, Min(0f)] private float maximumForwardSpeed = 10f;
    [SerializeField, Min(0.01f)] private float timeToMaximumForwardSpeed = 5f;
    [SerializeField, Min(0.01f)] private float forwardBrakeTime = 4f;

    [Header("Maniobrabilidad")]
    [SerializeField, Min(0.01f)] private float yaw180TurnTime = 5f;
    [SerializeField, Range(0f, 180f)] private float hardTurnAngle = 60f;
    [SerializeField, Range(0.05f, 1f)] private float hardTurnSpeedMultiplier = 0.45f;

    [Header("Aproximacion")]
    [SerializeField, Min(0.1f)] private float approachStopDistance = 8f;

    [Header("Debug")]
    [SerializeField] private EnemyShipState currentState;
    [SerializeField] private float currentForwardSpeed;
    [SerializeField] private Vector3 arrivalPoint;
    [SerializeField] private Vector3 approachPoint;

    private Rigidbody shipRigidbody;
    private bool initialized;

    public float CurrentForwardSpeed => currentForwardSpeed;

    private void Awake()
    {
        shipRigidbody = GetComponent<Rigidbody>();

        shipRigidbody.useGravity = false;
        shipRigidbody.isKinematic = false;
        shipRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        shipRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        shipRigidbody.linearDamping = 0f;
        shipRigidbody.angularDamping = 0f;
        shipRigidbody.maxLinearVelocity = Mathf.Max(arrivalSpeed, maximumForwardSpeed);
    }

    public void Initialize(Vector3 newArrivalPoint, Vector3 newApproachPoint)
    {
        arrivalPoint = newArrivalPoint;
        approachPoint = newApproachPoint;

        currentForwardSpeed = arrivalSpeed;
        currentState = EnemyShipState.Arrival;
        initialized = true;
    }

    private void FixedUpdate()
    {
        if (!initialized) return;

        switch (currentState)
        {
            case EnemyShipState.Arrival:
                UpdateArrival();
                break;

            case EnemyShipState.Approach:
                UpdateApproach();
                break;

            case EnemyShipState.Holding:
                UpdateHolding();
                break;
        }
    }

    private void UpdateArrival()
    {
        Vector3 toArrival = arrivalPoint - shipRigidbody.position;
        toArrival.y = 0f;

        float distance = toArrival.magnitude;

        if (distance > arrivalBrakeStartDistance)
        {
            currentForwardSpeed = arrivalSpeed;
        }
        else
        {
            float initialSpeedSquared = arrivalSpeed * arrivalSpeed;
            float targetSpeedSquared = maximumForwardSpeed * maximumForwardSpeed;

            float requiredDeceleration = (initialSpeedSquared - targetSpeedSquared) / (2f * Mathf.Max(arrivalBrakeStartDistance, 0.01f));
            requiredDeceleration = Mathf.Max(requiredDeceleration, 0f);

            currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed, maximumForwardSpeed,requiredDeceleration * Time.fixedDeltaTime);
        }

        ApplyForwardMovement(shipRigidbody.rotation);
        float forwardDot = Vector3.Dot(toArrival, transform.forward);

        if (distance <= arrivalPointTolerance || forwardDot <= 0f)
        {
            currentForwardSpeed = maximumForwardSpeed;
            currentState = EnemyShipState.Approach;
        }
    }

    private void UpdateApproach()
    {
        Vector3 toTarget = approachPoint - shipRigidbody.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;

        if (distance <= approachStopDistance)
        {
            currentState = EnemyShipState.Holding;
            return;
        }

        if (toTarget.sqrMagnitude < 0.001f) return;

        Vector3 desiredDirection = toTarget.normalized;
        Quaternion desiredRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);

        float yawSpeed = 180f / Mathf.Max(yaw180TurnTime, 0.01f);

        Quaternion nextRotation = Quaternion.RotateTowards(
            shipRigidbody.rotation,
            desiredRotation,
            yawSpeed * Time.fixedDeltaTime
        );

        shipRigidbody.MoveRotation(nextRotation);

        Vector3 currentForward = shipRigidbody.rotation * Vector3.forward;
        currentForward.y = 0f;

        float angleToTarget = Vector3.Angle(currentForward, desiredDirection);

        float targetSpeed = maximumForwardSpeed;

        if (angleToTarget > hardTurnAngle)
            targetSpeed = maximumForwardSpeed * hardTurnSpeedMultiplier;

        UpdateNormalSpeed(targetSpeed);
        ApplyForwardMovement(nextRotation);
    }

    private void UpdateHolding()
    {
        float brakingRate = maximumForwardSpeed / Mathf.Max(forwardBrakeTime, 0.01f);

        currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed,0f,brakingRate * Time.fixedDeltaTime);

        ApplyForwardMovement(shipRigidbody.rotation);
    }

    private void UpdateNormalSpeed(float targetSpeed)
    {
        float changeRate;

        if (targetSpeed > currentForwardSpeed)
            changeRate = maximumForwardSpeed / Mathf.Max(timeToMaximumForwardSpeed, 0.01f);
        else
            changeRate = maximumForwardSpeed / Mathf.Max(forwardBrakeTime, 0.01f);

        currentForwardSpeed = Mathf.MoveTowards(currentForwardSpeed,targetSpeed,changeRate * Time.fixedDeltaTime);
    }

    private void ApplyForwardMovement(Quaternion rotation)
    {
        Vector3 forwardDirection = rotation * Vector3.forward;
        forwardDirection.y = 0f;

        if (forwardDirection.sqrMagnitude > 0.001f)
            forwardDirection.Normalize();

        shipRigidbody.linearVelocity = forwardDirection * currentForwardSpeed;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(arrivalPoint, 2f);
        Gizmos.DrawSphere(approachPoint, 3f);
        Gizmos.DrawLine(arrivalPoint, approachPoint);
    }
}