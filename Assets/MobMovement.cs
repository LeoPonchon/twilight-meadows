using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobMovement : MonoBehaviour
{
    public float detectionRadius = 1f;
    public float moveSpeed = 1f;
    public float wanderRadius = 3f;
    public float timeUntilNewWanderTarget = 2f;
    public float stopDuration = 1f;
    public float wanderTime = 3f;

    [SerializeField]
    private GameObject player;

    private Vector3 wanderTarget;
    private float timer;
    private Animator animator;

    private float stopTimer;
    private bool isMoving = true;

    void Start()
    {
        timer = timeUntilNewWanderTarget;
        stopTimer = wanderTime;
        SetRandomWanderTarget();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer < detectionRadius)
        {
            ChasePlayer();
        }
        else
        {
            Wander();
        }

        if (isMoving)
        {
            MoveTowards(wanderTarget);
        }
        else
        {
            stopTimer -= Time.deltaTime;
            animator.SetBool("isMoving", false);
            if (stopTimer <= 0)
            {
                isMoving = true;
                stopTimer = wanderTime;
            }
        }
    }

    void SetRandomWanderTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle * wanderRadius;
        wanderTarget = new Vector3(randomDirection.x, randomDirection.y, 0) + transform.position;
    }

    void Wander()
    {
        timer += Time.deltaTime;
        if (timer >= timeUntilNewWanderTarget)
        {
            SetRandomWanderTarget();
            timer = 0;
        }

        if (isMoving)
        {
            stopTimer -= Time.deltaTime;
            if (stopTimer <= 0)
            {
                isMoving = false;
                stopTimer = stopDuration;
            }
        }
    }

    void ChasePlayer()
    {
        MoveTowards(player.transform.position);
    }

    void MoveTowards(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        float speed = direction.magnitude * moveSpeed;

        animator.SetBool("isMoving", speed > 0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
