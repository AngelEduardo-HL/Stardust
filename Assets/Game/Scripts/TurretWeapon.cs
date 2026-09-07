using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TurretWeapon : MonoBehaviour
{
    public enum FireControlMode
    {
        Player,
        AI
    }

    [Header("Referencias")]
    [SerializeField]
    private TurretAimController aimController;

    [SerializeField]
    private Transform firePoint;

    [SerializeField]
    private ProjectileController projectilePrefab;

    [Header("Control")]
    [SerializeField] private FireControlMode fireControlMode = FireControlMode.Player;
    private bool aiFireRequested;

    [Header("Proyectil")]
    [Tooltip("Daño causado por cada disparo.")]
    [SerializeField, Min(0f)]
    private float projectileDamage = 10f;

    [Tooltip("Velocidad del proyectil.")]
    [SerializeField, Min(0.1f)]
    private float projectileSpeed = 150f;

    [Tooltip("Multiplicador del tamaño visual y de impacto.")]
    [SerializeField, Min(0.01f)]
    private float projectileSize = 1f;

    [Tooltip("Separación inicial desde el FirePoint.")]
    [SerializeField, Min(0f)]
    private float spawnOffset = 0.1f;

    [SerializeField]
    private LayerMask projectileHitMask;

    [Header("Cadencia")]
    [Tooltip("Cooldown mínimo entre disparos.")]
    [SerializeField, Min(0.01f)]
    private float minimumShotCooldown = 0.30f;

    [Tooltip("Cooldown máximo entre disparos.")]
    [SerializeField, Min(0.01f)]
    private float maximumShotCooldown = 0.40f;

    [Tooltip("Variación inicial para que varias torretas " + "no comiencen exactamente al mismo tiempo.")]
    [SerializeField, Min(0f)]
    private float initialFireSpread = 0.12f;


    private float nextShotTime;
    private bool wasFirePressed;


    private void Awake()
    {
        if (aimController == null)
        {
            aimController = GetComponent <TurretAimController>();
        }


        if (minimumShotCooldown > maximumShotCooldown)
        {
            float temporary = minimumShotCooldown;

            minimumShotCooldown = maximumShotCooldown;

            maximumShotCooldown = temporary;
        }
    }


    private void Update()
    {
        bool firePressed = false;

        if (fireControlMode == FireControlMode.Player)
        {
            if (Mouse.current == null) return;

            firePressed = Mouse.current.leftButton.isPressed;
        }
        else
        {
            firePressed = aiFireRequested;
        }

        if (firePressed && !wasFirePressed)
            nextShotTime = Time.time + Random.Range(0f, initialFireSpread);

        wasFirePressed = firePressed;

        if (!firePressed) return;

        if (aimController != null && aimController.HasLockedTarget && !aimController.IsLockedTargetInRange)
            return;

        if (Time.time < nextShotTime) return;

        FireProjectile();

        nextShotTime = Time.time + Random.Range(minimumShotCooldown, maximumShotCooldown);
    }

    private void FireProjectile()
    {
        if (firePoint == null ||projectilePrefab == null ||aimController == null)
        {
            return;
        }

        Vector3 spawnPosition = firePoint.position + firePoint.forward * spawnOffset;

        ProjectileController projectile = Instantiate( projectilePrefab, spawnPosition, firePoint.rotation);
        projectile.Initialize(projectileDamage, projectileSpeed, aimController.WeaponRange, projectileSize,projectileHitMask);
    }
    public void SetAIFire(bool value)
    {
        aiFireRequested = value;
    }

    public void SetFireControlMode(FireControlMode mode)
    {
        fireControlMode = mode;
    }
}