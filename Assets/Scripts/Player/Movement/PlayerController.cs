using UnityEngine;
using FPSGame.Core;

namespace FPSGame.Player.Movement
{
    [DisallowMultipleComponent]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PerspectiveSwitcher perspectiveSwitcher;

        private void Reset()
        {
            inputReader = GetComponent<PlayerInputReader>();
            movement = GetComponent<PlayerMovement>();
            perspectiveSwitcher = GetComponent<PerspectiveSwitcher>();
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

            if (perspectiveSwitcher == null)
            {
                perspectiveSwitcher = GetComponent<PerspectiveSwitcher>();
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

            if (perspectiveSwitcher != null && inputReader.TogglePerspectivePressed)
            {
                perspectiveSwitcher.TogglePerspective();
            }

            var activeLook = perspectiveSwitcher != null ? perspectiveSwitcher.ActiveLookController : null;
            if (activeLook != null)
            {
                activeLook.ApplyLook(inputReader.LookInput);
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

            var activeLook = perspectiveSwitcher != null ? perspectiveSwitcher.ActiveLookController : null;
            bool isThirdPerson = perspectiveSwitcher != null && !perspectiveSwitcher.IsFirstPerson;
            Transform refTransform = activeLook != null ? activeLook.ReferenceTransform : null;

            movement.Move(inputReader.MoveInput, inputReader.SprintHeld, refTransform, isThirdPerson);
        }
    }
}

