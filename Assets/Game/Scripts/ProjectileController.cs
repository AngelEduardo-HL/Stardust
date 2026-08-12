using UnityEngine;

public sealed class ProjectileController : MonoBehaviour
{
    [Header("Configuracion base")]

    [Tooltip("Radio base usado para detectar impactos.")]
    [SerializeField, Min(0.01f)]
    private float baseHitRadius = 0.1f;


    private float damage;
    private float speed;
    private float maxRange;
    private float hitRadius;

    private float travelledDistance;

    private Vector3 direction;
    private Vector3 baseScale;

    private LayerMask hitMask;

    private bool initialized;


    private void Awake()
    {
        baseScale =
            transform.localScale;
    }


    public void Initialize(
        float newDamage,
        float newSpeed,
        float newRange,
        float sizeMultiplier,
        LayerMask newHitMask
    )
    {
        damage =
            newDamage;

        speed =
            newSpeed;

        maxRange =
            newRange;

        hitMask =
            newHitMask;


        direction =
            transform.forward.normalized;


        float safeSize =
            Mathf.Max(
                sizeMultiplier,
                0.01f
            );


        transform.localScale =
            baseScale *
            safeSize;


        hitRadius =
            baseHitRadius *
            safeSize;


        travelledDistance = 0f;

        initialized = true;
    }


    private void Update()
    {
        if (!initialized)
        {
            return;
        }


        MoveProjectile();
    }


    private void MoveProjectile()
    {
        float remainingRange =
            maxRange -
            travelledDistance;


        if (remainingRange <= 0f)
        {
            Destroy(gameObject);

            return;
        }


        float movementDistance =
            speed *
            Time.deltaTime;


        movementDistance =
            Mathf.Min(
                movementDistance,
                remainingRange
            );


        // Comprobamos todo el espacio que
        // recorrerá el proyectil este frame.
        if (Physics.SphereCast(
                transform.position,
                hitRadius,
                direction,
                out RaycastHit hit,
                movementDistance,
                hitMask,
                QueryTriggerInteraction.Ignore
            ))
        {
            transform.position =
                hit.point;


            HitTarget(
                hit.collider
            );


            return;
        }


        transform.position +=
            direction *
            movementDistance;


        travelledDistance +=
            movementDistance;


        if (travelledDistance >=
            maxRange)
        {
            Destroy(gameObject);
        }
    }


    private void HitTarget(
        Collider hitCollider
    )
    {
        HealthController health =
            hitCollider.GetComponentInParent
            <HealthController>();


        if (health != null)
        {
            health.TakeDamage(
                damage
            );
        }


        Destroy(gameObject);
    }
}