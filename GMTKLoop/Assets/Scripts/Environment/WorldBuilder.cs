using System;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    public static WorldBuilder i;

    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [SerializeField] private float cellSize = 5f;
    [SerializeField] private Vector3 originPosition;
    [SerializeField] private bool autoOffsetCenter = true;

    [SerializeField] private bool useRandomWalkableCells = false;
    [SerializeField] private float walkTreshhold = 0.3f;

    [SerializeField] private GameObject selectorPF;

    private Grid<Cell> grid;


    private void Awake()
    {
        if (i == null)
            i = this;
        else
            Destroy(gameObject);

        grid = GenerateGrid();

    }

    private Grid<Cell> GenerateGrid()
    {
        Vector3 autoOffset = new Vector3(width, 0, height) / -2 * cellSize;

        Grid<Cell> grid = new Grid<Cell>(width, height, cellSize, autoOffsetCenter ? autoOffset : originPosition,
            (Grid<Cell> g, int x, int y) => new Cell(g, x, y), true);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (useRandomWalkableCells)
                {
                    bool isWalkable = UnityEngine.Random.value > walkTreshhold;
                    grid.GetGridObject(x, y).IsWalkable = isWalkable;
                }

                GameObject selector = Instantiate(selectorPF, transform);
                grid.GetGridObject(x, y).SetSelector(selector);
            }
        }

        return grid;
    }


    public static Grid<Cell> GetGrid() => i.grid;

    public void DisableAllSelectors()
    {
        for(int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                grid.GetGridObject(x, y).ShowSelector(false);
            }
        }
    }

    public void SetAllValidMovePositions(bool state)
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                Cell cell = grid.GetGridObject(x, y);
                
                if (cell != null) cell.IsValidMovePos = state;
            }
        }
    }
}
