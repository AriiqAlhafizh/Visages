using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [Header("Assign in order: 0, 1, 2, 3, 4, 5")]
    public GameObject[] healthStates;

    public void SetHealthDisplay(int health)
    {
        // First, hide every health object
        for (int i = 0; i < healthStates.Length; i++)
        {
            if (healthStates[i] != null)
                healthStates[i].SetActive(false);
        }

        // Then, show only the one matching the current health
        if (health >= 0 && health < healthStates.Length)
        {
            if (healthStates[health] != null)
                healthStates[health].SetActive(true);
        }
    }
}