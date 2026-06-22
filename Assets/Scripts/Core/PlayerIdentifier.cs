using UnityEngine;

namespace FPSGame.Core
{
    /// <summary>
    /// A lightweight marker component that identifies the player GameObject.
    /// Attach this to the player's root object so that any assembly (Enemies, Combat, etc.)
    /// can locate the player without referencing player-specific scripts like PlayerCombat.
    /// This breaks the cyclic dependency between the Player and Enemies assemblies.
    /// </summary>
    public class PlayerIdentifier : MonoBehaviour
    {
        // No logic needed. This component's mere presence marks a GameObject as "the player".
    }
}
