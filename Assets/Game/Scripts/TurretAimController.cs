using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TurretAimController : MonoBehaviour
{
    [Header("Referencias")]

    [SerializeField]
    private Transform bodyPivot;

    [SerializeField]
    private Transform canonsPivot;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private Transform rangeReticle;

    [SerializeField]
    private Camera aimCamera;


    [Header("Rotacion")]

    [Tooltip("Velocidad horizontal de la torreta en grados por segundo.")]
    [SerializeField, Min(0f)]
    private float bodyTurnSpeed = 35f;

    [Tooltip("Velocidad vertical de los cañones en grados por segundo.")]
    [SerializeField, Min(0f)]
    private float canonsTurnSpeed = 25f;

    [Tooltip("Angulo maximo hacia arriba y abajo.")]
    [SerializeField, Range(0f, 89f)]
    private float maxPitchAngle = 45f;

    [Header("Alcance")]

    [SerializeField, Min(1f)]
    private float weaponRange = 300f;
    public float WeaponRange => weaponRange;

    [SerializeField]
    private bool showDebugRay = true;


    [Header("Reticula")]

    [Tooltip("Controla el tamaño visual de la reticula.")]
    [SerializeField, Min(0.0001f)]
    private float reticleScalePerDistance = 0.1f;

    [SerializeField, Min(0.01f)]
    private float minimumReticleScale = 0.1f;

    
    private Quaternion bodyStartRotation;
    private Quaternion canonsStartRotation;

    private Vector3 neutralForwardRoot;
    private Vector3 neutralForwardBody;
    private Vector3 pitchAxisBody;

    private float currentYaw;
    private float currentPitch;


    private void Awake()
    {
        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }

        if (bodyPivot == null ||
            canonsPivot == null ||
            firePoint == null)
        {
            Debug.LogError(
                $"{name}: Faltan referencias en TurretAimController.",
                this
            );

            enabled = false;
            return;
        }

        bodyStartRotation =
            bodyPivot.localRotation;

        canonsStartRotation =
            canonsPivot.localRotation;


        Transform turretRoot =
            bodyPivot.parent;


        // Direccion neutral de disparo
        // respecto a la torreta completa.
        neutralForwardRoot =
            turretRoot.InverseTransformDirection(
                firePoint.forward
            );

        neutralForwardBody =
            bodyPivot.InverseTransformDirection(
                firePoint.forward
            );

        // Eje rojo X del CanonsPivot.
        pitchAxisBody =
            bodyPivot.InverseTransformDirection(
                canonsPivot.right
            );
    }


    private void Update()
    {
        if (aimCamera == null)
        {
            aimCamera = Camera.main;

            if (aimCamera == null)
            {
                return;
            }
        }

        Vector3 aimPoint =
            GetCursorAimPoint();

        RotateBody(aimPoint);

        RotateCanons(aimPoint);

        UpdateRangeReticle();
    }

    public void SetAimCamera(
    Camera newCamera
)
    {
        aimCamera =
            newCamera;
    }

    private Vector3 GetCursorAimPoint()
    {
        Vector2 screenPosition;


        if (Mouse.current != null)
        {
            screenPosition =
                Mouse.current.position
                    .ReadValue();
        }
        else
        {
            screenPosition =
                new Vector2(
                    Screen.width * 0.5f,
                    Screen.height * 0.5f
                );
        }


        Ray cameraRay =
            aimCamera.ScreenPointToRay(
                screenPosition
            );


        return GetPointAtTurretRange(
            cameraRay
        );
    }


    private Vector3 GetPointAtTurretRange(
        Ray cameraRay
    )
    {
        Vector3 center =
            firePoint.position;

        Vector3 originToCenter =
            cameraRay.origin - center;


        float b =
            Vector3.Dot(
                originToCenter,
                cameraRay.direction
            );


        float c =
            originToCenter.sqrMagnitude -
            weaponRange * weaponRange;


        float discriminant =
            b * b - c;


        if (discriminant >= 0f)
        {
            float distance =
                -b +
                Mathf.Sqrt(discriminant);


            if (distance >= 0f)
            {
                return cameraRay.GetPoint(
                    distance
                );
            }
        }


        return cameraRay.GetPoint(
            weaponRange
        );
    }


    private void RotateBody(
        Vector3 aimPoint
    )
    {
        Transform turretRoot =
            bodyPivot.parent;


        Vector3 directionWorld =
            aimPoint -
            bodyPivot.position;


        if (directionWorld.sqrMagnitude
            <= 0.0001f)
        {
            return;
        }


        Vector3 directionLocal =
            turretRoot.InverseTransformDirection(
                directionWorld.normalized
            );


        Vector3 neutral =
            neutralForwardRoot;

        directionLocal.y = 0f;
        neutral.y = 0f;


        if (directionLocal.sqrMagnitude
            <= 0.0001f ||
            neutral.sqrMagnitude
            <= 0.0001f)
        {
            return;
        }


        float desiredYaw =
            Vector3.SignedAngle(
                neutral.normalized,
                directionLocal.normalized,
                Vector3.up
            );


        currentYaw =
            Mathf.MoveTowardsAngle(
                currentYaw,
                desiredYaw,
                bodyTurnSpeed *
                Time.deltaTime
            );


        bodyPivot.localRotation =
            Quaternion.AngleAxis(
                currentYaw,
                Vector3.up
            )
            *
            bodyStartRotation;
    }


    private void RotateCanons(
        Vector3 aimPoint
    )
    {
        Vector3 direction =
            aimPoint -
            canonsPivot.position;


        if (direction.sqrMagnitude
            <= 0.0001f)
        {
            return;
        }


        Vector3 pitchAxisWorld =
            bodyPivot.TransformDirection(
                pitchAxisBody
            );


        Vector3 neutralForwardWorld =
            bodyPivot.TransformDirection(
                neutralForwardBody
            );


        Vector3 neutralProjected =
            Vector3.ProjectOnPlane(
                neutralForwardWorld,
                pitchAxisWorld
            );


        Vector3 targetProjected =
            Vector3.ProjectOnPlane(
                direction.normalized,
                pitchAxisWorld
            );


        if (neutralProjected.sqrMagnitude
            <= 0.0001f ||
            targetProjected.sqrMagnitude
            <= 0.0001f)
        {
            return;
        }


        float desiredPitch =
            Vector3.SignedAngle(
                neutralProjected.normalized,
                targetProjected.normalized,
                pitchAxisWorld
            );


        desiredPitch =
            Mathf.Clamp(
                desiredPitch,
                -maxPitchAngle,
                maxPitchAngle
            );


        currentPitch =
            Mathf.MoveTowards(
                currentPitch,
                desiredPitch,
                canonsTurnSpeed *
                Time.deltaTime
            );


        canonsPivot.localRotation =
            canonsStartRotation
            *
            Quaternion.AngleAxis(
                currentPitch,
                Vector3.right
            );
    }


    private void UpdateRangeReticle()
    {
        // Punto exacto hasta donde realmente
        // está apuntando la torreta.
        Vector3 rayEnd =
            firePoint.position +
            firePoint.forward *
            weaponRange;


        // -------------------------
        // RETICULA
        // -------------------------

        if (rangeReticle != null)
        {
            rangeReticle.position =
                rayEnd;


            // Siempre mira hacia la camara.
            rangeReticle.rotation =
                aimCamera.transform.rotation;

            float distanceToCamera =
                Vector3.Distance(
                    aimCamera.transform.position,
                    rayEnd
                );

            float scale =
                Mathf.Max(
                    minimumReticleScale,
                    distanceToCamera *
                    reticleScalePerDistance
                );


            rangeReticle.localScale =
                Vector3.one * scale;
        }


        // -------------------------
        // RAY DE DEBUG
        // -------------------------

        if (showDebugRay)
        {
            Debug.DrawLine(
                firePoint.position,
                rayEnd,
                Color.cyan
            );
        }
    }
}