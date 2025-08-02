using UnityEngine;

public class Cell : IWalkable
{
    
    private int x;
    private int y;
    private Grid<Cell> grid;
    private float value;


    private bool isWalkable = true;
    public bool IsWalkable { get => isWalkable; set => isWalkable = value; }

    private bool isValidMovePos = true;
    public bool IsValidMovePos { get => isValidMovePos; set => isValidMovePos = value; }

    private Unit unit;

    private GameObject selector;

    public Cell(Grid<Cell> grid, int x, int y)
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

    public void SetSelector(GameObject selector)
    {
        this.selector = selector;
        selector.GetComponentInChildren<SpriteRenderer>().color = isWalkable ? Color.green : Color.red;
        if (selector != null)
        {
            selector.transform.position = grid.GetWorldPositionCellCenter(x, y);
            selector.SetActive(false);
        }
    }

    public float GetValue() => value;

    public Grid<Cell> GetGrid() => grid;

    public void ShowSelector(bool show) => selector.SetActive(show);

    public Unit GetUnit() => unit;

    public void SetUnit(Unit unit) => this.unit = unit;

    public void ClearUnit() => unit = null;

    public override string ToString()
    {
        return $"[{x},{y}, {IsWalkable}]";
    }
}

