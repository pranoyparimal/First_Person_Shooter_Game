using UnityEngine;
using FPSGame.Core.Interfaces;

namespace FPSGame.Combat
{
    /// <summary>
    /// A projectile that travels forward and damages the first IDamageable object it hits.
    /// Uses the IDamageable interface so it can damage anything (players, enemies, barrels)
    /// without knowing the concrete type.
    /// When hitting a Health component specifically, passes the shooter's position
    /// for directional damage indicators.
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

            // Try to find a Health component first (for directional damage info)
            Health healthTarget = other.GetComponent<Health>();
            if (healthTarget == null)
            {
                healthTarget = other.GetComponentInParent<Health>();
            }

            if (healthTarget != null)
            {
                // Use the directional overload so the victim knows where the shot came from
                Vector3 sourcePos = shooter != null ? shooter.transform.position : transform.position;
                healthTarget.TakeDamage(damage, sourcePos);
                Debug.Log($"Bullet hit {other.name} for {damage} damage!");
            }
            else
            {
                // Fallback: check for any IDamageable (barrels, etc. that might not use Health)
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
            }

            // The bullet destroys itself when it hits ANYTHING (walls, floors, or damageable objects)
            Destroy(gameObject);
        }
    }
}
