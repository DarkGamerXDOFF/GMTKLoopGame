using UnityEngine;

public class Cell : IWalkable
{
    
    private int x;
    private int y;
    private Grid<Cell> grid;
    private float value;


    private bool isWalkable = false;
    public bool IsWalkable { get => isWalkable; set => isWalkable = value; }

    private bool isValidMovePos = true;
    public bool IsValidMovePos { get => isValidMovePos; set => isValidMovePos = value; }

    private Unit unit;

    private GameObject CellGFX;
    private GameObject selector;

    public Cell(Grid<Cell> grid, int x, int y, Color selectorColor, Material wallMaterial)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void SetValue(float value)
    {
        this.value = value;
        grid.TriggerGridObjectChanged(x,y);
    }
    public float GetValue() => value;

    public void SetSelector(GameObject selector)
    {
        this.selector = selector;
        selector.GetComponentInChildren<SpriteRenderer>().color = Color.green;
        if (selector != null)
        {
            selector.transform.position = grid.GetWorldPositionCellCenter(x, y);
            selector.SetActive(false);
        }
    }


    public Grid<Cell> GetGrid() => grid;

    public void ShowSelector(bool show) => selector.SetActive(show);

    public Unit GetUnit() => unit;

    public void SetUnit(Unit unit) => this.unit = unit;

    public void ClearUnit() => unit = null;

    public void SetCellGFX(GameObject cellGFX)
    {
        CellGFX = cellGFX;
        if (CellGFX != null)
        {
            Vector3 cellPosition = IsWalkable ? grid.GetWorldPositionCellCenter(x, y) : grid.GetWorldPositionCellCenter(x, y) + new Vector3(0, grid.GetCellSize(), 0);
            CellGFX.transform.position = cellPosition;
        }
    }

    public void UpdateCellGFX()
    {
        if (CellGFX != null)
        {
            Vector3 cellPosition = IsWalkable ? 
                grid.GetWorldPositionCellCenter(x, y) - new Vector3(0, grid.GetCellSize(), 0) : 
                grid.GetWorldPositionCellCenter(x, y) + new Vector3(0, grid.GetCellSize(), 0);
            CellGFX.transform.position = cellPosition;
        }
    }

    public override string ToString()
    {
        return $"[{x},{y}, {IsWalkable}]";
    }
}

