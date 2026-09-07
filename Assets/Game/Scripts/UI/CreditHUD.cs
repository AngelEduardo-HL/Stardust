using TMPro;
using UnityEngine;

public sealed class CreditHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text creditsText;

    private void Start()
    {
        if (CreditManager.Instance == null) return;

        CreditManager.Instance.CreditsChanged += UpdateCredits;
        UpdateCredits(CreditManager.Instance.CurrentCredits);
    }

    private void UpdateCredits(int credits)
    {
        if (creditsText != null)
            creditsText.text = $"{credits:N0} $";
    }

    private void OnDestroy()
    {
        if (CreditManager.Instance != null)
            CreditManager.Instance.CreditsChanged -= UpdateCredits;
    }
}