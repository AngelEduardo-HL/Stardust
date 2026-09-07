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

    [Header("Entrada a batalla")]
    [SerializeField, Min(0.1f)] private float arrivalSpeed = 100f;
    [SerializeField, Min(1f)] private float arrivalBrakeStartDistance = 80f;
    [SerializeField, Min(0.1f)] private float arrivalPointTolerance = 5f;

    [Header("Movimiento normal")]
    [SerializeField, Min(0f)] private float maximumForwardSpeed = 12f;
    [SerializeField, Min(0.01f)] private float timeToMaximumForwardSpeed = 3f;
    [SerializeField, Min(0.01f)] private float forwardBrakeTime = 2f;

    [Header("Maniobrabilidad")]
    [SerializeField, Min(0.01f)] private float yaw180TurnTime = 2.5f;
    [SerializeField, Range(0f, 180f)] private float hardTurnAngle = 60f;
    [SerializeField, Range(0.05f, 1f)] private float hardTurnSpeedMultiplier = 0.55f;

    [Header("Navegacion alrededor de la estacion")]
    [SerializeField, Min(0f)] private float avoidancePadding = 10f;
    [SerializeField, Min(1f)] private float orbitLookAheadDistance = 45f;
    [SerializeField, Min(0.1f)] private float orbitRadialStrength = 2f;

    [Tooltip("Si necesita girar más que este ángulo, hará una maniobra curva en vez de enfilar directamente el destino.")]
    [SerializeField, Range(0f, 180f)] private float orbitWhenTurnExceeds = 65f;

    [Tooltip("Cuando durante la órbita el destino queda a este ángulo o menos, puede abandonar la curva.")]
    [SerializeField, Range(0f, 180f)] private float orbitExitAngle = 30f;

    [Header("Destino")]
    [SerializeField, Min(0f)] private float destinationPadding = 5f;
    [SerializeField, Min(0.1f)] private float approachStopDistance = 8f;

    [Header("Debug")]
    [SerializeField] private EnemyShipState currentState;
    [SerializeField] private float currentForwardSpeed;
    [SerializeField] private Vector3 arrivalPoint;
    [SerializeField] private Vector3 finalApproachPoint;
    [SerializeField] private Vector3 steeringPoint;
    [SerializeField] private bool orbiting;
    [SerializeField] private int orbitDirection;
    [SerializeField] private float orbitRadius;

    private Rigidbody shipRigidbody;
    public bool CanFight => currentState != EnemyShipState.Arrival;

    private Transform spaceStation;
    private float minimumApproachRadius;
    private float maximumApproachRadius;

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

    public void Initialize(Vector3 newArrivalPoint, Transform station, float minRadius, float maxRadius)
    {
        arrivalPoint = newArrivalPoint;
        spaceStation = station;

        minimumApproachRadius = Mathf.Max(0f, minRadius);
        maximumApproachRadius = Mathf.Max(minimumApproachRadius + 1f, maxRadius);

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

    // =========================================================
    // LLEGADA RAPIDA
    // =========================================================

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

            float requiredDeceleration =
                (initialSpeedSquared - targetSpeedSquared) /
                (2f * Mathf.Max(arrivalBrakeStartDistance, 0.01f));

            requiredDeceleration = Mathf.Max(requiredDeceleration, 0f);

            currentForwardSpeed = Mathf.MoveTowards(
                currentForwardSpeed,
                maximumForwardSpeed,
                requiredDeceleration * Time.fixedDeltaTime
            );
        }

        ApplyForwardMovement(shipRigidbody.rotation);

        float forwardDot = Vector3.Dot(toArrival, shipRigidbody.rotation * Vector3.forward);

        if (distance <= arrivalPointTolerance || forwardDot <= 0f)
        {
            currentForwardSpeed = maximumForwardSpeed;

            SelectFinalDestination();
            EvaluateInitialRoute();

            currentState = EnemyShipState.Approach;
        }
    }

    // =========================================================
    // NAVEGACION
    // =========================================================

    private void UpdateApproach()
    {
        Vector3 toFinal = finalApproachPoint - shipRigidbody.position;
        toFinal.y = 0f;

        float distanceToFinal = toFinal.magnitude;

        if (distanceToFinal <= approachStopDistance)
        {
            orbiting = false;
            currentState = EnemyShipState.Holding;
            return;
        }

        float safeRadius = minimumApproachRadius + avoidancePadding;
        bool directPathBlocked = SegmentIntersectsCircle(
            shipRigidbody.position,
            finalApproachPoint,
            spaceStation.position,
            safeRadius
        );

        Vector3 forward = shipRigidbody.rotation * Vector3.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 directionToFinal = toFinal.normalized;
        float angleToFinal = Vector3.Angle(forward, directionToFinal);

        if (!orbiting && (directPathBlocked || angleToFinal > orbitWhenTurnExceeds))
            BeginOrbit();

        if (orbiting)
        {
            bool pathIsClear = !directPathBlocked;

            if (pathIsClear && angleToFinal <= orbitExitAngle)
                orbiting = false;
        }

        steeringPoint = orbiting ? GetOrbitSteeringPoint() : finalApproachPoint;

        SteerTowards(steeringPoint);
    }

    // =========================================================
    // ORBITA / RUTA CURVA
    // =========================================================

    private void BeginOrbit()
    {
        orbiting = true;

        Vector3 radial = shipRigidbody.position - spaceStation.position;
        radial.y = 0f;

        if (radial.sqrMagnitude < 0.001f)
            radial = Vector3.right;

        radial.Normalize();

        Vector3 tangentA = new Vector3(-radial.z, 0f, radial.x);
        Vector3 tangentB = -tangentA;

        Vector3 forward = shipRigidbody.rotation * Vector3.forward;
        forward.y = 0f;
        forward.Normalize();

        float alignmentA = Vector3.Dot(forward, tangentA);
        float alignmentB = Vector3.Dot(forward, tangentB);

        orbitDirection = alignmentA >= alignmentB ? 1 : -1;

        float safeMinimum = minimumApproachRadius + avoidancePadding;
        float safeMaximum = Mathf.Max(safeMinimum, maximumApproachRadius);

        orbitRadius = Random.Range(safeMinimum, safeMaximum);
    }

    private Vector3 GetOrbitSteeringPoint()
    {
        Vector3 position = shipRigidbody.position;

        Vector3 radial = position - spaceStation.position;
        radial.y = 0f;

        float currentRadius = radial.magnitude;

        if (currentRadius < 0.001f)
            radial = Vector3.right;
        else
            radial.Normalize();

        Vector3 tangent;

        if (orbitDirection > 0)
            tangent = new Vector3(-radial.z, 0f, radial.x);
        else
            tangent = new Vector3(radial.z, 0f, -radial.x);

        float radiusError = orbitRadius - currentRadius;
        float radialCorrection = radiusError / Mathf.Max(orbitRadius, 1f);

        radialCorrection = Mathf.Clamp(radialCorrection, -0.8f, 1.5f);

        Vector3 desiredDirection = tangent + radial * radialCorrection * orbitRadialStrength;

        float safeRadius = minimumApproachRadius + avoidancePadding;

        if (currentRadius < safeRadius)
            desiredDirection += radial * 3f;

        desiredDirection.Normalize();

        return position + desiredDirection * orbitLookAheadDistance;
    }

    // =========================================================
    // GIRO Y MOVIMIENTO
    // =========================================================

    private void SteerTowards(Vector3 target)
    {
        Vector3 direction = target - shipRigidbody.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        direction.Normalize();

        Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);

        float yawSpeed = 180f / Mathf.Max(yaw180TurnTime, 0.01f);

        Quaternion nextRotation = Quaternion.RotateTowards(
            shipRigidbody.rotation,
            desiredRotation,
            yawSpeed * Time.fixedDeltaTime
        );

        shipRigidbody.MoveRotation(nextRotation);

        Vector3 currentForward = nextRotation * Vector3.forward;
        currentForward.y = 0f;
        currentForward.Normalize();

        float angle = Vector3.Angle(currentForward, direction);

        float targetSpeed = maximumForwardSpeed;

        if (angle > hardTurnAngle)
            targetSpeed *= hardTurnSpeedMultiplier;

        UpdateNormalSpeed(targetSpeed);
        ApplyForwardMovement(nextRotation);
    }

    private void UpdateNormalSpeed(float targetSpeed)
    {
        float changeRate;

        if (targetSpeed > currentForwardSpeed)
            changeRate = maximumForwardSpeed / Mathf.Max(timeToMaximumForwardSpeed, 0.01f);
        else
            changeRate = maximumForwardSpeed / Mathf.Max(forwardBrakeTime, 0.01f);

        currentForwardSpeed = Mathf.MoveTowards(
            currentForwardSpeed,
            targetSpeed,
            changeRate * Time.fixedDeltaTime
        );
    }

    private void ApplyForwardMovement(Quaternion rotation)
    {
        Vector3 forward = rotation * Vector3.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.001f)
            forward.Normalize();

        shipRigidbody.linearVelocity = forward * currentForwardSpeed;
    }

    // =========================================================
    // DESTINO FINAL
    // =========================================================

    private void SelectFinalDestination()
    {
        float minimumDestinationRadius = minimumApproachRadius + destinationPadding;
        minimumDestinationRadius = Mathf.Min(minimumDestinationRadius, maximumApproachRadius);

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(minimumDestinationRadius, maximumApproachRadius);

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * radius,
            0f,
            Mathf.Sin(angle) * radius
        );

        finalApproachPoint = spaceStation.position + offset;
        finalApproachPoint.y = spaceStation.position.y;
    }

    private void EvaluateInitialRoute()
    {
        Vector3 forward = shipRigidbody.rotation * Vector3.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 toFinal = finalApproachPoint - shipRigidbody.position;
        toFinal.y = 0f;

        if (toFinal.sqrMagnitude < 0.001f) return;

        float angle = Vector3.Angle(forward, toFinal.normalized);
        float safeRadius = minimumApproachRadius + avoidancePadding;

        bool blocked = SegmentIntersectsCircle(
            shipRigidbody.position,
            finalApproachPoint,
            spaceStation.position,
            safeRadius
        );

        if (blocked || angle > orbitWhenTurnExceeds)
            BeginOrbit();
    }

    // =========================================================
    // DETECCION DE LA ZONA PROHIBIDA
    // =========================================================

    private bool SegmentIntersectsCircle(Vector3 start, Vector3 end, Vector3 center, float radius)
    {
        start.y = 0f;
        end.y = 0f;
        center.y = 0f;

        Vector3 segment = end - start;

        if (segment.sqrMagnitude < 0.001f)
            return (start - center).sqrMagnitude <= radius * radius;

        float t = Vector3.Dot(center - start, segment) / segment.sqrMagnitude;
        t = Mathf.Clamp01(t);

        Vector3 closestPoint = start + segment * t;

        return (closestPoint - center).sqrMagnitude <= radius * radius;
    }

    // =========================================================
    // DETENERSE
    // =========================================================

    private void UpdateHolding()
    {
        float brakingRate = maximumForwardSpeed / Mathf.Max(forwardBrakeTime, 0.01f);

        currentForwardSpeed = Mathf.MoveTowards(
            currentForwardSpeed,
            0f,
            brakingRate * Time.fixedDeltaTime
        );

        ApplyForwardMovement(shipRigidbody.rotation);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(arrivalPoint, 2f);
        Gizmos.DrawSphere(finalApproachPoint, 3f);

        if (Application.isPlaying)
        {
            Gizmos.DrawSphere(steeringPoint, 1.5f);
            Gizmos.DrawLine(transform.position, steeringPoint);
        }
    }
}