using UnityEngine;
using UnityEngine.UI;
using FPSGame.Core;
using FPSGame.Core.Interfaces;
using FPSGame.Combat;

namespace FPSGame.UI
{
    /// <summary>
    /// Main HUD controller. Displays health bar (bottom-left) and ammo counter (bottom-right).
    /// All UI elements are assigned via the Inspector from the prefab.
    /// Finds the player at runtime via PlayerIdentifier (Core).
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [Header("Health Bar")]
        [SerializeField] private Image healthBarFill;
        [SerializeField] private Text healthText;

        [Header("Ammo Counter")]
        [SerializeField] private Text ammoText;
        [SerializeField] private Text reserveAmmoText;
        [SerializeField] private Text reloadPromptText;

        [Header("References (Auto-populated at runtime)")]
        private Health playerHealth;
        private IAmmoProvider ammoProvider;

        private void Start()
        {
            FindPlayerReferences();

            // Subscribe to game state changes to show/hide HUD
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void FindPlayerReferences()
        {
            PlayerIdentifier player = FindFirstObjectByType<PlayerIdentifier>();
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
                ammoProvider = player.GetComponent<IAmmoProvider>() as IAmmoProvider;

                // Also search in children if not found on root
                if (ammoProvider == null)
                {
                    MonoBehaviour[] behaviours = player.GetComponents<MonoBehaviour>();
                    foreach (var b in behaviours)
                    {
                        if (b is IAmmoProvider provider)
                        {
                            ammoProvider = provider;
                            break;
                        }
                    }
                }
            }
        }

        private void Update()
        {
            // Try to find player if not yet found (e.g., spawned after HUD)
            if (playerHealth == null)
            {
                FindPlayerReferences();
                if (playerHealth == null) return;
            }

            UpdateHealthBar();
            UpdateAmmoCounter();
        }

        private void UpdateHealthBar()
        {
            if (playerHealth == null) return;

            float healthPercent = (float)playerHealth.currentHealth / playerHealth.maxHealth;
            healthPercent = Mathf.Clamp01(healthPercent);

            // Update fill bar
            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = healthPercent;

                // Color transitions: green -> yellow -> red
                if (healthPercent > 0.5f)
                {
                    healthBarFill.color = Color.Lerp(Color.yellow, Color.green, (healthPercent - 0.5f) * 2f);
                }
                else
                {
                    healthBarFill.color = Color.Lerp(Color.red, Color.yellow, healthPercent * 2f);
                }
            }

            // Update text
            if (healthText != null)
            {
                healthText.text = $"{Mathf.Max(0, playerHealth.currentHealth)} / {playerHealth.maxHealth}";
            }
        }

        private void UpdateAmmoCounter()
        {
            if (ammoProvider == null) return;

            // Current / Magazine
            if (ammoText != null)
            {
                ammoText.text = $"{ammoProvider.CurrentAmmo} / {ammoProvider.MaxAmmo}";

                // Flash red when empty
                if (ammoProvider.CurrentAmmo <= 0)
                {
                    ammoText.color = Color.red;
                }
                else if (ammoProvider.CurrentAmmo <= ammoProvider.MaxAmmo * 0.25f)
                {
                    ammoText.color = Color.yellow;
                }
                else
                {
                    ammoText.color = Color.white;
                }
            }

            // Reserve ammo
            if (reserveAmmoText != null)
            {
                reserveAmmoText.text = ammoProvider.TotalAmmo.ToString();
            }

            // Reload prompt
            if (reloadPromptText != null)
            {
                if (ammoProvider.IsReloading)
                {
                    reloadPromptText.text = "RELOADING...";
                    reloadPromptText.gameObject.SetActive(true);
                }
                else if (ammoProvider.CurrentAmmo <= 0 && ammoProvider.TotalAmmo > 0)
                {
                    reloadPromptText.text = "Press R to Reload";
                    reloadPromptText.gameObject.SetActive(true);
                }
                else
                {
                    reloadPromptText.gameObject.SetActive(false);
                }
            }
        }

        private void OnGameStateChanged(GameManager.GameState newState)
        {
            // HUD is only visible during gameplay
            gameObject.SetActive(newState == GameManager.GameState.Playing);
        }
    }
}
