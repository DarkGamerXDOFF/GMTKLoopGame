using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitMovement), typeof(Animator), typeof(Rigidbody))]
public class Unit : MonoBehaviour
{
    [SerializeField] private Team team = Team.Blue;

    private UnitMovement unitMovement;
    private LineRenderer lineRenderer;
    private Animator animator;
    private Rigidbody rb;
    public HealthSystem healthSystem;

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private HealthBar healthBar;

    private Grid<Cell> grid;

    [SerializeField] private bool showPath = true;
    [SerializeField] private float lineHeightOffset = 0.1f;

    private bool isMoving = false;

    private Action OnAttackCompleted;

    [SerializeField] private int attackDamage = 10;

    [SerializeField] private UnitSpawner unitSpawner;

    private Vector3? lookTarget;

    [SerializeField] private float rotationSpeed = 360f; // Degrees per second

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
        Rigidbody rb = GetComponent<Rigidbody>();

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

    private void Update()
    {
        if (lookTarget.HasValue)
        {
            Vector3 direction = (lookTarget.Value - transform.position).normalized;
            direction.y = 0; // Keep the rotation on the horizontal plane
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                if (Quaternion.Angle(transform.rotation, targetRotation) < 0.5f)
                {
                    transform.rotation = targetRotation; // Snap to the target rotation if close enough
                    lookTarget = null; // Stop looking at the target once close enough
                }
            }
        }
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


    public void LookAt(Vector3 targetPosition) => lookTarget = targetPosition;

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
        Destroy(gameObject);
    }

    public bool IsEnemy(Unit unit) => unit.team != team;

    public void AttackUnit(Unit unit, Action OnAttackCompleted)
    {
        LookAt(unit.transform.position);
        animator.SetTrigger("Attack");
        this.OnAttackCompleted = OnAttackCompleted;
    }

    public bool CanAttackUnit(Unit unit) => grid.GetGridObject(unit.transform.position).IsValidMovePos;

    public void TakeDamage(int damage) => healthSystem.TakeDamage(damage);

    public Team GetTeam() => team;


    public void SetTeam(Team newTeam) => team = newTeam;

    public UnitSpawner GetSpawner() => unitSpawner;

    public void SetSpawner(UnitSpawner spawner) => unitSpawner = spawner;

    public void ClearSpawner() => unitSpawner = null;
}
public enum Team { Blue, Red }
