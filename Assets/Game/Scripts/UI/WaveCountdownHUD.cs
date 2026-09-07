using TMPro;
using UnityEngine;

public sealed class WaveCountdownHUD : MonoBehaviour
{
    [SerializeField] private GameObject countdownRoot;
    [SerializeField] private TMP_Text countdownText;

    private void Awake()
    {
        HideCountdown();
    }

    public void ShowCountdown(int nextWave, float seconds)
    {
        if (countdownRoot != null)
            countdownRoot.SetActive(true);

        if (countdownText != null)
            countdownText.text = $"OLEADA {nextWave} EN\n{Mathf.CeilToInt(seconds)}";
    }

    public void HideCountdown()
    {
        if (countdownRoot != null)
            countdownRoot.SetActive(false);
    }
}