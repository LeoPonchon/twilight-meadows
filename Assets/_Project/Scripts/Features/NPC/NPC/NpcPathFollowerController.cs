using System.Collections.Generic;
using UnityEngine;

public class NpcPathFollowerController : MonoBehaviour
{
    [System.Serializable]
    public sealed class Waypoint
    {
        public Transform targetTransform;
        public float stopTimeSeconds;

        public Vector2 Position => targetTransform != null ? targetTransform.position : Vector2.zero;
    }

    [Header("Path")]
    [SerializeField] private List<Waypoint> waypoints = new List<Waypoint>();
    [SerializeField] private bool loop = true;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float reachDistance = 0.05f;

    private Rigidbody2D rb;
    private Animator animator;
    private NPCController dialogue;
    private int index;
    private float waitTimer;
    private Vector2 lastDirection = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        dialogue = GetComponent<NPCController>();

        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (rb == null || waypoints == null || waypoints.Count == 0 || IsPaused())
        {
            SetMoving(false);
            return;
        }

        var waypoint = waypoints[index];
        if (waypoint == null || waypoint.targetTransform == null) return;

        Vector2 toTarget = waypoint.Position - rb.position;
        if (toTarget.magnitude <= reachDistance)
        {
            WaitThenAdvance(waypoint.stopTimeSeconds);
            return;
        }

        Vector2 direction = toTarget.normalized;
        lastDirection = direction;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
        SetMoving(true);
    }

    public void SetWaypoints(List<Waypoint> newWaypoints, bool shouldLoop)
    {
        waypoints = newWaypoints ?? new List<Waypoint>();
        loop = shouldLoop;
        index = 0;
        waitTimer = 0f;
    }

    private bool IsPaused() => dialogue != null && dialogue.IsDialogueOpen;

    private void WaitThenAdvance(float waitSeconds)
    {
        if (waitTimer <= 0f) waitTimer = waitSeconds;
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            SetMoving(false);
            return;
        }

        index++;
        if (index >= waypoints.Count) index = loop ? 0 : waypoints.Count - 1;
    }

    private void SetMoving(bool moving)
    {
        if (animator == null) return;
        animator.SetFloat("moveX", lastDirection.x);
        animator.SetFloat("moveY", lastDirection.y);

        foreach (var parameter in animator.parameters)
        {
            if (parameter.type != AnimatorControllerParameterType.Bool) continue;
            if (parameter.name != "isMoving" && parameter.name != "moving") continue;
            animator.SetBool(parameter.name, moving);
            return;
        }
    }
}
