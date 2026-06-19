using UnityEngine;
using FPSGame.Core.Interfaces;

namespace FPSGame.Combat
{
    /// <summary>
    /// A projectile that travels forward and damages the first IDamageable object it hits.
    /// Uses the IDamageable interface so it can damage anything (players, enemies, barrels)
    /// without knowing the concrete type.
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [Header("Settings")]
        public float speed = 40f;
        public float lifeTime = 3f;
        public int damage = 20;
        public GameObject shooter; // The specific character who fired the bullet

        private Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = transform.forward * speed;
            }

            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Ignore other bullets
            if (other.GetComponent<Bullet>() != null) return;

            // Prevent the bullet from hitting the person who shot it
            if (shooter != null)
            {
                if (other.gameObject == shooter || other.transform.IsChildOf(shooter.transform))
                    return;
            }

            // Check if we hit anything that implements IDamageable (Health, destructible barrel, etc.)
            IDamageable target = other.GetComponent<IDamageable>();
            if (target == null)
            {
                target = other.GetComponentInParent<IDamageable>();
            }

            if (target != null)
            {
                target.TakeDamage(damage);
                Debug.Log($"Bullet hit {other.name} for {damage} damage!");
            }

            // The bullet destroys itself when it hits ANYTHING (walls, floors, or damageable objects)
            Destroy(gameObject);
        }
    }
}
