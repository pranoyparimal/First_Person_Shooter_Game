using UnityEngine;
using UnityEngine.UI;
using FPSGame.Core;

namespace FPSGame.UI
{
    /// <summary>
    /// Main menu title screen controller.
    /// Handles Play, Settings, and Quit buttons.
    /// All UI elements are assigned via the Inspector from the prefab.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Text titleText;

        private void Start()
        {
            // Ensure GameManager exists
            if (GameManager.Instance == null)
            {
                GameObject gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
            }

            GameManager.Instance.SetState(GameManager.GameState.MainMenu);

            // Wire up button callbacks
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            // Animate title with subtle pulsing
            if (titleText != null)
            {
                StartCoroutine(AnimateTitle());
            }
        }

        private void OnPlayClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
            }
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenu] Settings not implemented yet.");
        }

        private void OnQuitClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }

        private System.Collections.IEnumerator AnimateTitle()
        {
            while (true)
            {
                if (titleText == null) yield break;

                // Subtle pulsing alpha animation
                float alpha = 0.7f + 0.3f * Mathf.Sin(Time.unscaledTime * 1.5f);
                Color c = titleText.color;
                c.a = alpha;
                titleText.color = c;

                yield return null;
            }
        }
    }
}
