using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class TopDownMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float walkMultiplier = 0.5f;

    [Header("Animator Overrides")]
    [SerializeField] private AnimatorOverrideController animatorOverrideController;
    [SerializeField] private AnimationClip idleUpClip;
    [SerializeField] private AnimationClip idleDownClip;
    [SerializeField] private AnimationClip idleLeftClip;
    [SerializeField] private AnimationClip idleRightClip;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput playerInput;
    private SpriteRenderer playerSprite;

    private Vector2 movementInput;
    private float currentSpeed;


    [Header("Object Detection")]
    [SerializeField] private LayerMask interactableLayer; // Définir le layer des objets pertinents.
    private GameObject nearestObject; // L'objet le plus proche.

    [SerializeField] private Vector2 capsuleSize = new Vector2(1f, 2f); // Taille de la capsule (largeur, hauteur).

    private GameObject FindNearestObject()
    {
        Collider2D[] colliders = Physics2D.OverlapCapsuleAll(
            transform.position, // Centre de la capsule
            capsuleSize,        // Taille de la capsule
            CapsuleDirection2D.Vertical, // Orientation de la capsule
            0,                  // Angle de rotation
            interactableLayer   // Layer des objets à détecter
        );

        if (colliders.Length == 0) return null;

        GameObject closestObject = null;
        float minDistance = Mathf.Infinity;

        foreach (var collider in colliders)
        {
            if (collider.gameObject == gameObject)
                continue;

            float distance = Vector2.Distance(transform.position, collider.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestObject = collider.gameObject;
            }
        }

        return closestObject;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        // Calcul de la position et dessin de la capsule
        Vector3 capsuleCenter = transform.position;
        Gizmos.DrawWireCube(capsuleCenter, capsuleSize);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        playerSprite = GetComponent<SpriteRenderer>();

        currentSpeed = speed;

        if (animatorOverrideController != null)
        {
            animator.runtimeAnimatorController = animatorOverrideController;
        }
    }

    private void OnEnable()
    {
        playerInput.actions["Move"].performed += OnMoveInput;
        playerInput.actions["Move"].canceled += OnMoveInput;
        playerInput.actions["Sprint"].performed += OnSprintInput;
        playerInput.actions["Sprint"].canceled += OnSprintInput;
        playerInput.actions["Walk"].performed += OnWalkInput;
        playerInput.actions["Walk"].canceled += OnWalkInput;
    }

    private void OnDisable()
    {
        playerInput.actions["Move"].performed -= OnMoveInput;
        playerInput.actions["Move"].canceled -= OnMoveInput;
        playerInput.actions["Sprint"].performed -= OnSprintInput;
        playerInput.actions["Sprint"].canceled -= OnSprintInput;
        playerInput.actions["Walk"].performed -= OnWalkInput;
        playerInput.actions["Walk"].canceled -= OnWalkInput;
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>().normalized;
        UpdateAnimation();
    }

    private void OnSprintInput(InputAction.CallbackContext context)
    {
        currentSpeed = context.performed ? speed * sprintMultiplier : speed;
        animator.speed = context.performed ? 2f : 1f;
    }

    private void OnWalkInput(InputAction.CallbackContext context)
    {
        currentSpeed = context.performed ? speed * walkMultiplier : speed;
        animator.speed = context.performed ? .5f : 1f;
    }

    private void UpdateAnimation()
    {
        animator.SetFloat("moveX", movementInput.x);
        animator.SetFloat("moveY", movementInput.y);

        if (movementInput.y > 0)
        {
            animatorOverrideController["idle_player"] = idleUpClip;
        }
        else if (movementInput.y < 0)
        {
            animatorOverrideController["idle_player"] = idleDownClip;
        }
        else if (movementInput.x > 0)
        {
            animatorOverrideController["idle_player"] = idleRightClip;
        }
        else if (movementInput.x < 0)
        {
            animatorOverrideController["idle_player"] = idleLeftClip;
        }
    }

    private void Update()
    {
        nearestObject = FindNearestObject();
        if (nearestObject)
        {
            Debug.Log(nearestObject.name);
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = nearestObject.GetComponent<TilemapRenderer>().sortingOrder;
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movementInput * currentSpeed * Time.fixedDeltaTime);
    }
}
