using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    

    public static GameManager i;

    private List<UnitSpawner> unitSpawners;

    [SerializeField] private int currentRound = 0;

    private const string HighScoreKey = "HighRound";

    public static GameObject escapeMarker;

    [SerializeField] private Cell escapeCell;

    [SerializeField] private bool debugMode = false;

    private void Awake()
    {
        if (i == null)
            i = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UIManager.i.OpenMenu("Main", (menu) => menu.SetHighscoreText());
    }

    
    public void StartRound()
    {
        WorldBuilder.i.RegenerateWorld(WorldBuilder.GetGrid());

        // Logic to start the round
        Pathfinder.i.Init();
        SpawnAllUnits();
        escapeCell = WorldBuilder.GetGrid().GetGridObject(escapeMarker.transform.position);
        GridCombatSystem.i.StartRound();
        
        currentRound++;
        CheckAndUpdateHighRound();
        
        if (debugMode)
            Debug.Log("Round started.");
    }

    private void SpawnAllUnits()
    {
        unitSpawners = WorldBuilder.i.GetAllUnitSpawners();

        if (unitSpawners.Count > 0)
        {
            for (int i = 0; i < unitSpawners.Count; i++)
            {
                unitSpawners[i].SpawnUnit();
            }
        }
        else
        {
            if (debugMode)
                Debug.LogWarning("No UnitSpawners found in the scene.");
        }
    }

    public void RemoveAllUnits()
    {
        if (unitSpawners.Count > 0)
        {
            for (int i = 0; i < unitSpawners.Count; i++)
            {
                unitSpawners[i].RemoveUnit();
            }
        }
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public void ResetRounds()
    {
        if (debugMode)
            Debug.Log("Resetting rounds.");
        currentRound = 0;
    }

    public int GetHighRound()
    {
        return PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    public void ResetHighRound()
    {
        PlayerPrefs.DeleteKey(HighScoreKey);
        if (debugMode)
            Debug.Log("High round score reset.");
    }

    private void CheckAndUpdateHighRound()
    {
        int highRound = GetHighRound();
        if (currentRound > highRound)
        {
            PlayerPrefs.SetInt(HighScoreKey, currentRound);
            PlayerPrefs.Save(); // Always save after setting
            if (debugMode)
                Debug.Log($"New high round reached: {currentRound}");
        }
    }

    public void ResetBlueUnitsToSpawn(List<Unit> units)
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
            {
                units[i].GetSpawner()?.ResetUnitPosition();
            }
            else
            {
                if (debugMode)
                    Debug.LogWarning($"Unit at index {i} is null, cannot reset to spawn.");
            }
        }
    }

    public Cell GetEscapeCell() => escapeCell;
}
