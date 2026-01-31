using UnityEngine;
using System.Collections;

public class EnemyAI1 : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 4f;
    public float patrolDistance = 5f;
    public float detectionRange = 5f;

    [Header("Stomp Logic")]
    public float bounceForce = 10f;
    public Vector3 squishScale = new Vector3(1.2f, 0.2f, 1f);

    private Vector3 startPos;
    private int direction = 1;
    private Transform player;
    private bool isDead = false;
    private Rigidbody2D rb;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isDead) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (distToPlayer < detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);

        if (Vector2.Distance(startPos, transform.position) >= patrolDistance)
        {
            direction *= -1; // Reverse
            FlipSprite();
        }
    }

    void ChasePlayer()
    {
        float moveDir = (player.position.x > transform.position.x) ? 1 : -1;
        transform.Translate(Vector2.right * moveDir * chaseSpeed * Time.deltaTime);

        // Face the player
        if ((moveDir > 0 && transform.localScale.x < 0) || (moveDir < 0 && transform.localScale.x > 0))
        {
            FlipSprite();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isDead)
        {
            // Check if player is above the enemy
            // Contact point 0 is the first point of impact
            if (collision.contacts[0].normal.y < -0.5f)
            {
                StartCoroutine(GetSquished(collision.gameObject));
            }
            else
            {
                // Deal damage to player (uses the script we made earlier)
                collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(1);
            }
        }
    }

    IEnumerator GetSquished(GameObject playerObj)
    {
        isDead = true;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true; // Stop physics
        GetComponent<Collider2D>().enabled = false;

        // Bounce the player up
        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        playerRb.velocity = new Vector2(playerRb.velocity.x, bounceForce);

        // Visual Squish
        transform.localScale = squishScale;

        yield return new WaitForSeconds(0.5f);

        // Item drop placeholder
        Debug.Log("Enemy dropped an item!");

        Destroy(gameObject);
    }

    void FlipSprite()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}