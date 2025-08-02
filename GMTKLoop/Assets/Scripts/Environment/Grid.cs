using UnityEngine;
using System;

public class Grid<TGridObject> 
{
    private bool debugPositions = false;

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;

    private TGridObject[,] gridArray;
    private TextMesh[,] debugTextArray;
    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int,TGridObject> createGridObject, bool showDebug = false)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int y = 0; y < gridArray.GetLength(0); y++)
        {
            for (int x = 0; x < gridArray.GetLength(1); x++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        if (showDebug)
        {
            debugTextArray = new TextMesh[width, height];


            for (int y = 0; y < gridArray.GetLength(0); y++)
            {
                for (int x = 0; x < gridArray.GetLength(1); x++)
                {
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

            OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) =>
            {
                debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }
    }

    public Vector3 GetWorldPosition(int x, int y) => new Vector3(x, 0, y) * cellSize + originPosition;
    public Vector3 GetWorldPositionCellCenter(int x, int y) => GetWorldPosition(x, y) + new Vector3(1,0,1) * cellSize / 2;

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
    }

    public void GetXY(Vector3 worldPosition, out Vector3 pos)
    {
        int x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        int y = Mathf.FloorToInt((worldPosition - originPosition).z / cellSize);
        pos = new Vector3(x, 0, y);
    }

    public void GetObjectGridPosition(TGridObject gridObject, out int x, out int y)
    {
        x = 0;
        y = 0;

        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                if (gridArray[i, j].Equals(gridObject))
                {
                    x = i; y = j;
                    break;
                }
            }
        }
    }

    public void SetGridObject(int x, int y, TGridObject value) 
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }
        else
            if (debugPositions)
                Debug.LogError($"Coordiantes invalid: {x},{y}");
    }

    public void TriggerGridObjectChanged(int x, int y)
    {
        if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
            if (debugPositions)
                Debug.LogError($"Coordiantes invalid: {x},{y}");

        return default;
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

    public int GetWidth() => width;
    public int GetHeight() => height;

    public float GetCellSize() => cellSize;
}