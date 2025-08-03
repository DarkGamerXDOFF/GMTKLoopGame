using System.Collections.Generic;
using UnityEngine;

public class GridCombatSystem : MonoBehaviour
{
    public static GridCombatSystem i;

    public GameSettingsSO gameSettings;

    private List<Unit> units;
    [SerializeField] private Unit activeUnit;

    private List<Unit> blueTeam;
    private List<Unit> redTeam;

    [SerializeField] private int blueTeamActiveUnitIndex;
    [SerializeField] private int redTeamActiveUnitIndex;

    //[SerializeField] private int maxMoveDistance = 5;

    private Grid<Cell> grid;

    private enum State { Normal, Waiting }
    [SerializeField] private State state;

    [SerializeField] private bool canMoveThisTurn;
    [SerializeField] private bool canAttackThisTurn;

    [SerializeField] private int currentActionPoints;

    public int MaxActionPoints
    {
        get { return gameSettings.maxActionPoints; }
        private set { gameSettings.maxActionPoints = value; }
    }
    public int CurrentActionPoints
    {
        get { return currentActionPoints; }
        private set 
        { 
            currentActionPoints = value;
            UIManager.i.OpenMenu("Game", (menu) => menu.SetActionPointsText());
        }
    }

    [SerializeField] private bool debugMode = false;

    private void Awake()
    {
        if (i == null)
            i = this;
        else
            Destroy(gameObject);

        blueTeam = new List<Unit>();
        redTeam = new List<Unit>();

        blueTeamActiveUnitIndex = -1;
        redTeamActiveUnitIndex = -1;

        state = State.Waiting;
    }

    public void StartRound()
    {
        CurrentActionPoints = MaxActionPoints;

        grid = WorldBuilder.GetGrid();
        units = new List<Unit>(FindObjectsByType<Unit>(FindObjectsSortMode.InstanceID));

        foreach (Unit unit in units)
        {
            grid.GetGridObject(unit.transform.position).SetUnit(unit);
            if (unit.GetTeam() != Team.Blue)
                redTeam.Add(unit);
            else
                blueTeam.Add(unit);
        }

        SelectNextActiveUnit();
        state = State.Normal;
    }

    private void SetActiveUnit(Unit unit)
    {
        activeUnit = unit;
        UpdateValidMovePositions();
        CameraController.SetTarget(activeUnit.transform);
    }

    private void Update()
    {
        if (activeUnit == null)
            return;

        switch (state)
        {
            case State.Normal:
                if (Input.GetMouseButtonDown(0))
                {
                    Cell currentCell = grid.GetGridObject(activeUnit.transform.position);
                    
                    Vector3 mousePos = Mouse3D.GetMouseWorldPosition();
                    Cell cell = grid.GetGridObject(mousePos);
                    Unit targetUnit = cell.GetUnit();

                    if (targetUnit != null)
                    {
                        //Clicked on a unit, so we can attack it
                        if (targetUnit.IsEnemy(activeUnit))
                        {
                            if (activeUnit.CanAttackUnit(targetUnit))
                            {
                                if (canAttackThisTurn)
                                {
                                    canAttackThisTurn = false;

                                    state = State.Waiting;

                                    activeUnit.LookAt(targetUnit.transform.position);
                                    activeUnit.AttackUnit(targetUnit, () =>
                                    {
                                        if (debugMode)
                                            Debug.Log("Attack completed!");

                                        state = State.Normal;

                                        targetUnit.TakeDamage(activeUnit.AttackDamage);
                                        
                                        TestTurnOver();
                                    });
                                }
                            }
                            else
                            {
                                if (debugMode)
                                    Debug.Log($"Cannot attack, {targetUnit} is too far");
                                //Cannot attack enemy because reasons...
                            }
                        }
                        else
                        {
                            //No an enemy unit, so we cannot attack
                        }
                    }
                    else
                    {
                        //No Unit at the clicked position, so we can move
                        if (cell.IsValidMovePos == false || cell == grid.GetGridObject(activeUnit.transform.position))
                        {
                            if (debugMode)
                                Debug.Log($"Cell is valid: {cell.IsValidMovePos} | Unit is on cell: {cell == grid.GetGridObject(activeUnit.transform.position)}");
                            return;
                        }

                        if (canMoveThisTurn)
                        {
                            canMoveThisTurn = false;

                            state = State.Waiting;

                            activeUnit.MoveTo(mousePos, () =>
                            {
                                state = State.Normal;

                                currentCell.ClearUnit(); //Remove unit from current cell
                                cell.SetUnit(activeUnit); //Set unit to new cell

                                UpdateValidMovePositions(); //Update valid move positions for the new unit position

                                TestTurnOver();
                                if (debugMode)
                                    Debug.Log("Unit reached the destination.");
                            });
                        }
                        else
                        {
                            if (debugMode)
                            {
                                //Cannot move because already moved this turn
                                Debug.Log("Cannot move, already moved this turn");
                            }
                        }   
                    } 
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    if (debugMode)
                        Debug.Log("Ending turn");
                    ForceTurnOver();
                }
                break;
            case State.Waiting:
                break;
            default:
                break;
        }
    }

