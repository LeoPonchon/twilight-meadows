using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobMovement : MonoBehaviour
{
    public float detectionRadius = 1f;
    public float moveSpeed = 1f;
    public float wanderRadius = 3f;
    public float timeUntilNewWanderTarget = 2f;
    public float stopDuration = 1f; // Durée d'arrêt
    public float wanderTime = 3f; // Temps de marche avant l'arrêt

    private GameObject player;
    private Vector3 wanderTarget;
    private float timer;
    private bool isChasingPlayer = false;
    private Animator animator;

    private float stopTimer; // Timer pour contrôler l'arrêt
    private bool isMoving = true; // Indique si le monstre est en mouvement

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        timer = timeUntilNewWanderTarget;
        stopTimer = wanderTime; // Initialise le timer d'arrêt
        SetRandomWanderTarget();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        if (distanceToPlayer < detectionRadius)
        {
            isChasingPlayer = true;
            ChasePlayer();
        }
        else
        {
            isChasingPlayer = false;
            Wander();
        }

        // Gère le mouvement et l'animation
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
                // Redémarre le timer d'arrêt et définit le mouvement
                isMoving = true;
                stopTimer = wanderTime; // Réinitialise le timer d'arrêt
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

        // Décide si le monstre doit s'arrêter ou non
        if (isMoving)
        {
            stopTimer -= Time.deltaTime;
            if (stopTimer <= 0)
            {
                isMoving = false; // Le monstre s'arrête
                stopTimer = stopDuration; // Réinitialise le timer d'arrêt
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
