using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Current Mask")]
    public MaskType currentMask;
    public bool hasMask = false;

    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float acceleration = 60f;
    public float deceleration = 60f;
    public float jumpForce = 14f;
    public float wallJumpSideForce = 5f;

    [Header("Feel Settings")]
    public float coyoteTime = 0.15f;      // Grace period to jump after falling
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.15f; // Grace period for pressing jump before landing
    private float jumpBufferCounter;

    [Header("Dash Settings")]
    public float dashForce = 22f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.2f;
    private bool isDashing = false;
    private int dashesLeft;
    private int maxDashes = 1;

    [Header("Pogo Parry Settings")]
    public float pogoDashForce = 20f;
    public float pogoBounceForce = 16f;
    public float pogoTotalDuration = 0.4f;
    public float pogoParryWindow = 0.15f;
    [HideInInspector] public bool isPogoing = false;
    private bool isParryActive = false;
    private bool canPogo = true;

    [Header("Screen Shake")]
    public float shakeDuration = 0.1f;
    public float shakeMagnitude = 0.15f;

    [Header("Gravity & Physics")]
    public float fallMultiplier = 4f;
    public float lowJumpMultiplier = 3f;
    private Rigidbody2D rb;
    private Vector3 baseScale;
    private float moveInput;

    [Header("Checks")]
    public int jumpsLeft;
    private int maxJumps = 1;
    private bool canWallClimb = false;
    private bool isGrounded;
    private bool isTouchingWall;
    public Transform groundCheck;
    public Transform wallCheck;
    public float checkRadius = 0.25f;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    [Header("References")]
    public GameObject[] projectilePrefabs;
    public Transform shootPoint;
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

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.gravityScale = 1f; // Ensure default

        jumpsLeft = maxJumps;
    }

    void Update()
    {
        if (isDashing || isPogoing) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        // Direction Handling
        if (moveInput > 0) transform.localScale = new Vector3(baseScale.x, baseScale.y, baseScale.z);
        else if (moveInput < 0) transform.localScale = new Vector3(-baseScale.x, baseScale.y, baseScale.z);

        PerformChecks();

        // Coyote Time logic
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        // Jump Buffer logic
        if (Input.GetKeyDown(KeyCode.Space)) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        // --- JUMP LOGIC ---
        if (jumpBufferCounter > 0f)
        {
            if (canWallClimb && isTouchingWall && !isGrounded)
            {
                DoWallJump();
                jumpBufferCounter = 0;
            }
            else if (coyoteTimeCounter > 0f || jumpsLeft > 0)
            {
                DoJump();
                jumpBufferCounter = 0;
                coyoteTimeCounter = 0;
            }
        }

        // Abilities
        if (Input.GetKeyDown(KeyCode.C) && !isGrounded && canPogo) StartCoroutine(PogoParryAction());
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashesLeft > 0 && currentMask == MaskType.Dash) StartCoroutine(Dash());
        if (Input.GetMouseButtonDown(0) && hasMask) Shoot();

        animator.SetBool("isWalking", isGrounded && Mathf.Abs(rb.linearVelocity.x) > 0.1f);
        animator.SetBool("isFalling", !isGrounded && rb.linearVelocity.y < -0.1f);
    }

    void FixedUpdate()
    {
        // NO CLING logic: stops sticking to walls if not wearing Yellow Mask
        if (!canWallClimb && isTouchingWall && !isGrounded)
        {
            if ((transform.localScale.x > 0 && moveInput > 0) || (transform.localScale.x < 0 && moveInput < 0))
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }

        if (!isDashing && !isPogoing)
        {
            float targetSpeed = moveInput * moveSpeed;
            float speedDif = targetSpeed - rb.linearVelocity.x;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
            rb.AddForce(speedDif * accelRate * Vector2.right, ForceMode2D.Force);
        }

        if (canWallClimb && isTouchingWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2.5f);
        }

        BetterGravity();
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
        rb.linearVelocity = new Vector2(pushDir * (wallJumpSideForce * 2f), jumpForce);
        jumpsLeft = maxJumps;
        animator.SetTrigger("Jump");
        playerSFX.PlayJumpSFX();
    }

    private IEnumerator PogoParryAction()
    {
        isPogoing = true;
        canPogo = false;
        rb.gravityScale = 0f;

        float faceDir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 pogoDir = new Vector2(faceDir * 0.5f, -1f).normalized;
        rb.linearVelocity = pogoDir * pogoDashForce;

        animator.SetTrigger("Pogo");
        isParryActive = true;
        yield return new WaitForSeconds(pogoParryWindow);
        isParryActive = false;

        yield return new WaitForSeconds(pogoTotalDuration - pogoParryWindow);

        if (isPogoing)
        {
            isPogoing = false;
            rb.gravityScale = 1f; // FIXED: Returns to 1, not 4
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPogoing && (collision.CompareTag("Enemy") || collision.CompareTag("Spikes")))
        {
            if (isParryActive) ExecutePogoBounce(collision.gameObject);
            else { isPogoing = false; rb.gravityScale = 1f; } // FIXED
        }
    }

    void ExecutePogoBounce(GameObject hitTarget)
    {
        isPogoing = false;
        isParryActive = false;
        rb.gravityScale = 1f; // FIXED: Reset to default gravity

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, pogoBounceForce);

        jumpsLeft = maxJumps;
        dashesLeft = maxDashes;
        canPogo = true;

        StartCoroutine(ScreenShake());
        playerSFX.PlayJumpSFX();
        animator.SetTrigger("Jump");
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        dashesLeft--;
        playerSFX.PlayDashSFX();
        rb.gravityScale = 0f;
        float dashDir = transform.localScale.x > 0 ? 1 : -1;
        rb.linearVelocity = new Vector2(dashDir * dashForce, 0f);
        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = 1f; // FIXED
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.2f, rb.linearVelocity.y);
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
    }

    void BetterGravity()
    {
        if (isPogoing || isDashing) return;

        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    }

    public void EquipMask(MaskType newMask)
    {
        mc.SetActiveMask(newMask);
        currentMask = newMask;
        hasMask = true;
        maxJumps = (newMask == MaskType.DoubleJump) ? 2 : 1;
        maxDashes = (newMask == MaskType.Dash) ? 2 : 0;
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
        isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, wallLayer);
        if (isGrounded)
        {
            jumpsLeft = maxJumps;
            dashesLeft = maxDashes;
            canPogo = true;
        }
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
}