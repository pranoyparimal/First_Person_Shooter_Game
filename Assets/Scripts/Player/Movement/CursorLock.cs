using UnityEngine;
using UnityEngine.InputSystem;
using FPSGame.Core;

namespace FPSGame.Player.Movement
{
    [DisallowMultipleComponent]
    public class CursorLock : MonoBehaviour
    {
        [SerializeField] private bool lockOnStart = true;

        private void Start()
        {
            if (lockOnStart)
            {
                LockCursor();
            }
        }

        private void Update()
        {
            // Only manage cursor during active gameplay.
            // During Paused/GameOver/MainMenu, the GameManager controls the cursor.
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            {
                return;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
            {
                LockCursor();
            }
        }

        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
