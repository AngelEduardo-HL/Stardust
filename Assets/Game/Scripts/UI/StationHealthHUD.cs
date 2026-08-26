using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class StationHealthHUD : MonoBehaviour
{
    [Header("Referencias")]

    [SerializeField]
    private HealthController stationHealth;

    [SerializeField]
    private Slider healthSlider;

    [SerializeField]
    private TMP_Text healthText;


    private void Start()
    {
        if (stationHealth == null)
        {
            return;
        }


        stationHealth.HealthChanged +=
            OnHealthChanged;


        RefreshHUD();
    }


    private void OnHealthChanged(
        HealthController health
    )
    {
        RefreshHUD();
    }


    private void RefreshHUD()
    {
        if (stationHealth == null)
        {
            return;
        }


        if (healthSlider != null)
        {
            healthSlider.value =
                stationHealth.HealthPercentage;
        }


        if (healthText != null)
        {
            healthText.text =
                $"ESTACIÓN  " +
                $"{stationHealth.CurrentHealth:0} / " +
                $"{stationHealth.MaxHealth:0}";
        }
    }


    private void OnDestroy()
    {
        if (stationHealth != null)
        {
            stationHealth.HealthChanged -=
                OnHealthChanged;
        }
    }
}