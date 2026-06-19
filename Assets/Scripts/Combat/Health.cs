using UnityEngine;
using UnityEngine.SceneManagement;
using FPSGame.Core.Interfaces;
using FPSGame.Core.Events;

namespace FPSGame.Combat
{
    /// <summary>
    /// Defines what happens when an object's health reaches zero.
    /// Configured per-prefab in the Inspector so Health doesn't need to know
    /// whether it belongs to a player, an enemy, or a destructible barrel.
    /// </summary>
    public enum DeathBehavior
    {
        Destroy,
        RestartScene
    }

    /// <summary>
    /// Manages hit points for any damageable object (player, enemy, barrel, etc.).
    /// Implements IDamageable so projectiles can deal damage via the interface
    /// without needing to know the concrete type.
    /// 
    /// Communication is fully decoupled:
    /// - C# event OnDamaged: for local subscribers on the same prefab (e.g., EnemyController alerting on hit).
    /// - ScriptableObject onDeathEvent: for global listeners like UI, Audio, Score managers.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        public int maxHealth = 100;
        public int currentHealth;

        [Header("Death Configuration")]
        [Tooltip("What happens when health reaches zero. Set to RestartScene for the player, Destroy for enemies.")]
        public DeathBehavior deathBehavior = DeathBehavior.Destroy;

        [Tooltip("Optional ScriptableObject event raised on death (e.g., OnPlayerDied, OnEnemyKilled).")]
        public GameEvent onDeathEvent;

        /// <summary>
        /// C# event fired every time this object takes damage.
        /// Subscribe to this from local scripts (e.g., EnemyController subscribes to trigger alert state).
        /// </summary>
        public event System.Action OnDamaged;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        /// <summary>
        /// Applies damage and fires the OnDamaged event.
        /// If health drops to zero or below, triggers the death sequence.
        /// </summary>
        /// <param name="amount">The amount of damage to apply.</param>
        public void TakeDamage(int amount)
        {
            currentHealth -= amount;

            // Notify local subscribers (e.g., EnemyController alerting on damage)
            OnDamaged?.Invoke();

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Handles the death sequence based on the configured DeathBehavior.
        /// Raises the optional ScriptableObject death event for global listeners.
        /// </summary>
        private void Die()
        {
            // Raise the global event so any listener (UI, Audio, Score) can react
            if (onDeathEvent != null)
            {
                onDeathEvent.Raise();
            }

            switch (deathBehavior)
            {
                case DeathBehavior.RestartScene:
                    Debug.Log("PLAYER DIED! Restarting Scene...");
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    break;

                case DeathBehavior.Destroy:
                default:
                    Debug.Log($"{gameObject.name} DIED!");
                    Destroy(gameObject);
                    break;
            }
        }
    }
}
