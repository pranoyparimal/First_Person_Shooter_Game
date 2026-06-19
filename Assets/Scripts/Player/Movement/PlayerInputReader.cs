using UnityEngine;
using UnityEngine.InputSystem;

namespace FPSGame.Player.Movement
{
    [DisallowMultipleComponent]
    public class PlayerInputReader : MonoBehaviour, IMovementInput
    {
        [SerializeField] private InputActionAsset inputActions;

        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction togglePerspectiveAction;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool TogglePerspectivePressed { get; private set; }

        private void Awake()
        {
            if (inputActions == null)
            {
                Debug.LogError($"{nameof(PlayerInputReader)} requires an InputActionAsset.", this);
                return;
            }

            var playerMap = inputActions.FindActionMap("Player", true);
            moveAction = playerMap.FindAction("Move", true);
            lookAction = playerMap.FindAction("Look", true);
            jumpAction = playerMap.FindAction("Jump", true);
            sprintAction = playerMap.FindAction("Sprint", true);
            togglePerspectiveAction = playerMap.FindAction("TogglePerspective", true);
        }

        private void OnEnable()
        {
            moveAction?.Enable();
            lookAction?.Enable();
            jumpAction?.Enable();
            sprintAction?.Enable();
            togglePerspectiveAction?.Enable();
        }

        private void OnDisable()
        {
            moveAction?.Disable();
            lookAction?.Disable();
            jumpAction?.Disable();
            sprintAction?.Disable();
            togglePerspectiveAction?.Disable();
        }

        private void Update()
        {
            if (moveAction == null)
            {
                return;
            }

            MoveInput = moveAction.ReadValue<Vector2>();
            LookInput = lookAction.ReadValue<Vector2>();
            JumpPressed = jumpAction.WasPressedThisFrame();
            SprintHeld = sprintAction.IsPressed();
            TogglePerspectivePressed = togglePerspectiveAction != null && togglePerspectiveAction.WasPressedThisFrame();
        }
    }
}
