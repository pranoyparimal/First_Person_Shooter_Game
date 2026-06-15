using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 40f; 
    public float lifeTime = 3f;
    public int damage = 20;   
    public bool isPlayerBullet; // True if player shot it, false if enemy

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

        // Determine if we hit the player (or something on the player like the gun/arm)
        bool hitPlayer = other.CompareTag("Player") || other.GetComponentInParent<PlayerCombat>() != null;

        // Prevent self-damage
        if (isPlayerBullet && hitPlayer) return;
        if (!isPlayerBullet && !hitPlayer) return;

        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth == null)
        {
            targetHealth = other.GetComponentInParent<Health>();
        }

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
            string shooter = isPlayerBullet ? "Player" : "Enemy";
            Debug.Log($"{shooter}'s bullet hit {other.name} for {damage} damage!");
        }

        Destroy(gameObject);
    }
}
