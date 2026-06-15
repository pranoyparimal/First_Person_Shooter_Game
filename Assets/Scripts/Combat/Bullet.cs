using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 20f;
    public float lifeTime = 3f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Propel the bullet forward immediately
            rb.linearVelocity = transform.forward * speed;
        }

        // Destroy after lifeTime seconds to prevent memory leaks
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Don't hit the enemy that fired it or other bullets
        if (other.CompareTag("Enemy") || other.GetComponent<Bullet>() != null)
            return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit by bullet!");
            // TODO: Apply damage to player when health system is implemented
        }
        else
        {
            Debug.Log($"Bullet hit {other.name}");
        }

        // Destroy bullet upon impact
        Destroy(gameObject);
    }
}
