using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerHealthHUD : MonoBehaviour
{
    [SerializeField] private GameObject healthRoot;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    private HealthController currentHealth;

    private void Awake()
    {
        if (healthRoot != null)
            healthRoot.SetActive(false);
    }

    public void SetShip(HealthController health)
    {
        ClearShip();

        currentHealth = health;

        if (currentHealth == null) return;

        currentHealth.HealthChanged += OnHealthChanged;

        if (healthRoot != null)
            healthRoot.SetActive(true);

        Refresh();
    }

    public void ClearShip()
    {
        if (currentHealth != null)
            currentHealth.HealthChanged -= OnHealthChanged;

        currentHealth = null;

        if (healthRoot != null)
            healthRoot.SetActive(false);
    }

    private void OnHealthChanged(HealthController health)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (currentHealth == null) return;

        if (healthSlider != null)
            healthSlider.value = currentHealth.HealthPercentage;

        if (healthText != null)
            healthText.text = $"{currentHealth.CurrentHealth:0} / {currentHealth.MaxHealth:0}";
    }

    private void OnDestroy()
    {
        if (currentHealth != null)
            currentHealth.HealthChanged -= OnHealthChanged;
    }
}