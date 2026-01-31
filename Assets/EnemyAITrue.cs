using UnityEngine;
using System.Collections;

public class EnemyAITrue : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;
    public float patrolDistance = 5f;
    public float detectionRange = 5f;

    [Header("Stomp Logic")]
    public float bounceForce = 12f;
    public Vector3 squishScale = new Vector3(1.2f, 0.2f, 1f);

    [Header("Drop Settings")]
    public GameObject maskPrefabToDrop;
    public float dropForce = 5f;

    private Vector3 startPos;
    private int direction = 1;
    private Transform player;
    private bool isDead = false;
    private Rigidbody2D rb;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer < detectionRange) ChasePlayer();
        else Patrol();
    }

    void Patrol()
    {
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        if (Vector2.Distance(startPos, transform.position) >= patrolDistance)
        {
            direction *= -1;
            FlipSprite();
            startPos = transform.position;
        }
    }

    void ChasePlayer()
    {
        float moveDir = (player.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(moveDir * chaseSpeed, rb.linearVelocity.y);
        if ((moveDir > 0 && transform.localScale.x < 0) || (moveDir < 0 && transform.localScale.x > 0)) FlipSprite();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Calculate if the player is above the enemy
            // Normal.y < -0.5 means the impact came from above
            if (collision.contacts[0].normal.y < -0.5f)
            {
                StartCoroutine(GetSquished(collision.gameObject));
            }
            else
            {
                // Side collision: Player takes damage
                collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(1);
            }
        }
    }

    // This handles the projectile hitting the enemy too!
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile"))
        {
            // You can decide if projectiles squish them or just destroy them
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }
    }

    IEnumerator GetSquished(GameObject playerObj)
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static; // Stop moving entirely
        GetComponent<Collider2D>().enabled = false;

        // Bounce the player up
        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
        }

        // ONLY squish this enemy's sprite
        transform.localScale = squishScale;

        yield return new WaitForSeconds(0.2f);

        if (maskPrefabToDrop != null)
        {
            GameObject droppedItem = Instantiate(maskPrefabToDrop, transform.position, Quaternion.identity);
            Rigidbody2D itemRb = droppedItem.GetComponent<Rigidbody2D>();
            if (itemRb != null) itemRb.AddForce(Vector2.up * dropForce, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }

    void FlipSprite()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}