using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;
    public float invincibilityDuration = 2f;
    public float blinkInterval = 0.1f;

    [Header("UI Reference")]
    public HealthUI healthUI;

    private bool isInvincible = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (healthUI != null) healthUI.SetHealthDisplay(currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;

        // Clamp health so it doesn't go below 0
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthUI != null) healthUI.SetHealthDisplay(currentHealth);

        if (currentHealth <= 0) Die();
        else StartCoroutine(BecomeInvincible());
    }

    private IEnumerator BecomeInvincible()
    {
        isInvincible = true;
        float timer = 0;
        while (timer < invincibilityDuration)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }
        spriteRenderer.enabled = true;
        isInvincible = false;
    }

    void Die()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}