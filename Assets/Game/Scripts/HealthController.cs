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


    private void Awake()
    {
        currentHealth = maxHealth;
    }


    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }


        currentHealth =
            Mathf.Max(
                currentHealth - damage,
                0f
            );


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
        Debug.Log(
            $"{name} destruido.",
            this
        );

        Destroy(gameObject);
    }
}