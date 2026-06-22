using UnityEngine;
using UnityEngine.SceneManagement;

namespace FPSGame.Core
{
    /// <summary>
    /// Global game state manager. Singleton that persists across scenes.
    /// Manages game flow (MainMenu -> Playing -> Paused -> GameOver) and tracks player stats.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>
        /// All possible game states.
        /// </summary>
        public enum GameState
        {
            MainMenu,
            Playing,
            Paused,
            GameOver
        }

        /// <summary>
        /// Fired whenever the game state changes. UI elements subscribe to this.
        /// </summary>
        public event System.Action<GameState> OnGameStateChanged;

        /// <summary>
        /// The current game state.
        /// </summary>
        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        // ─── Player Stats (reset each game session) ────────────────────
        public int Kills { get; set; }
        public int ShotsFired { get; set; }
        public int ShotsHit { get; set; }
        public float TimeSurvived { get; private set; }

        /// <summary>
        /// Accuracy as a percentage (0-100). Returns 0 if no shots fired.
        /// </summary>
        public float Accuracy => ShotsFired > 0 ? (float)ShotsHit / ShotsFired * 100f : 0f;

        /// <summary>
        /// Score formula: kills * 100 + time bonus.
        /// </summary>
        public int Score => Kills * 100 + (int)(TimeSurvived * 10f);

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            // Track time survived during gameplay
            if (CurrentState == GameState.Playing)
            {
                TimeSurvived += Time.deltaTime;
            }
        }

        /// <summary>
        /// Transitions to a new game state. Handles Time.timeScale and cursor locking.
        /// </summary>
        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;

            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;

                case GameState.Playing:
                    Time.timeScale = 1f;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;

                case GameState.Paused:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;

                case GameState.GameOver:
                    Time.timeScale = 0f;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
            }

            OnGameStateChanged?.Invoke(newState);
            Debug.Log($"[GameManager] State changed to: {newState}");
        }

        /// <summary>
        /// Resets all stats for a new game session.
        /// </summary>
        public void ResetStats()
        {
            Kills = 0;
            ShotsFired = 0;
            ShotsHit = 0;
            TimeSurvived = 0f;
        }

        /// <summary>
        /// Starts a new game: resets stats, loads the game scene, sets state to Playing.
        /// </summary>
        public void StartGame()
        {
            ResetStats();
            Time.timeScale = 1f;
            SceneManager.LoadScene("SampleScene");
            // State will be set to Playing after scene loads
            SceneManager.sceneLoaded += OnGameSceneLoaded;
        }

        /// <summary>
        /// Returns to the main menu scene.
        /// </summary>
        public void QuitToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
            SetState(GameState.MainMenu);
        }

        /// <summary>
        /// Restarts the current game session.
        /// </summary>
        public void RestartGame()
        {
            ResetStats();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            SceneManager.sceneLoaded += OnGameSceneLoaded;
        }

        private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnGameSceneLoaded;
            SetState(GameState.Playing);
        }

        /// <summary>
        /// Quits the application.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[GameManager] Quitting game...");
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
