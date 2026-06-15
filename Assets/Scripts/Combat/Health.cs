using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Check for PlayerCombat script instead of a tag to guarantee we correctly identify the player
        if (GetComponent<PlayerCombat>() != null)
        {
            Debug.Log("PLAYER DIED! Restarting Scene...");
            // Restart the active scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Debug.Log($"{gameObject.name} DIED!");
            // It's an enemy, destroy it
            Destroy(gameObject);
        }
    }
}
