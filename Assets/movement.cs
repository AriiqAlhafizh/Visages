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
    public float wallJumpSideForce = 3f; // Low value keeps you close to the wall for easier climbing
    public GameObject[] projectilePrefabs;
    public Transform shootPoint;

    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.1f;
    private bool isDashing = false;
    private int dashesLeft;
    private int maxDashes = 1;

    [Header("Pogo Parry Settings")]
    public float pogoDashForce = 18f;
    public float pogoBounceForce = 15f;
    public float pogoTotalDuration = 0.4f;
    public float pogoParryWindow = 0.15f;
    [HideInInspector] public bool isPogoing = false;
    private bool isParryActive = false;
    private bool canPogo = true;

    [Header("Screen Shake")]
    public float shakeDuration = 0.1f;
    public float shakeMagnitude = 0.2f;

    [Header("Gravity & Physics")]
    public float fallMultiplier = 3.5f;
    public float lowJumpMultiplier = 2.5f;
    private Rigidbody2D rb;
    private Vector3 baseScale;

    [Header("Checks")]
    public int jumpsLeft;
    private int maxJumps = 0;
    private bool canWallClimb = false; // Linked to Yellow Mask
    private bool isGrounded;
    private bool isFalling;
    private bool isTouchingWall;
    public Transform groundCheck;
    public Transform wallCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    [Header("References")]
    private Animator animator;
    private MaskController mc;
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
        if (isDashing || isPogoing) return;

        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (moveInput > 0) transform.localScale = new Vector3(baseScale.x, baseScale.y, baseScale.z);
        else if (moveInput < 0) transform.localScale = new Vector3(-baseScale.x, baseScale.y, baseScale.z);

        PerformChecks();

        // --- THE INFINITE CLIMB LOGIC (Yellow Mask) ---
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // PRIORITY 1: If Yellow Mask + Wall = Infinite Spammable Jump
            if (canWallClimb && isTouchingWall)
            {
                DoWallJump();
            }
            // PRIORITY 2: Normal Jump / Double Jump logic
            else if (isGrounded || jumpsLeft > 0)
            {
                DoJump();
            }
        }

        // --- DASH & POGO INPUTS ---
        if (Input.GetKeyDown(KeyCode.C) && !isGrounded && canPogo) StartCoroutine(PogoParryAction());
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashesLeft > 0 && currentMask == MaskType.Dash) StartCoroutine(Dash());

        // --- WALL SLIDE VISUAL ---
        if (canWallClimb && isTouchingWall && !isGrounded)
        {
            if (rb.linearVelocity.y < 0)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);
        }

        if (Input.GetMouseButtonDown(0) && hasMask) Shoot();

        BetterGravity();

        // Reset abilities on ground
        if (isGrounded)
        {
            jumpsLeft = maxJumps;
            dashesLeft = maxDashes;
            canPogo = true;
        }
    }

    void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpsLeft--; // This consumes a jump charge
        animator.SetTrigger("Jump");
        playerSFX.PlayJumpSFX();
    }

    void DoWallJump()
    {
        // Secret sauce: Kill velocity so every spam-jump starts fresh
        rb.linearVelocity = Vector2.zero;

        // Apply a small push away from wall and a big push UP
        float pushDir = transform.localScale.x > 0 ? -1f : 1f;
        rb.linearVelocity = new Vector2(pushDir * wallJumpSideForce, jumpForce);

        // We DON'T subtract from jumpsLeft here, making it infinite on the wall.
        // We also reset jumpsLeft so you have your double jump available after leaving the wall!
        jumpsLeft = maxJumps;

        animator.SetTrigger("Jump");
        playerSFX.PlayJumpSFX();
    }

    private IEnumerator PogoParryAction()
    {
        isPogoing = true;
        canPogo = false;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        float faceDir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 pogoDir = new Vector2(faceDir * 0.4f, -1f).normalized;
        rb.linearVelocity = pogoDir * pogoDashForce;
        animator.SetTrigger("Pogo");
        isParryActive = true;
        yield return new WaitForSeconds(pogoParryWindow);
        isParryActive = false;
        yield return new WaitForSeconds(pogoTotalDuration - pogoParryWindow);
        if (isPogoing)
        {
            isPogoing = false;
            rb.gravityScale = originalGravity;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPogoing)
        {
            bool isHazard = collision.CompareTag("Enemy") || collision.CompareTag("Spikes");
            if (isHazard)
            {
                if (isParryActive) ExecutePogoBounce(collision.gameObject);
                else { isPogoing = false; rb.gravityScale = 3f; }
            }
        }
    }

    void ExecutePogoBounce(GameObject hitTarget)
    {
        isPogoing = false;
        isParryActive = false;
        rb.gravityScale = 3f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoBounceForce);
        jumpsLeft = maxJumps;
        dashesLeft = maxDashes;
        canPogo = true;
        StartCoroutine(ScreenShake());
        playerSFX.PlayJumpSFX();
        animator.SetTrigger("Jump");
    }

    private IEnumerator ScreenShake()
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0.0f;
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;
            Camera.main.transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Camera.main.transform.localPosition = originalPos;
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        dashesLeft--;
        playerSFX.PlayDashSFX();
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        float dashDir = transform.localScale.x > 0 ? 1 : -1;
        rb.linearVelocity = new Vector2(dashDir * dashForce, 0f);
        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y);
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
    }

    void BetterGravity()
    {
        // Don't apply heavy gravity if sticking to a wall with Yellow Mask
        if (canWallClimb && isTouchingWall) return;
        if (isPogoing) return;

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

        // Settings for different masks
        maxJumps = (newMask == MaskType.DoubleJump) ? 2 : 1;
        maxDashes = (newMask == MaskType.Dash) ? 2 : 0;

        // Yellow Mask enables Wall Climb/Infinite Wall Jump
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
        animator.SetBool("isWalking", isGrounded && rb.linearVelocity.x != 0);
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, wallLayer);
    }
}