    public static void Unsubscribe(Unit unit)
    {
        if (i == null || i.units == null)
            return;

        i.grid.GetGridObject(unit.transform.position).ClearUnit();

        if (i.units.Contains(unit))
            i.units.Remove(unit);

        if (i.redTeam.Contains(unit)) i.redTeam.Remove(unit);
        else if (i.blueTeam.Contains(unit)) i.blueTeam.Remove(unit);

        if (i.debugMode)
            Debug.Log($"{unit.name} has unsubscried");
    }

    private void ForceTurnOver()
    {
        if ((!canMoveThisTurn || !canAttackThisTurn) && activeUnit != null && activeUnit.GetTeam() == Team.Blue)
        {
            CurrentActionPoints--;

            if (debugMode)
                Debug.Log($"Blue team used an actionpoint");

            CheckEscapeCondition();

            if (CurrentActionPoints <= 0)
            {
                if (debugMode)
                    Debug.Log("No action points left, ending turn");

                GameManager.i.ResetBlueUnitsToSpawn(blueTeam);
                CurrentActionPoints = MaxActionPoints; 
                SelectNextActiveUnit();
                return;
            }
        }

        SelectNextActiveUnit();
    }

    private void TestTurnOver()
    {
        if (!canAttackThisTurn && !canMoveThisTurn)
        {
            if (debugMode)
                Debug.Log("Turn over, selecting next unit");
            
            if (activeUnit != null && activeUnit.GetTeam() == Team.Blue)
            {
                CurrentActionPoints--;

                if (debugMode)
                    Debug.Log($"Blue team used an actionpoint");

                CheckEscapeCondition();
                
                if (CurrentActionPoints <= 0)
                {
                    if (debugMode)
                        Debug.Log("No action points left, ending turn");

                    //TODO: Reset team blue positions
                    GameManager.i.ResetBlueUnitsToSpawn(blueTeam);
                    SelectNextActiveUnit();
                    return;
                }
            }

            SelectNextActiveUnit();
        }
        else
        {
            CheckEscapeCondition();

            if (debugMode)
                Debug.Log("Cannot end turn, still have actions left");
        }
    }


    private void CheckEscapeCondition()
    {
        foreach (Unit unit in blueTeam)
        {
            if (!unit.gameObject.activeSelf || unit == null)
                continue;
            Cell cell = grid.GetGridObject(unit.transform.position);
            if (cell != null && cell == GameManager.i.GetEscapeCell())
            {
                EndGame(true);
                return;
            }
        }
        if (blueTeam.Count == 0)
        {
            EndGame(false);
        }
    }

    public void SelectNextActiveUnit()
    {
        blueTeam.RemoveAll(unit => unit == null || !unit.gameObject.activeSelf);
        redTeam.RemoveAll(unit => unit == null || !unit.gameObject.activeSelf);

        // No units left at all? Just return
        if (blueTeam.Count == 0 && redTeam.Count == 0)
        {
            if (debugMode)
                Debug.LogWarning("No units left in either team.");
            return;
        }

        if (blueTeam.Count == 0)
        {
            // Only red team left
            SetActiveUnit(GetNextActiveUnit(redTeam));
        }
        else if (redTeam.Count == 0)
        {
            // Only blue team left
            SetActiveUnit(GetNextActiveUnit(blueTeam));
        }
        else
        {
            // Alternate between blue and red units
            if (activeUnit == null || activeUnit.GetTeam() == Team.Red)
            {
                SetActiveUnit(GetNextActiveUnit(blueTeam));
            }
            else
            {
                SetActiveUnit(GetNextActiveUnit(redTeam));
            }
        }

        canMoveThisTurn = true;
        canAttackThisTurn = true;
    }

