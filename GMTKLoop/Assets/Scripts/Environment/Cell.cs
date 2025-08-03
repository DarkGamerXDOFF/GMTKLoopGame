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

    private GameObject cellGFX;
    private GameObject selector;

    private Mesh fullMesh;
    private Mesh emptyMesh;

    private Color walkableColor;
    private Material wallMaterial;


    public Cell(Grid<Cell> grid, int x, int y, Color selectorColor, Material wallMaterial, Mesh fullMesh, Mesh emptyMesh)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;

        this.fullMesh = fullMesh;
        this.emptyMesh = emptyMesh;
        this.wallMaterial = wallMaterial;
        walkableColor = selectorColor;
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
        selector.GetComponentInChildren<SpriteRenderer>().color = walkableColor;
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
        this.cellGFX = cellGFX;
        if (this.cellGFX != null)
        {
            Vector3 cellPosition = grid.GetWorldPositionCellCenter(x, y);
            this.cellGFX.transform.position = cellPosition;
            cellGFX.GetComponentInChildren<MeshFilter>().mesh = IsWalkable ? emptyMesh : fullMesh;
            cellGFX.GetComponentInChildren<MeshRenderer>().material = wallMaterial;
        }
    }

    public void UpdateCellGFX()
    {
        if (cellGFX != null)
        {
            Vector3 cellPosition = grid.GetWorldPositionCellCenter(x, y);
            cellGFX.transform.position = cellPosition;
            cellGFX.GetComponentInChildren<MeshFilter>().mesh = IsWalkable ? emptyMesh : fullMesh;
        }
    }

    public override string ToString()
    {
        return $"[{x},{y}, {IsWalkable}]";
    }
}

