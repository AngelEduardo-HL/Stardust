using UnityEngine;
using static TurretWeapon;

public sealed class EnemyCombatController : MonoBehaviour
{
    [Header("Objetivos")]
    [SerializeField] private LayerMask targetMask;
    [SerializeField, Min(1f)] private float searchRadius = 1000f;
    [SerializeField, Min(0.05f)] private float targetRefreshInterval = 0.5f;

    [Header("Comportamiento")]
    [SerializeField] private bool canAttackDuringApproach = true;

    [Header("Debug")]
    [SerializeField] private Collider currentTarget;

    private EnemyShipAI shipAI;
    private TurretAimController[] turretAims;
    private TurretWeapon[] turretWeapons;

    private float nextTargetRefresh;

    private void Awake()
    {
        shipAI = GetComponent<EnemyShipAI>();

        turretAims = GetComponentsInChildren<TurretAimController>(true);
        turretWeapons = GetComponentsInChildren<TurretWeapon>(true);

        foreach (TurretWeapon weapon in turretWeapons)
        {
            if (weapon == null) continue;

            weapon.SetFireControlMode(FireControlMode.AI);
            weapon.SetAIFire(false);
        }
    }

    private void Update()
    {
        if (Time.time >= nextTargetRefresh)
        {
            FindBestTarget();
            nextTargetRefresh = Time.time + targetRefreshInterval;
        }

        if (currentTarget == null)
        {
            ClearTarget();
            return;
        }

        bool canFight = shipAI == null || shipAI.CanFight;

        if (!canFight)
        {
            SetWeaponsFiring(false);
            return;
        }

        SetWeaponsFiring(true);
    }

    private void FindBestTarget()
    {
        Collider[] possibleTargets = Physics.OverlapSphere(
            transform.position,
            searchRadius,
            targetMask,
            QueryTriggerInteraction.Ignore
        );

        Collider bestTarget = null;
        float bestDistance = float.MaxValue;

        foreach (Collider candidate in possibleTargets)
        {
            if (candidate == null) continue;

            HealthController health = candidate.GetComponentInParent<HealthController>();
            if (health == null) continue;

            float distance = (candidate.bounds.center - transform.position).sqrMagnitude;

            if (distance >= bestDistance) continue;

            bestDistance = distance;
            bestTarget = candidate;
        }

        if (bestTarget == currentTarget) return;

        currentTarget = bestTarget;

        ApplyTargetToTurrets();
    }

    private void ApplyTargetToTurrets()
    {
        foreach (TurretAimController turret in turretAims)
        {
            if (turret == null) continue;

            if (currentTarget != null)
                turret.SetLockedTarget(currentTarget);
            else
                turret.ClearLockedTarget();
        }
    }

    private void SetWeaponsFiring(bool value)
    {
        foreach (TurretWeapon weapon in turretWeapons)
        {
            if (weapon == null) continue;
            weapon.SetAIFire(value);
        }
    }

    private void ClearTarget()
    {
        currentTarget = null;

        foreach (TurretAimController turret in turretAims)
        {
            if (turret == null) continue;
            turret.ClearLockedTarget();
        }

        SetWeaponsFiring(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        if (currentTarget != null)
            Gizmos.DrawLine(transform.position, currentTarget.bounds.center);
    }
}