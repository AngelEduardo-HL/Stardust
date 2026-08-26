using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ObjectiveManager : MonoBehaviour
{
    [Header("Estacion espacial")]

    [SerializeField]
    private HealthController spaceStation;


    [Header("Objetivos enemigos")]

    [SerializeField]
    private HealthController[] enemyTargets;

    [SerializeField, Min(1)]
    private int requiredKills = 2;


    [Header("Escenas")]

    [SerializeField]
    private string victorySceneName = "VictoryMenu";

    [SerializeField]
    private string defeatSceneName = "DefeatMenu";


    [Header("Debug")]

    [SerializeField]
    private int currentKills;


    private bool gameEnded;


    private void Start()
    {
        SubscribeToStation();

        SubscribeToEnemies();
    }


    private void SubscribeToStation()
    {
        if (spaceStation == null)
        {
            Debug.LogError(
                "PrototypeObjectiveManager: " +
                "no hay estacion espacial asignada.",
                this
            );

            return;
        }


        spaceStation.Died +=
            OnStationDestroyed;
    }


    private void SubscribeToEnemies()
    {
        if (enemyTargets == null)
        {
            return;
        }


        foreach (
            HealthController enemy
            in enemyTargets
        )
        {
            if (enemy == null)
            {
                continue;
            }


            enemy.Died +=
                OnEnemyDestroyed;
        }
    }


    private void OnEnemyDestroyed(
        HealthController enemy
    )
    {
        if (gameEnded)
        {
            return;
        }


        // Evitamos contar nuevamente
        // el mismo enemigo.
        enemy.Died -=
            OnEnemyDestroyed;


        currentKills++;


        Debug.Log(
            $"Objetivo destruido. " +
            $"{currentKills}/{requiredKills}",
            this
        );


        if (currentKills >=
            requiredKills)
        {
            Victory();
        }
    }


    private void OnStationDestroyed(
        HealthController station
    )
    {
        if (gameEnded)
        {
            return;
        }


        Defeat();
    }


    private void Victory()
    {
        if (gameEnded)
        {
            return;
        }


        gameEnded = true;


        Debug.Log(
            "VICTORIA",
            this
        );


        SceneManager.LoadScene(
            victorySceneName
        );
    }


    private void Defeat()
    {
        if (gameEnded)
        {
            return;
        }


        gameEnded = true;


        Debug.Log(
            "DERROTA",
            this
        );


        SceneManager.LoadScene(
            defeatSceneName
        );
    }


    private void OnDestroy()
    {
        if (spaceStation != null)
        {
            spaceStation.Died -=
                OnStationDestroyed;
        }


        if (enemyTargets == null)
        {
            return;
        }


        foreach (
            HealthController enemy
            in enemyTargets
        )
        {
            if (enemy != null)
            {
                enemy.Died -=
                    OnEnemyDestroyed;
            }
        }
    }
}