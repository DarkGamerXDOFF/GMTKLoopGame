using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitMovement), typeof(Animator))]
public class Unit : MonoBehaviour
{
    [SerializeField] private Team team = Team.Player;

    private UnitMovement unitMovement;
    private LineRenderer lineRenderer;
    private Animator animator;
    public HealthSystem healthSystem;

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private HealthBar healthBar;

    private Grid<Cell> grid;

    [SerializeField] private bool showPath = true;
    [SerializeField] private float lineHeightOffset = 0.1f;

    private bool isMoving = false;

    private Action OnAttackCompleted;

    [SerializeField] private int attackDamage = 10;

    public int AttackDamage
    {
        get { return attackDamage; }
        private set { attackDamage = value; }
    }


    private void Awake()
    {
        unitMovement = GetComponent<UnitMovement>();
        lineRenderer = GetComponent<LineRenderer>();
        animator = GetComponent<Animator>();

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.widthMultiplier = 0.2f;
        }

        healthSystem = new HealthSystem(maxHealth);
        healthSystem.OnUnitDied += OnUnitDeath;
        healthBar.Setup(healthSystem);
    }

    public void OnEnable()
    {
        grid = WorldBuilder.GetGrid();
    }

    private void OnDisable()
    {
        GridCombatSystem.Unsubscribe(this);
    }

    public void MoveTo(Vector3 destination, Action OnDestinationReached = null)
    {
        if (isMoving)
        {
            return;
        }

        if (grid == null || Pathfinder.i == null)
        {
            Debug.LogError($"Unit not initialized with a valid {grid} and {Pathfinder.i}.");
            return;
        }

        Cell startCell = grid.GetGridObject(transform.position);
        Cell endCell = grid.GetGridObject(destination);

        if (startCell == null || endCell == null || !endCell.IsWalkable)
        {
            Debug.LogWarning("Invalid destination or unreachable.");
            return;
        }

        List<PathNode<Cell>> path = Pathfinder.FindPath(startCell, endCell);

        if (path == null || path.Count == 0)
        {
            return;
        }

        if (showPath && lineRenderer != null)
            DrawPathLine(path);

        isMoving = true;

        unitMovement.SetPath(path, grid, () =>
        {
            isMoving = false;

            if (lineRenderer != null)
                lineRenderer.positionCount = 0;

            OnDestinationReached?.Invoke();
        });
    }

    private void DrawPathLine(List<PathNode<Cell>> path)
    {
        Vector3[] positions = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            grid.GetObjectGridPosition(path[i].GetNodeObject(), out int x, out int y);
            Vector3 worldPos = grid.GetWorldPositionCellCenter(x, y);
            positions[i] = new Vector3(worldPos.x, worldPos.y + lineHeightOffset, worldPos.z);
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    public bool IsMoving() => isMoving;

    public Vector3 GetGridPosition()
    {
        grid.GetXY(transform.position, out int x, out int z);
        return new Vector3(x, 0, z);
    }

    public void OnAnimationComplete()
    {
        OnAttackCompleted?.Invoke();
        OnAttackCompleted = null;
    }

    public void OnUnitDeath()
    {
        GridCombatSystem.Unsubscribe(this);
        animator.SetTrigger("Die");
        GridCombatSystem.i.CheckTeamDefeat(); // Add this line
    }

    public void OnUnitDeathAnimationCompleted()
    {
        // Cleanup logic after death animation
        Destroy(gameObject);
    }

    public bool IsEnemy(Unit unit)
    {
        return unit.team != team;
    }

    public void AttackUnit(Unit unit, Action OnAttackCompleted)
    {
        animator.SetTrigger("Attack");
        this.OnAttackCompleted = OnAttackCompleted;
    }

    public bool CanAttackUnit(Unit unit)
    {
        return grid.GetGridObject(unit.transform.position).IsValidMovePos;
    }

    public void TakeDamage(int damage)
    {
        healthSystem.TakeDamage(damage);

        //if (healthSystem.IsDead()) // You'll need this helper in HealthSystem
        //{
        //    OnUnitDeath(); // Immediately trigger cleanup
        //}
    }

    public Team GetTeam()
    {
        return team;
    }

    public void SetTeam(Team newTeam)
    {
        team = newTeam;
    }
}
public enum Team { Player, Enemy }
