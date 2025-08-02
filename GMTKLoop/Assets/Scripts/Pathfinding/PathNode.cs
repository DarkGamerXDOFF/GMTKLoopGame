public class PathNode<T> where T : IWalkable
{
    private Grid<PathNode<T>> grid;
    public int x { get; private set; }
    public int y { get; private set; }

    public int gCost;
    public int hCost;
    public int fCost;

    public T nodeObject { get; private set; }


    public PathNode<T> cameFromNode;

    public bool walkable { get; private set; }

    public PathNode(Grid<PathNode<T>> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
    }

    public void SetNodeObject(T nodeObject)
    {
        this.nodeObject = nodeObject;
        walkable = nodeObject.IsWalkable;
    }
    public T GetNodeObject() => nodeObject;

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    public override string ToString()
    {
        return $"{x},{y}";
    }
}
