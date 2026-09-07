using System;
using UnityEngine;

public sealed class HealthController : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField, Min(1f)] private float maxHealth = 500f;
    [SerializeField] private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float HealthPercentage => maxHealth > 0f ? currentHealth / maxHealth : 0f;
    public bool IsDead => isDead;

    public event Action<HealthController> HealthChanged;
    public event Action<HealthController> Died;
    public event Action<HealthController, DamageSourceType> DiedWithSource;

    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        TakeDamage(damage, DamageSourceType.None);
    }

    public void TakeDamage(float damage, DamageSourceType source)
    {
        if (damage <= 0f || isDead) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);
        HealthChanged?.Invoke(this);

        if (currentHealth <= 0f)
            Die(source);
    }

    private void Die(DamageSourceType source)
    {
        if (isDead) return;

        isDead = true;

        Died?.Invoke(this);
        DiedWithSource?.Invoke(this, source);

        Destroy(gameObject);
    }

}