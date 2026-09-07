using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ObjectiveManager : MonoBehaviour
{
    [Header("Estacion")]
    [SerializeField] private HealthController spaceStation;

    [Header("Escenas")]
    [SerializeField] private string victorySceneName = "VictoryMenu";
    [SerializeField] private string defeatSceneName = "DefeatMenu";

    private bool gameEnded;

    private void Start()
    {
        if (spaceStation == null)
        {
            Debug.LogError("ObjectiveManager: no hay estacion asignada.", this);
            return;
        }

        spaceStation.Died += OnStationDestroyed;
    }

    private void OnStationDestroyed(HealthController station)
    {
        TriggerDefeat();
    }

    public void TriggerVictory()
    {
        if (gameEnded) return;

        gameEnded = true;
        SceneManager.LoadScene(victorySceneName);
    }

    public void TriggerDefeat()
    {
        if (gameEnded) return;

        gameEnded = true;
        SceneManager.LoadScene(defeatSceneName);
    }

    private void OnDestroy()
    {
        if (spaceStation != null)
            spaceStation.Died -= OnStationDestroyed;
    }
}