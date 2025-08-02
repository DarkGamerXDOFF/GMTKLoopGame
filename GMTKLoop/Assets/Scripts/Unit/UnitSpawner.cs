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
        WorldBuilder.GetGrid().GetGridObject(unit.transform.position).SetUnit(unit);
        unit.SetTeam(team);
        unit.SetSpawner(this);
    }

    public void RemoveUnit()
    {
        if ( unit != null)
        {
            unit.ClearSpawner();
            Destroy(unit.gameObject);
        }
    }

    public void ResetUnitPosition()
    {
        if (unit != null)
        {
            WorldBuilder.GetGrid().GetGridObject(unit.transform.position).ClearUnit();
            unit.transform.position = transform.position;
            WorldBuilder.GetGrid().GetGridObject(unit.transform.position).SetUnit(unit);
        }
    }
}
