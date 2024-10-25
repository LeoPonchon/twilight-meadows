using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float detectionRadius = 1f;
    public float moveSpeed = 1f;
    public float wanderRadius = 3f;
    public float stopDuration = 1f;

    [Header("Wandering Settings")]
    public float timeUntilNewWanderTarget = 2f;
    public float wanderTime = 3f;

    [SerializeField]
    private GameObject player;

    private Vector3 wanderTarget;
    private float wanderTimer;
    private float stopTimer;
    private bool isMoving = true;

    private Animator animator;

    void Start()
    {
        wanderTimer = timeUntilNewWanderTarget;
        stopTimer = wanderTime;
        SetRandomWanderTarget();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (IsPlayerInDetectionRange())
        {
            ChasePlayer();
        }
        else
        {
            HandleWandering();
        }
    }

    bool IsPlayerInDetectionRange()
    {
        return Vector3.Distance(transform.position, player.transform.position) < detectionRadius;
    }

    void HandleWandering()
    {
        wanderTimer += Time.deltaTime;
        if (wanderTimer >= timeUntilNewWanderTarget)
        {
            SetRandomWanderTarget();
            wanderTimer = 0;
        }

        if (isMoving)
        {
            MoveTowards(wanderTarget);
            CheckForStop();
        }
        else
        {
            StopTemporarily();
        }
    }

    void CheckForStop()
    {
        stopTimer -= Time.deltaTime;
        if (stopTimer <= 0)
        {
            isMoving = false;
            stopTimer = stopDuration;
        }
    }

    void StopTemporarily()
    {
        stopTimer -= Time.deltaTime;
        animator.SetBool("isMoving", false);

        if (stopTimer <= 0)
        {
            isMoving = true;
            stopTimer = wanderTime;
        }
    }

    void SetRandomWanderTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
        wanderTarget = new Vector3(randomDirection.x, randomDirection.y, 0) + transform.position;
    }

    void ChasePlayer()
    {
        MoveTowards(player.transform.position);
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        animator.SetBool("isMoving", direction.sqrMagnitude > 0);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
