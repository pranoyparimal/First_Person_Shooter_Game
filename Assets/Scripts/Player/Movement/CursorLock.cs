using UnityEngine;
using UnityEngine.InputSystem;

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
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                UnlockCursor();
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
