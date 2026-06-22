using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FPSGame.Core;

namespace FPSGame.UI
{
    /// <summary>
    /// Game Over screen displayed when the player dies.
    /// Shows stats (kills, accuracy, time survived, score) and Restart/Quit buttons.
    /// All UI elements are assigned via the Inspector from the prefab.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject gameOverPanel;

        [Header("Stats Display")]
        [SerializeField] private Text killsText;
        [SerializeField] private Text accuracyText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text scoreText;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitToMenuButton;

        private void Start()
        {
            // Ensure an EventSystem exists — without it, no UI clicks work at all
            EnsureEventSystemExists();

            // Wire up button callbacks
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (quitToMenuButton != null)
                quitToMenuButton.onClick.AddListener(OnQuitToMenuClicked);

            // Start hidden
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            // Listen for game state changes
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

        private void OnGameStateChanged(GameManager.GameState newState)
        {
            if (newState == GameManager.GameState.GameOver)
            {
                ShowGameOver();
            }
            else
            {
                if (gameOverPanel != null)
                    gameOverPanel.SetActive(false);
            }
        }

        private void ShowGameOver()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            // Populate stats from GameManager
            if (GameManager.Instance != null)
            {
                if (killsText != null)
                    killsText.text = GameManager.Instance.Kills.ToString();

                if (accuracyText != null)
                    accuracyText.text = $"{GameManager.Instance.Accuracy:F1}%";

                if (timeText != null)
                {
                    float time = GameManager.Instance.TimeSurvived;
                    int minutes = (int)(time / 60f);
                    int seconds = (int)(time % 60f);
                    timeText.text = $"{minutes:D2}:{seconds:D2}";
                }

                if (scoreText != null)
                    scoreText.text = GameManager.Instance.Score.ToString();
            }
        }

        private void OnRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }

        private void OnQuitToMenuClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitToMainMenu();
            }
        }

        /// <summary>
        /// Creates an EventSystem if one doesn't exist in the scene.
        /// Without an EventSystem, Unity cannot process any UI clicks.
        /// </summary>
        private void EnsureEventSystemExists()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                Debug.Log("[GameOverUI] Created missing EventSystem in scene.");
            }
        }
    }
}
