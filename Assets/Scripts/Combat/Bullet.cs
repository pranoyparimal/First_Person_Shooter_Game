using UnityEngine;

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

        // Check if we hit anything with health
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth == null)
        {
            targetHealth = other.GetComponentInParent<Health>();
        }

        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
            Debug.Log($"Bullet hit {other.name} for {damage} damage!");
        }

        // The bullet will now destroy itself when it hits ANYTHING else (like walls, floors, or enemies)
        Destroy(gameObject);
    }
}
