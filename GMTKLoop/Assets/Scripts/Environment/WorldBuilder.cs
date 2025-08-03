using System.Collections.Generic;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    public static WorldBuilder i;

    [Header("Grid Settings")]
    public WorldSettingsSO worldSettings; 

    [Header("Prefabs")]
    [SerializeField] private GameObject cellGFXPF;
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject selectorPF;
    [SerializeField] private GameObject escapePF;
    [SerializeField] private GameObject blueSpawnerPF;
    [SerializeField] private GameObject redSpawnerPF;

    [SerializeField] private GameObject groundPlane;

    private Grid<Cell> grid;
    private GameObject escapeMarker;
    private GameObject blueSpawner;
    private GameObject[] redSpawners;

    private List<RectInt> rooms = new List<RectInt>();
    private System.Random rng = new System.Random();


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
        Vector3 autoOffset = new Vector3(worldSettings.width, 0, worldSettings.height) / -2 * worldSettings.cellSize;

        Grid<Cell> grid = new Grid<Cell>(worldSettings.width, worldSettings.height, worldSettings.cellSize, worldSettings.autoOffsetCenter ? autoOffset : worldSettings.originPosition,
            (Grid<Cell> g, int x, int y) => new Cell(g, x, y, worldSettings.walkableColor, worldSettings.wallMaterial), true);

        groundPlane.transform.localScale = new Vector3((float)worldSettings.width / 2, 1, (float)worldSettings.height / 2);
        groundPlane.GetComponent<Renderer>().material.mainTextureScale = new Vector2((float)worldSettings.width / 2, (float)worldSettings.height / 2);

        GenerateBaseMap(grid);
        RegenerateWorld(grid);

        return grid;
    }

    public void RegenerateWorld(Grid<Cell> grid)
    {
        GenerateRoomsAndCorridors(grid);
        UpdateCells(grid);
        PlaceEscapeAndSpawners(grid);
        DisableAllSelectors();
    }

    private void GenerateBaseMap(Grid<Cell> grid)
    {
        Vector3[] wallPos = {
        new Vector3(0, 0, worldSettings.height * worldSettings.cellSize / 2),
        new Vector3(0, 0, -worldSettings.height * worldSettings.cellSize /2),
        new Vector3(worldSettings.width * worldSettings.cellSize / 2, 0, 0),
        new Vector3(-worldSettings.width * worldSettings.cellSize / 2, 0, 0)
        };

        Instantiate(wall, wallPos[0], Quaternion.identity, transform).transform.localScale = new Vector3(worldSettings.width, 1, 1);
        Instantiate(wall, wallPos[1], Quaternion.Euler(0, 180, 0), transform).transform.localScale = new Vector3(worldSettings.width, 1, 1);
        Instantiate(wall, wallPos[2], Quaternion.Euler(0, 90, 0), transform).transform.localScale = new Vector3(worldSettings.width, 1, 1);
        Instantiate(wall, wallPos[3], Quaternion.Euler(0, -90, 0), transform).transform.localScale = new Vector3(worldSettings.width, 1, 1);

        for (int x = 0; x < worldSettings.width; x++)
        {
            for (int y = 0; y < worldSettings.height; y++)
            {
                GameObject cellGFX = Instantiate(cellGFXPF, transform);
                cellGFX.transform.localScale = Vector3.one * worldSettings.cellSize;
                grid.GetGridObject(x, y).SetCellGFX(cellGFX);

                GameObject selector = Instantiate(selectorPF, transform);
                grid.GetGridObject(x, y).SetSelector(selector);
            }
        }
    }

    private void UpdateCells(Grid<Cell> grid)
    {
        for (int x = 0; x < worldSettings.width; x++)
        {
            for (int y = 0; y < worldSettings.height; y++)
            {
                Cell cell = grid.GetGridObject(x, y);
                if (cell != null)
                {
                    cell.UpdateCellGFX();
                }
            }
        }
    }

    private void ResetCells(Grid<Cell> grid)
    {
        for (int x = 0; x < worldSettings.width; x++)
        {
            for (int y = 0; y < worldSettings.height; y++)
            {
                Cell cell = grid.GetGridObject(x, y);
                if (cell != null)
                {
                    cell.IsWalkable = false;
                }
            }
        }
    }

    private void GenerateRoomsAndCorridors(Grid<Cell> grid)
    {
        ResetCells(grid);  

        rooms.Clear();

        int attempts = 0;
        int maxAttempts = worldSettings.roomCount * 10; // Arbitrary limit to avoid infinite loops

        while (rooms.Count < worldSettings.roomCount && attempts < maxAttempts)
        {
            attempts++;

            int roomWidth = rng.Next(worldSettings.minRoomSize, worldSettings.maxRoomSize);
            int roomHeight = rng.Next(worldSettings.minRoomSize, worldSettings.maxRoomSize);
            int roomX = rng.Next(1, worldSettings.width - roomWidth - 1);
            int roomY = rng.Next(1, worldSettings.height - roomHeight - 1);

            if (roomX < 0 || roomY < 0 || roomX + roomWidth >= worldSettings.width || roomY + roomHeight >= worldSettings.height)
                continue; // Room doesn't fit

            RectInt newRoom = new RectInt(roomX, roomY, roomWidth, roomHeight);

            bool overlaps = rooms.Exists(r => r.Overlaps(newRoom));
            if (overlaps)
                continue;

            rooms.Add(newRoom);

            for (int x = roomX; x < roomX + roomWidth; x++)
            {
                for (int y = roomY; y < roomY + roomHeight; y++)
                {
                    grid.GetGridObject(x, y).IsWalkable = true;
                }
            }

            if (rooms.Count > 1)
            {
                Vector2Int prevCenter = Vector2Int.RoundToInt((Vector2)rooms[rooms.Count - 2].center);
                Vector2Int currCenter = Vector2Int.RoundToInt((Vector2)newRoom.center);

                if (rng.Next(0, 2) == 0)
                {
                    CreateHorizontalTunnel(prevCenter.x, currCenter.x, prevCenter.y, grid);
                    CreateVerticalTunnel(prevCenter.y, currCenter.y, currCenter.x, grid);
                }
                else
                {
                    CreateVerticalTunnel(prevCenter.y, currCenter.y, prevCenter.x, grid);
                    CreateHorizontalTunnel(prevCenter.x, currCenter.x, currCenter.y, grid);
                }
            }
        }

        if (rooms.Count < worldSettings.roomCount)
            Debug.LogWarning($"Only generated {rooms.Count}/{worldSettings.roomCount} rooms. Consider increasing grid size.");
    }

    private void CreateHorizontalTunnel(int x1, int x2, int y, Grid<Cell> grid)
    {
        for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
            grid.GetGridObject(x, y).IsWalkable = true;
    }

    private void CreateVerticalTunnel(int y1, int y2, int x, Grid<Cell> grid)
    {
        for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
            grid.GetGridObject(x, y).IsWalkable = true;
    }

    private void PlaceEscapeAndSpawners(Grid<Cell> grid)
    {
        if (escapeMarker != null) Destroy(escapeMarker); 
        if (blueSpawner != null) Destroy(blueSpawner);
        if (redSpawners != null)
        {
            for (int i = 0; i < redSpawners.Length; i++)
            {
                if (redSpawners[i] != null) Destroy(redSpawners[i]);
            }
        }


        // Escape cell in last room
        Vector2Int escapePos = Vector2Int.RoundToInt(rooms[^1].center);
        GameManager.escapeMarker = escapeMarker = Instantiate(escapePF, grid.GetWorldPositionCellCenter(escapePos.x, escapePos.y), Quaternion.identity, transform);

        // Blue spawner in first room
        Vector2Int blueSpawn = Vector2Int.RoundToInt(rooms[0].center);
        blueSpawner = Instantiate(blueSpawnerPF, grid.GetWorldPositionCellCenter(blueSpawn.x, blueSpawn.y), Quaternion.identity, transform);

        redSpawners = new GameObject[rooms.Count - 2];
        // Enemy spawners in other rooms
        for (int i = 1; i < rooms.Count - 1; i++)
        {
            Vector2Int redSpawn = Vector2Int.RoundToInt(rooms[i].center);
            redSpawners[i - 1] = Instantiate(redSpawnerPF, grid.GetWorldPositionCellCenter(redSpawn.x, redSpawn.y), Quaternion.identity, transform);
            redSpawners[i - 1].name = $"RedSpawner_{i-1}";
        }
    }

    public static Grid<Cell> GetGrid() => i.grid;

    public List<UnitSpawner> GetAllUnitSpawners()
    {
        List<UnitSpawner> spawners = new List<UnitSpawner>();

        spawners.Add(blueSpawner.GetComponent<UnitSpawner>());

        for (int i = 0; i < redSpawners.Length; i++)
        {
            spawners.Add(redSpawners[i].GetComponent<UnitSpawner>());
        }
        
        return spawners;
    }

    public void DisableAllSelectors()
    {
        if (grid == null) return;
        for (int x = 0; x < grid.GetWidth(); x++)
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
