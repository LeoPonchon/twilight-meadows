using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;

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
    }

    private void OnDisable()
    {
        playerInput.actions["Move"].performed -= OnMoveInput;
        playerInput.actions["Move"].canceled -= OnMoveInput;
        playerInput.actions["Sprint"].performed -= OnSprintInput;
        playerInput.actions["Sprint"].canceled -= OnSprintInput;
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

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movementInput * currentSpeed * Time.fixedDeltaTime);
    }
}
