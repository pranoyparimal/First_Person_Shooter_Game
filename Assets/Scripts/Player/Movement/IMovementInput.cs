using UnityEngine;

namespace FPSGame.Player.Movement
{
    public interface IMovementInput
    {
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool JumpPressed { get; }
        bool SprintHeld { get; }
    }
}
