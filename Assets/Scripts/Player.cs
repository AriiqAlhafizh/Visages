using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    float moveSpeed = 500f;
    float dashDistance = 1000f;
    float jumpForce = 10f;

    public bool canDash = true;
    public bool isDashing = false;
    public int dashCount = 0;
    public int maxDashCount = 1; // TODO: Set Max Dash Count & Max Jump Count di game manager

    public bool canJump = true;
    int jumpCount = 0;
    int maxJumpCount = 1;

    Rigidbody2D rb;
    public bool isGrounded = true;
    Vector2 moveInput;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        Move();
    }
    public void OnMove(InputAction.CallbackContext context) 
    {
        if (context.performed)
        {
            moveInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            moveInput = Vector2.zero;
        }
    }
    
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Dash();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Jump();
        }
    }

    void Move()
    {
        if (isDashing) return; // Skip normal movement during dash
        Vector2 velocity = rb.linearVelocity;
        velocity.x = moveInput.x * moveSpeed * Time.deltaTime;
        rb.linearVelocity = velocity;
    }

    void Dash()
    {
        if (!isDashing && canDash && dashCount <  maxDashCount)
        {
            StartCoroutine(DashCoroutine());
            dashCount++;
        }
    }

    IEnumerator DashCoroutine()
    {
        isDashing = true;
        canDash = false;
        rb.linearVelocityY = 0; // Reset vertical velocity during dash
        rb.gravityScale = 0; // Disable gravity during dash

        rb.linearVelocityX = moveInput.x * dashDistance * Time.deltaTime; // Example dash speed
        yield return new WaitForSeconds(0.5f);
        
        rb.gravityScale = 1; // Re-enable gravity after dash
        isDashing = false;
        canDash = true;
    }


    void Jump()
    {
        if (isGrounded && jumpCount < maxJumpCount)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;
        }
    }

    void ResetCooldown()
    {
        if (isGrounded)
        {
            dashCount = 0; // Reset dash count when grounded
            jumpCount = 0; // Reset jump count when grounded
        }
    }
}
