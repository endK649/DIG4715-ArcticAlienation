using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthPoints : MonoBehaviour
{
    public int maxHitpoints = 5;
    private int currentHitpoints;

    public string gameOverSceneName = "GameOverScene"; // Set this in the Inspector

    void Start()
    {
        currentHitpoints = maxHitpoints;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered!");
        if (other.CompareTag("Enemy"))
        {
            TakeDamage(1); // Adjust damage as needed
        }
    }

    void TakeDamage(int amount)
    {
        currentHitpoints -= amount;
        Debug.Log("Took damage! Current HP: " + currentHitpoints);

        if (currentHitpoints <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player died! Loading Game Over Scene...");
        SceneManager.LoadScene(gameOverSceneName);
    }
}

