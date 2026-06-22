namespace FPSGame.Core.Interfaces
{
    /// <summary>
    /// Interface for reading ammo data from any weapon holder.
    /// Lives in Core so the UI assembly can read ammo without referencing the Player assembly.
    /// Implemented by PlayerCombat (in Player assembly).
    /// </summary>
    public interface IAmmoProvider
    {
        /// <summary>Current rounds in the magazine.</summary>
        int CurrentAmmo { get; }

        /// <summary>Maximum rounds per magazine.</summary>
        int MaxAmmo { get; }

        /// <summary>Total reserve ammo available for reloading.</summary>
        int TotalAmmo { get; }

        /// <summary>True if currently reloading.</summary>
        bool IsReloading { get; }
    }
}
