using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    private float defaultSpeed;
    private float currentSpeed;
    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        defaultSpeed = GetComponent<StatsManager>().speed;
    }

    void Update()
    {
        currentSpeed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? defaultSpeed * 2 : defaultSpeed;

        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        string direction = "idle";
        if (movement != Vector2.zero)
        {
            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
            {
                direction = movement.x > 0 ? "right" : "left";
            }
            else
            {
                direction = movement.y > 0 ? "up" : "down";
            }
        }

        switch (direction)
        {
            case "up":
                animator.Play("walking_up_player");
                break;
            case "down":
                animator.Play("walking_down_player");
                break;
            case "left":
                animator.Play("walking_horizontal_player");
                break;
            case "right":
                animator.Play("walking_horizontal_player");
                break;
            default:
                animator.Play("idle_player");
                break;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * currentSpeed * Time.fixedDeltaTime);
    }
}
