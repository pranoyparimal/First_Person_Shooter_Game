using UnityEngine;
using FPSGame.Core;

namespace FPSGame.Player.Movement
{
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private FirstPersonLook firstPersonLook;

        private void Reset()
        {
            inputReader = GetComponent<PlayerInputReader>();
            movement = GetComponent<PlayerMovement>();
            firstPersonLook = GetComponent<FirstPersonLook>();
        }

        private void Awake()
        {
            if (inputReader == null)
            {
                inputReader = GetComponent<PlayerInputReader>();
            }

            if (movement == null)
            {
                movement = GetComponent<PlayerMovement>();
            }

            if (firstPersonLook == null)
            {
                firstPersonLook = GetComponent<FirstPersonLook>();
            }
        }

        /// <summary>
        /// Returns true only when the game is in the Playing state.
        /// Blocks all player input during Paused, GameOver, and MainMenu.
        /// </summary>
        private bool IsInputAllowed()
        {
            return GameManager.Instance == null ||
                   GameManager.Instance.CurrentState == GameManager.GameState.Playing;
        }

        private void Update()
        {
            // Block all input (camera look, jumping, perspective toggle) when not playing
            if (!IsInputAllowed()) return;

            if (firstPersonLook != null)
            {
                firstPersonLook.ApplyLook(inputReader.LookInput);
            }

            if (inputReader.JumpPressed)
            {
                movement.Jump();
            }
        }

        private void FixedUpdate()
        {
            // Block movement when not playing
            if (!IsInputAllowed()) return;

            Transform refTransform = firstPersonLook != null ? firstPersonLook.ReferenceTransform : null;

            movement.Move(inputReader.MoveInput, inputReader.SprintHeld, refTransform, false);
        }
    }
}

