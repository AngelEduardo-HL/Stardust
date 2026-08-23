using System;
using UnityEngine;

public sealed class HealthController : MonoBehaviour
{
    [Header("Vida")]

    [SerializeField, Min(1f)]
    private float maxHealth = 500f;

    [SerializeField]
    private float currentHealth;


    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public float HealthPercentage =>
        maxHealth > 0f
            ? currentHealth / maxHealth
            : 0f;


    public event Action<HealthController> HealthChanged;
    public event Action<HealthController> Died;


    private bool isDead;


    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }


    public void TakeDamage(float damage)
    {
        if (damage <= 0f || isDead)
        {
            return;
        }


        currentHealth =
            Mathf.Max(
                currentHealth - damage,
                0f
            );


        HealthChanged?.Invoke(this);


        Debug.Log(
            $"{name} recibio {damage} de daño. " +
            $"Vida: {currentHealth}/{maxHealth}",
            this
        );


        if (currentHealth <= 0f)
        {
            Die();
        }
    }


    private void Die()
    {
        if (isDead)
        {
            return;
        }


        isDead = true;


        Debug.Log(
            $"{name} destruido.",
            this
        );

        Died?.Invoke(this);


        Destroy(gameObject);
    }
}