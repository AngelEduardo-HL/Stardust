using UnityEngine;

[RequireComponent(typeof(HealthController))]
public sealed class ShipCreditReward : MonoBehaviour
{
    [Header("Recompensas")]
    [SerializeField, Min(0)] private int killReward = 800;
    [SerializeField, Min(0)] private int playerHitReward = 1;

    private HealthController health;

    private void Awake()
    {
        health = GetComponent<HealthController>();
        health.DiedWithSource += OnDestroyed;
    }

    public void RegisterHit(DamageSourceType source)
    {
        if (source != DamageSourceType.Player) return;

        CreditManager.Instance?.AddCredits(playerHitReward);
    }

    private void OnDestroyed(HealthController destroyedHealth, DamageSourceType source)
    {
        if (source != DamageSourceType.Player && source != DamageSourceType.Ally) return;

        CreditManager.Instance?.AddCredits(killReward);
    }

    private void OnDestroy()
    {
        if (health != null)
            health.DiedWithSource -= OnDestroyed;
    }
}