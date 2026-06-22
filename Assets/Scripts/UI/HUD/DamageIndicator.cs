using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FPSGame.Core;
using FPSGame.Combat;

namespace FPSGame.UI
{
    /// <summary>
    /// Displays red directional chevrons on screen edges indicating where damage came from.
    /// Each indicator fades out over time. Supports multiple simultaneous indicators.
    /// 
    /// Subscribes to the player's Health.OnDamagedFromDirection event.
    /// UI indicator images are assigned via the Inspector from the prefab.
    /// </summary>
    public class DamageIndicator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float displayDuration = 1.0f;
        [SerializeField] private float fadeSpeed = 1.0f;

        [Header("Indicator Prefab")]
        [Tooltip("A UI Image placed on this Canvas that will be duplicated for each damage event.")]
        [SerializeField] private Image indicatorTemplate;

        private Health playerHealth;
        private Transform playerTransform;
        private readonly List<IndicatorInstance> activeIndicators = new List<IndicatorInstance>();

        private class IndicatorInstance
        {
            public Image image;
            public Vector3 sourceWorldPosition;
            public float timeRemaining;
        }

        private void Start()
        {
            FindPlayer();

            // Hide the template
            if (indicatorTemplate != null)
            {
                indicatorTemplate.gameObject.SetActive(false);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from health events
            if (playerHealth != null)
            {
                playerHealth.OnDamagedFromDirection -= OnDamageReceived;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void FindPlayer()
        {
            PlayerIdentifier player = FindFirstObjectByType<PlayerIdentifier>();
            if (player != null)
            {
                playerTransform = player.transform;
                playerHealth = player.GetComponent<Health>();

                if (playerHealth != null)
                {
                    playerHealth.OnDamagedFromDirection += OnDamageReceived;
                }
            }
        }

        private void OnDamageReceived(Vector3 sourcePosition)
        {
            if (indicatorTemplate == null || playerTransform == null) return;

            // Create a new indicator
            Image newIndicator = Instantiate(indicatorTemplate, indicatorTemplate.transform.parent);
            newIndicator.gameObject.SetActive(true);

            var instance = new IndicatorInstance
            {
                image = newIndicator,
                sourceWorldPosition = sourcePosition,
                timeRemaining = displayDuration
            };

            activeIndicators.Add(instance);
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                return;
            }

            // Update all active indicators
            for (int i = activeIndicators.Count - 1; i >= 0; i--)
            {
                var indicator = activeIndicators[i];
                indicator.timeRemaining -= Time.deltaTime;

                if (indicator.timeRemaining <= 0f)
                {
                    // Remove expired indicator
                    if (indicator.image != null)
                    {
                        Destroy(indicator.image.gameObject);
                    }
                    activeIndicators.RemoveAt(i);
                    continue;
                }

                // Calculate angle from player to damage source
                Vector3 dirToSource = indicator.sourceWorldPosition - playerTransform.position;
                dirToSource.y = 0f; // Flatten to horizontal plane

                // Calculate angle relative to player's forward direction
                float angle = Vector3.SignedAngle(playerTransform.forward, dirToSource, Vector3.up);

                // Position the indicator image and rotate it
                if (indicator.image != null)
                {
                    RectTransform rt = indicator.image.rectTransform;
                    rt.localRotation = Quaternion.Euler(0, 0, -angle);

                    // Fade out
                    float alpha = Mathf.Clamp01(indicator.timeRemaining / displayDuration);
                    Color c = indicator.image.color;
                    c.a = alpha;
                    indicator.image.color = c;
                }
            }
        }

        private void OnGameStateChanged(GameManager.GameState newState)
        {
            gameObject.SetActive(newState == GameManager.GameState.Playing);
        }
    }
}
