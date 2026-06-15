using UnityEngine;

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

    private void Update()
    {
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
        var activeLook = perspectiveSwitcher != null ? perspectiveSwitcher.ActiveLookController : null;
        bool isThirdPerson = perspectiveSwitcher != null && !perspectiveSwitcher.IsFirstPerson;
        Transform refTransform = activeLook != null ? activeLook.ReferenceTransform : null;

        movement.Move(inputReader.MoveInput, inputReader.SprintHeld, refTransform, isThirdPerson);
    }
}
