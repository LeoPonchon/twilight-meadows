using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    [SerializeField]
    private float normalSpeed = 5f;   // Default movement speed (adjust to suit your needs)
    private float moveSpeed;          // Speed used for actual movement, modified by input
    private Rigidbody2D rb;           // Reference to the Rigidbody2D component
    private Vector2 movement;         // Store movement direction
    private Animator animator;        // Reference to the Animator component

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();  // Initialize Animator
        moveSpeed = normalSpeed;              // Set moveSpeed to normalSpeed
    }

    void Update()
    {
        // Capture input for movement (instant response, no smoothing)
        movement.x = Input.GetAxisRaw("Horizontal");  // Raw input ensures no smooth acceleration
        movement.y = Input.GetAxisRaw("Vertical");

        // Shift key for sprinting (optional)
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            moveSpeed = normalSpeed * 2;  // Double speed when Shift is held down
        }
        else
        {
            moveSpeed = normalSpeed;      // Return to normal speed
        }

        // Update the animator based on movement direction
        if (movement.y > 0)
        {
            animator.Play("walking_up_player");    // Play walking up animation
        }
        else if (movement.y < 0)
        {
            animator.Play("walking_down_player");  // Play walking down animation
        }
        else if (movement.x != 0)
        {
            animator.Play("walking_horizontal_player");  // Play horizontal walk animation (left or right)
        }

        // If no movement, play idle animation
        if (movement == Vector2.zero)
        {
            animator.Play("idle_player");          // Play idle animation
        }
    }

    void FixedUpdate()
    {
        // Apply movement instantly at full speed, no acceleration
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
