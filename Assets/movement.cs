using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Current Mask")]
    public MaskType currentMask;
    public bool hasMask = false;

    [Header("Movement & Shooting")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float wallJumpSideForce = 3f;
    public GameObject[] projectilePrefabs;
    public Transform shootPoint;

    [Header("Dash Settings (Blue Mask)")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.1f;
    private bool isDashing = false;
    private int dashesLeft;
    private int maxDashes = 1;

    [Header("Gravity & Physics")]
    public float fallMultiplier = 3.5f;
    public float lowJumpMultiplier = 2.5f;
    private Rigidbody2D rb;
    private Vector3 baseScale;

    [Header("Mask Stats")]
    public int jumpsLeft;
    private int maxJumps = 1;
    private bool canWallClimb = false;

    [Header("Checks")]
    private bool isGrounded;
    private bool isFalling;
    private bool isTouchingWall;
    public Transform groundCheck;
    public Transform wallCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    [Header("Animation")]
    private Animator animator;
    private MaskController mc;

    [Header("Sounds")]
    private PlayerSFX playerSFX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        mc = GetComponent<MaskController>();
        playerSFX = GetComponent<PlayerSFX>();
        baseScale = transform.localScale;
    }

    void Update()
    {
        // Prevent normal movement/jumping while dashing
        if (isDashing) return;

        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput > 0) transform.localScale = new Vector3(baseScale.x, baseScale.y, baseScale.z);
        else if (moveInput < 0) transform.localScale = new Vector3(-baseScale.x, baseScale.y, baseScale.z);

        PerformChecks();

        // --- DASH INPUT (Blue Mask) ---
        // Triggered by Left Shift (standard) or your choice of key
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashesLeft > 0 && currentMask == MaskType.Dash)
        {
            StartCoroutine(Dash());
        }

        // --- JUMP LOGIC ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canWallClimb && isTouchingWall) DoWallJump();
            else if (isGrounded || jumpsLeft > 0) DoJump();
        }

        // --- WALL SLIDE ---
        if (canWallClimb && isTouchingWall && !isGrounded)
        {
            if (rb.linearVelocity.y < 0)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
        }

        if (Input.GetMouseButtonDown(0) && hasMask) Shoot();

        BetterGravity();

        if (isGrounded)
        {
            jumpsLeft = maxJumps;
            dashesLeft = maxDashes; // Reset dashes on ground
        }
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        dashesLeft--;
        playerSFX.PlayDashSFX();

        // Store original gravity so we can turn it off
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Set dash velocity based on facing direction
        float dashDir = transform.localScale.x > 0 ? 1 : -1;
        rb.linearVelocity = new Vector2(dashDir * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        // Reset physics
        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y); // Slight slowdown
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
    }

    void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpsLeft--;
        animator.SetTrigger("Jump");
        playerSFX.PlayJumpSFX();
    }

    void DoWallJump()
    {
        rb.linearVelocity = Vector2.zero;
        float pushDir = transform.localScale.x > 0 ? -1f : 1f;
        rb.linearVelocity = new Vector2(pushDir * wallJumpSideForce, jumpForce);
        animator.SetTrigger("Jump");
        playerSFX.PlayJumpSFX();
    }

    void BetterGravity()
    {
        if (canWallClimb && isTouchingWall) return;

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            if (!isGrounded && !isFalling)
            {
                animator.SetBool("isFalling", true);
                animator.ResetTrigger("Jump");
            }
        }

        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
    }

    public void EquipMask(MaskType newMask)
    {
        mc.SetActiveMask(newMask);
        currentMask = newMask;
        hasMask = true;

        // Reset/Update stats based on mask
        maxJumps = (newMask == MaskType.DoubleJump) ? 1 : 0;
        maxDashes = (newMask == MaskType.Dash) ? 2 : 0; // Blue mask gets 2 dashes
        canWallClimb = (newMask == MaskType.WallClimb);

        jumpsLeft = maxJumps;
        dashesLeft = maxDashes;
    }

    void Shoot()
    {
        int index = (int)currentMask;
        if (projectilePrefabs.Length > index && projectilePrefabs[index] != null)
        {
            float lookDir = transform.localScale.x > 0 ? 0 : 180;
            Instantiate(projectilePrefabs[index], shootPoint.position, Quaternion.Euler(0, lookDir, 0));
            if (currentMask == MaskType.WallClimb)
            {
                Instantiate(projectilePrefabs[index], shootPoint.position, Quaternion.Euler(0, lookDir, 15));
                Instantiate(projectilePrefabs[index], shootPoint.position, Quaternion.Euler(0, lookDir, -15));
            }
        }
    }

    void PerformChecks()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        if (isGrounded) 
        {
            isFalling = false;
            animator.SetBool("isFalling", false);
        }
        if (isGrounded && rb.linearVelocityX != 0) {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, wallLayer);
    }
}