namespace FPSGame.Core.Interfaces
{
    /// <summary>
    /// Universal interface for any object that can receive damage.
    /// Implemented by Health, but can be implemented by destructible barrels, turrets, etc.
    /// Lives in the Core assembly so all other assemblies can reference it without cyclic dependencies.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }
}