    public Unit GetNextActiveUnit(List<Unit> team)
    {
        if (team == blueTeam)
        {
            blueTeamActiveUnitIndex = (blueTeamActiveUnitIndex + 1) % blueTeam.Count;
            return blueTeam[blueTeamActiveUnitIndex];
        }
        else if (team == redTeam)
        {
            redTeamActiveUnitIndex = (redTeamActiveUnitIndex + 1) % redTeam.Count;
            return redTeam[redTeamActiveUnitIndex];
        }
        else
        {
            Debug.LogError(GetNextActiveUnit(team) + " is not a valid team");
            return null; 
        }
    }

    public void CheckTeamDefeat()
    {
        blueTeam.RemoveAll(unit => unit == null || !unit.gameObject.activeSelf);
        redTeam.RemoveAll(unit => unit == null || !unit.gameObject.activeSelf);

        if (blueTeam.Count == 0)
        {
            if (debugMode)
                Debug.Log("Game Over! You lost.");
            EndGame(false);
        }
        else if (redTeam.Count == 0)
            if (debugMode)
                Debug.Log("Victory! All enemies defeated.");

    }

    private void EndGame(bool playerWon)
    {
        state = State.Waiting;
        
        ClearStage();


        if (playerWon)
        {
            UIManager.i.OpenMenu("Win", (menu) => { 
                menu.SetRoundText(true); 
                menu.SetHighscoreText();
            });

        }
        else
        {
            UIManager.i.OpenMenu("Lose", (menu) => { 
                menu.SetRoundText(false);
                menu.SetHighscoreText();
            });
        }
    }

    private void ClearStage()
    {
        activeUnit = null;
        units.Clear();
        blueTeam.Clear();
        redTeam.Clear();
        blueTeamActiveUnitIndex = -1;
        redTeamActiveUnitIndex = -1;
        WorldBuilder.i.DisableAllSelectors();
        GameManager.i.RemoveAllUnits();
    }

    private void UpdateValidMovePositions()
    {
        if (grid == null || activeUnit == null)
            return; 

        WorldBuilder.i.DisableAllSelectors();
        WorldBuilder.i.SetAllValidMovePositions(false);
        
        List<Cell> cellsToCheck= new List<Cell>();

        grid.GetXY(activeUnit.transform.position, out int unitX, out int unitY);
        
        for (int x = unitX - gameSettings.maxMoveDistance; x <= unitX + gameSettings.maxMoveDistance; x++)
        {
            for (int y = unitY - gameSettings.maxMoveDistance; y <= unitY + gameSettings.maxMoveDistance; y++)
            {
                Cell cell = grid.GetGridObject(x, y);

                if (cell == null)
                    continue;

                if (cell.IsWalkable)
                {
                    cell.IsValidMovePos = true;
                    cellsToCheck.Add(cell);
                }
                else
                    cell.IsValidMovePos = false;
            }
        }

        for (int x = unitX - gameSettings.maxMoveDistance; x <= unitX + gameSettings.maxMoveDistance; x++)
        {
            for (int y = unitY - gameSettings.maxMoveDistance; y <= unitY + gameSettings.maxMoveDistance; y++)
            {
                Cell cell = grid.GetGridObject(x, y);

                if (cell == null || !cell.IsValidMovePos)
                    continue;

                List<PathNode<Cell>> path = Pathfinder.FindPath(grid.GetGridObject(unitX, unitY), cell);
                cell.IsValidMovePos = path != null && path.Count <= gameSettings.maxMoveDistance + 1;

                cell.ShowSelector(cell.IsValidMovePos);
            }
        }
    }
}
