using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using FPSGame.Core;
using UnityEngine.InputSystem.UI;

namespace FPSGame.UI
{
    /// <summary>
    /// Pause menu overlay. Toggled with ESC key during gameplay.
    /// Sets Time.timeScale = 0 when paused, restores on resume.
    /// All UI elements are assigned via the Inspector from the prefab.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitToMenuButton;

        private void Start()
        {
            // Ensure an EventSystem exists — without it, no UI clicks work at all
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<InputSystemUIInputModule>();
                Debug.Log("[PauseMenuUI] Created missing EventSystem in scene.");
            }

            // Wire up button callbacks
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitToMenuButton != null)
                quitToMenuButton.onClick.AddListener(OnQuitToMenuClicked);

            // Start hidden
            if (pausePanel != null)
                pausePanel.SetActive(false);

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

        private void Update()
        {
            // Toggle pause with ESC key
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (GameManager.Instance == null) return;

                if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
                {
                    GameManager.Instance.SetState(GameManager.GameState.Paused);
                }
                else if (GameManager.Instance.CurrentState == GameManager.GameState.Paused)
                {
                    GameManager.Instance.SetState(GameManager.GameState.Playing);
                }
            }
        }

        private void OnGameStateChanged(GameManager.GameState newState)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(newState == GameManager.GameState.Paused);
            }
        }

        private void OnResumeClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameManager.GameState.Playing);
            }
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[PauseMenu] Settings not implemented yet.");
        }

        private void OnQuitToMenuClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitToMainMenu();
            }
        }
    }
}
