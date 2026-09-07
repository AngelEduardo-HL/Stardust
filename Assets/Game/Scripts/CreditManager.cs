using System;
using UnityEngine;

public sealed class CreditManager : MonoBehaviour
{
    public static CreditManager Instance { get; private set; }

    [Header("Creditos")]
    [SerializeField, Min(0)] private int startingCredits = 1250;
    [SerializeField] private int currentCredits;

    public int CurrentCredits => currentCredits;

    public event Action<int> CreditsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        currentCredits = startingCredits;
    }

    public void AddCredits(int amount)
    {
        if (amount <= 0) return;

        currentCredits += amount;
        CreditsChanged?.Invoke(currentCredits);
    }

    public bool TrySpendCredits(int amount)
    {
        if (amount <= 0) return true;
        if (currentCredits < amount) return false;

        currentCredits -= amount;
        CreditsChanged?.Invoke(currentCredits);

        return true;
    }
}