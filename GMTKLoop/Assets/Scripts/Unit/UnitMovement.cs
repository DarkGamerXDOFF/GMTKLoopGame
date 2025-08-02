using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UnitMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10;
    [SerializeField] private float stopDistance = 0.05f;

    private Queue<Vector3> pathPoints = new Queue<Vector3>();
    
    private bool isMoving = false;
    private Action onDestinationReached;

    private Rigidbody rb;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void SetPath(List<PathNode<Cell>> path, Grid<Cell> grid, Action onReached = null)
    {
        pathPoints.Clear();
        onDestinationReached = onReached;

        foreach (var node in path)
        {
            grid.GetObjectGridPosition(node.GetNodeObject(), out int x, out int y);
            Vector3 worldPos = grid.GetWorldPositionCellCenter(x, y);
            pathPoints.Enqueue(worldPos);
        }

        if (!isMoving && pathPoints.Count > 0)
            StartCoroutine(FollowPath());
    }

    private IEnumerator FollowPath()
    {
        isMoving = true;
        
        animator.SetBool("Run", true);

        while (pathPoints.Count > 0)
        {
            Vector3 target = pathPoints.Dequeue();
            target.y = transform.position.y;

            while (Vector3.Distance(transform.position, target) > stopDistance)
            {
                Vector3 direction = (target - transform.position).normalized;
                
                Vector3 localDirection = transform.InverseTransformDirection(direction);
                animator.SetFloat("MoveX", localDirection.z); // Forward
                animator.SetFloat("MoveZ", localDirection.x); // Right

                // Smooth rotation using Rigidbody
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    Quaternion smoothedRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * 100 * Time.deltaTime);
                    rb.MoveRotation(smoothedRotation);
                }

                Vector3 newPos = rb.position + direction * moveSpeed * Time.deltaTime;
                rb.MovePosition(newPos);

                yield return null;
            }
        }

        animator.SetBool("Run", false);
        animator.SetFloat("MoveX", 0f);
        animator.SetFloat("MoveZ", 0f);

        isMoving = false;
        onDestinationReached?.Invoke();
    }
}
