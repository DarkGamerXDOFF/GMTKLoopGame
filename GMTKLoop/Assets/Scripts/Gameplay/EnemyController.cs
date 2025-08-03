using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyController : MonoBehaviour
{
    public static EnemyController i;

    private void Awake()
    {
        if (i == null) i = this;
        else Destroy(gameObject);
    }

    public void TakeEnemyTurn(Unit enemy)
    {
        StartCoroutine(EnemyTurnRoutine(enemy));
    }

    private IEnumerator EnemyTurnRoutine(Unit enemy)
    {
        yield return new WaitForSeconds(0.5f);

        Unit closestTarget = FindClosestBlueUnit(enemy);
        if (closestTarget == null)
        {
            Debug.Log("No target found, ending enemy turn.");
            GridCombatSystem.i.ForceTurnOver();
            yield break;
        }

        // Attempt attack first
        if (enemy.CanAttackUnit(closestTarget))
        {
            yield return PerformAttack(enemy, closestTarget);
            yield break;
        }

        // If no attack possible, try moving
        Vector3? bestMove = FindBestMovePosition(enemy, closestTarget);
        if (!bestMove.HasValue)
        {
            Debug.Log("No valid move, ending enemy turn.");
            GridCombatSystem.i.ForceTurnOver();
            yield break;
        }

        // Move to best position
        bool moveDone = false;
        Cell currentCell = WorldBuilder.GetGrid().GetGridObject(enemy.transform.position);
        Cell targetCell = WorldBuilder.GetGrid().GetGridObject(bestMove.Value);

        GridCombatSystem.i.SetStateWaiting();

        enemy.MoveTo(bestMove.Value, () =>
        {
            currentCell.ClearUnit();
            targetCell.SetUnit(enemy);
            GridCombatSystem.i.UpdateValidMovePositions();
            moveDone = true;
        });

        yield return new WaitUntil(() => moveDone);
        yield return new WaitForSeconds(0.2f);

        Unit newTarget = FindClosestBlueUnit(enemy);

        if (newTarget != null && enemy.CanAttackUnit(newTarget))
        {
            yield return PerformAttack(enemy, newTarget);
        }
        else
        {
            Debug.Log("Enemy couldn't attack after move. Ending turn.");
            GridCombatSystem.i.ResetEnemyTurn();
            GridCombatSystem.i.ForceTurnOver();
        }
    }

    private IEnumerator PerformAttack(Unit attacker, Unit target)
    {
        GridCombatSystem.i.SetStateWaiting();

        bool attackDone = false;

        attacker.AttackUnit(target, () =>
        {
            target.TakeDamage(attacker.AttackDamage);
            attackDone = true;
        });

        yield return new WaitUntil(() => attackDone);
        yield return new WaitForSeconds(0.2f);

        GridCombatSystem.i.ResetEnemyTurn();
        GridCombatSystem.i.ForceTurnOver(); //Make sure turn ends
    }

    private IEnumerator Attack(Unit attacker, Unit target)
    {
        Debug.Log($"Enemy {attacker.name} attacking {target.name}");
        GridCombatSystem.i.SetStateWaiting();

        attacker.AttackUnit(target, () =>
        {
            target.TakeDamage(attacker.AttackDamage);
            GridCombatSystem.i.ResetEnemyTurn();
            GridCombatSystem.i.ForceTurnOver();
        });

        yield return null;
    }

    private IEnumerator HandlePostMoveAttack(Unit enemy)
    {
        yield return new WaitForSeconds(0.2f); // small delay for clarity

        Unit target = FindClosestBlueUnit(enemy);
        if (target != null && enemy.CanAttackUnit(target))
        {
            Debug.Log($"After moving, {enemy.name} attacking {target.name}");
            GridCombatSystem.i.SetStateWaiting();

            bool attackDone = false;
            enemy.AttackUnit(target, () =>
            {
                target.TakeDamage(enemy.AttackDamage);
                attackDone = true;
            });

            // Wait until attack finishes
            yield return new WaitUntil(() => attackDone);

            GridCombatSystem.i.ResetEnemyTurn();
        }
        else
        {
            GridCombatSystem.i.ResetEnemyTurn();
        }
    }

    private Unit FindClosestBlueUnit(Unit enemy)
    {
        List<Unit> blueUnits = GridCombatSystem.i.GetBlueTeam();
        Unit closest = null;
        float closestDist = float.MaxValue;

        foreach (var unit in blueUnits)
        {
            if (!unit.gameObject.activeSelf) continue;

            float dist = Vector3.Distance(unit.transform.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = unit;
            }
        }

        return closest;
    }

    private Vector3? FindBestMovePosition(Unit enemy, Unit target)
    {
        Grid<Cell> grid = WorldBuilder.GetGrid();
        List<Cell> candidates = new List<Cell>();

        grid.GetXY(enemy.transform.position, out int unitX, out int unitY);
        int range = GridCombatSystem.i.gameSettings.maxMoveDistance;

        for (int x = unitX - range; x <= unitX + range; x++)
        {
            for (int y = unitY - range; y <= unitY + range; y++)
            {
                Cell cell = grid.GetGridObject(x, y);
                if (cell == null || !cell.IsWalkable || cell.GetUnit() != null)
                    continue;

                List<PathNode<Cell>> path = Pathfinder.FindPath(
                    grid.GetGridObject(enemy.transform.position),
                    cell
                );

                if (path != null && path.Count <= range + 1)
                {
                    candidates.Add(cell);
                }
            }
        }

        if (candidates.Count == 0)
            return null;

        candidates.Sort((a, b) =>
        {
            a.GetGrid().GetObjectGridPosition(a, out int ax, out int ay);
            b.GetGrid().GetObjectGridPosition(b, out int bx, out int by);

            Vector3 aWorld = a.GetGrid().GetWorldPositionCellCenter(ax, ay);
            Vector3 bWorld = b.GetGrid().GetWorldPositionCellCenter(bx, by);

            float distA = Vector3.Distance(aWorld, target.transform.position);
            float distB = Vector3.Distance(bWorld, target.transform.position);

            return distA.CompareTo(distB);
        });

        Cell bestCell = candidates[0];
        bestCell.GetGrid().GetObjectGridPosition(bestCell, out int cx, out int cy);
        return bestCell.GetGrid().GetWorldPositionCellCenter(cx, cy);
    }
}

