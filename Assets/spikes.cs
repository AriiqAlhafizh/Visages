using UnityEngine;

public class Spikes : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        // Check if the thing hitting the spikes is the player
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }
    }
}