using UnityEngine;
using System.Collections.Generic;

public class Pathfinder : MonoBehaviour
{
    static public Pathfinder i;

    private Grid<Cell> grid;
    private Pathfinding<Cell> pathfinding;

    [SerializeField] private bool moveDiagonal = false;

    private void Awake()
    {
        if (i == null)
            i = this;
        else
            Destroy(gameObject);

    }

    public void Init()
    {
        grid = WorldBuilder.GetGrid();
        pathfinding = new Pathfinding<Cell>(grid, moveDiagonal);
    }

    public static List<PathNode<Cell>> FindPath(Cell startCell, Cell endCell)
    {
        if (i == null || i.pathfinding == null)
        {
            Debug.LogError("Pathfinder not initialized.");
            return null;
        }
        return i.pathfinding.FindPath(startCell, endCell);
    }

    public static bool HasPath(Cell startCell, Cell endCell)
    {
        if (i == null || i.pathfinding == null)
        {
            Debug.LogError("Pathfinder not initialized.");
            return false;
        }
        return i.pathfinding.HasPath(startCell, endCell);
    }
}
