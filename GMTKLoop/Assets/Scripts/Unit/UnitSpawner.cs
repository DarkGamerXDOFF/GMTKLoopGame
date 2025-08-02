using System;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private Unit unit;

    [SerializeField] private Team team;


    public void SpawnUnit()
    {
        if (unitPrefab == null)
        {
            Debug.LogError("Unit Prefab is not assigned in the UnitSpawner.");
            return;
        }
        GameObject unitGO = Instantiate(unitPrefab, transform.position, Quaternion.identity);
        unit = unitGO.GetComponent<Unit>();
        unit.SetTeam(team);
    }

    public void RemoveUnit()
    {
        if ( unit != null)
            Destroy(unit.gameObject);
    }
}
