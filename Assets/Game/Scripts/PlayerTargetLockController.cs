using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerTargetLockController :
    MonoBehaviour
{
    [Header("Camara")]

    [SerializeField]
    private Camera aimCamera;


    [Header("Seleccion de objetivo")]

    [Tooltip(
        "Layers que contienen las naves enemigas."
    )]
    [SerializeField]
    private LayerMask enemyTargetMask;


    [Tooltip(
        "Distancia maxima desde la camara " +
        "para seleccionar un objetivo.")]
    [SerializeField, Min(1f)]
    private float lockRayDistance = 5000f;


    private TurretAimController[] turrets;

    private Collider currentTarget;


    private void Awake()
    {
        if (aimCamera == null)
        {
            aimCamera =
                Camera.main;
        }


        RefreshTurrets();
    }


    private void Update()
    {
        if (aimCamera == null)
        {
            aimCamera =
                Camera.main;
        }


        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current
            .cKey
            .wasPressedThisFrame)
        {
            TryLockTarget();
        }

        if (Keyboard.current
            .zKey
            .wasPressedThisFrame)
        {
            ClearTarget();
        }

        if (currentTarget == null)
        {
            ClearTarget();
        }
    }


    private void RefreshTurrets()
    {
        turrets =
            GetComponentsInChildren
            <TurretAimController>(
                true
            );
    }


    private void TryLockTarget()
    {
        if (aimCamera == null ||
            Mouse.current == null)
        {
            return;
        }


        Vector2 mousePosition =
            Mouse.current
            .position
            .ReadValue();


        Ray selectionRay =
            aimCamera.ScreenPointToRay(
                mousePosition
            );


        if (!Physics.Raycast(
                selectionRay,
                out RaycastHit hit,
                lockRayDistance,
                enemyTargetMask,
                QueryTriggerInteraction.Ignore
            ))
        {
            return;
        }


        currentTarget =
            hit.collider;


        foreach (
            TurretAimController turret
            in turrets
        )
        {
            if (turret == null)
            {
                continue;
            }


            turret.SetLockedTarget(
                currentTarget
            );
        }


        Debug.Log(
            $"Objetivo bloqueado: " +
            $"{currentTarget.name}",
            this
        );
    }


    private void ClearTarget()
    {
        currentTarget = null;


        if (turrets == null)
        {
            return;
        }


        foreach (
            TurretAimController turret
            in turrets
        )
        {
            if (turret == null)
            {
                continue;
            }


            turret.ClearLockedTarget();
        }
    }
}