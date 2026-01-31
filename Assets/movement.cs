using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Current Mask")]
    public MaskType currentMask;
    public bool hasMask = false;

    [Header("Movement & Shooting")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f; // Increased slightly because higher gravity needs more push
    public GameObject[] projectilePrefabs; // 0: Red, 1: Blue, 2: Yellow
    public Transform shootPoint;

    [Header("Gravity & Physics")]
    public float fallMultiplier = 3.5f;      // How fast you fall normally
    public float lowJumpMultiplier = 2.5f;   // For short hops (releasing Space early)
    private Rigidbody2D rb;

    [Header("Mask Stats")]
    private int jumpsLeft;
    private int maxJumps = 1;
    private int dashesLeft;
    private int maxDashes = 1;
    private bool canWallClimb = false;

    [Header("Ground Check")]
    private bool isGrounded;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        // Horizontal Movement
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Flip Sprite Direction
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);

        // --- JUMP LOGIC ---
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || jumpsLeft > 0))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpsLeft--;
        }

        // --- SHOOT LOGIC ---
        if (Input.GetMouseButtonDown(0) && hasMask)
        {
            Shoot();
        }

        // --- WALL CLIMB (Yellow Mask) ---
        if (canWallClimb && IsTouchingWall() && !isGrounded)
        {
            // Slide down the wall slowly
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -2f, float.MaxValue));
        }

        BetterGravity();
        CheckGround();

        if (isGrounded) jumpsLeft = maxJumps;
    }

    // This handles the "Stronger Gravity" feel
    void BetterGravity()
    {
        if (rb.linearVelocity.y < 0) // We are falling
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space)) // Short hop
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    public void EquipMask(MaskType newMask)
    {
        currentMask = newMask;
        hasMask = true;

        maxJumps = 1;
        maxDashes = 1;
        canWallClimb = false;

        if (newMask == MaskType.Red) maxJumps = 2;
        if (newMask == MaskType.Blue) maxDashes = 2;
        if (newMask == MaskType.Yellow) canWallClimb = true;

        Debug.Log("Equipped " + newMask + " Mask!");
    }

    void Shoot()
    {
        int index = (int)currentMask;
        if (projectilePrefabs.Length > index && projectilePrefabs[index] != null)
        {
            Instantiate(projectilePrefabs[index], shootPoint.position, transform.rotation);

            if (currentMask == MaskType.Yellow) // Triple shot
            {
                Instantiate(projectilePrefabs[index], shootPoint.position, transform.rotation * Quaternion.Euler(0, 0, 15));
                Instantiate(projectilePrefabs[index], shootPoint.position, transform.rotation * Quaternion.Euler(0, 0, -15));
            }
        }
    }

    bool IsTouchingWall()
    {
        return Physics2D.Raycast(transform.position, Vector2.right * transform.localScale.x, 0.7f, groundLayer);
    }

    void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
}