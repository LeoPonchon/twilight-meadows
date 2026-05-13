using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float walkMultiplier = 0.5f;

    [Header("Idle Clips")]
    [SerializeField] private AnimatorOverrideController animatorOverrideController;
    [SerializeField] private AnimationClip idleUpClip;
    [SerializeField] private AnimationClip idleDownClip;
    [SerializeField] private AnimationClip idleLeftClip;
    [SerializeField] private AnimationClip idleRightClip;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerInput input;
    private PlayerSpriteManager sprites;

    private Vector2 move;
    private bool sprinting;
    private bool walking;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        input = GetComponent<PlayerInput>();
        sprites = GetComponent<PlayerSpriteManager>();

        if (animator != null && animatorOverrideController != null)
        {
            animator.runtimeAnimatorController = animatorOverrideController;
        }
    }

    private void OnEnable()
    {
        // If PlayerInput is set to Send/Broadcast Messages, Unity will call OnMove/OnSprint/OnWalk by name.
        // In that mode we must NOT also subscribe manually (would process input twice).
        if (!UsesMessageNotifications())
        {
            Bind("Move", OnMove, OnMove);
            Bind("Sprint", OnSprint, OnSprint);
            Bind("Walk", OnWalk, OnWalk);
        }
    }

    private void OnDisable()
    {
        if (!UsesMessageNotifications())
        {
            Unbind("Move", OnMove, OnMove);
            Unbind("Sprint", OnSprint, OnSprint);
            Unbind("Walk", OnWalk, OnWalk);
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        rb.MovePosition(rb.position + move * CurrentSpeed * Time.fixedDeltaTime);
    }

    private float CurrentSpeed => sprinting ? speed * sprintMultiplier : walking ? speed * walkMultiplier : speed;

    private void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>().normalized;
        UpdateAnimator();
    }

    private void OnSprint(InputAction.CallbackContext context)
    {
        sprinting = context.performed;
        if (sprinting) walking = false;
        UpdateAnimatorSpeed();
    }

    private void OnWalk(InputAction.CallbackContext context)
    {
        walking = context.performed;
        if (walking) sprinting = false;
        UpdateAnimatorSpeed();
    }

    // PlayerInput "Send Messages" / "Broadcast Messages" support
    // (method names must match the action names prefixed with "On").
    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>().normalized;
        UpdateAnimator();
    }

    public void OnSprint(InputValue value)
    {
        sprinting = value.isPressed;
        if (sprinting) walking = false;
        UpdateAnimatorSpeed();
    }

    public void OnWalk(InputValue value)
    {
        walking = value.isPressed;
        if (walking) sprinting = false;
        UpdateAnimatorSpeed();
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetFloat("moveX", move.x);
        animator.SetFloat("moveY", move.y);

        if (animatorOverrideController != null)
        {
            var idle = GetIdleClip(move);
            if (idle != null) animatorOverrideController["idle_player"] = idle;
        }

        sprites?.UpdateCustomization();
    }

    private void UpdateAnimatorSpeed()
    {
        if (animator == null) return;
        animator.speed = sprinting ? 2f : walking ? 0.5f : 1f;
    }

    private AnimationClip GetIdleClip(Vector2 direction)
    {
        if (direction.y > 0f) return idleUpClip;
        if (direction.y < 0f) return idleDownClip;
        if (direction.x > 0f) return idleRightClip;
        if (direction.x < 0f) return idleLeftClip;
        return null;
    }

    private void Bind(string actionName, System.Action<InputAction.CallbackContext> performed, System.Action<InputAction.CallbackContext> canceled)
    {
        var action = input != null ? input.actions.FindAction(actionName, false) : null;
        if (action == null) return;
        action.performed += performed;
        action.canceled += canceled;
    }

    private void Unbind(string actionName, System.Action<InputAction.CallbackContext> performed, System.Action<InputAction.CallbackContext> canceled)
    {
        var action = input != null ? input.actions.FindAction(actionName, false) : null;
        if (action == null) return;
        action.performed -= performed;
        action.canceled -= canceled;
    }

    private bool UsesMessageNotifications()
    {
        if (input == null) return false;
        return input.notificationBehavior == PlayerNotifications.SendMessages
            || input.notificationBehavior == PlayerNotifications.BroadcastMessages;
    }
}
