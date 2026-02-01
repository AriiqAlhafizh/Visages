using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 1;

    void Start()
    {
        Destroy(gameObject, 3f); // Cleanup after 3 seconds
    }

    void Update()
    {
        // Move forward based on the direction the projectile is facing
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we hit an enemy
        if (collision.CompareTag("Enemy"))
        {
            // If the enemy has a health script, we could deal damage here
            // For now, we'll just destroy the projectile
            Destroy(gameObject);

            // If you want the projectile to kill the enemy instantly:
            // Destroy(collision.gameObject); 
        }
    }
}