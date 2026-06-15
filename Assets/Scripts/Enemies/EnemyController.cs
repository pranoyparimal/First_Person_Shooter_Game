using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float preferredDistance = 10f; // Maintains this distance from the player
    public float backupDistance = 8f;   // Backs away if player is closer than this

    [Header("Combat")]
    public GameObject bulletPrefab;
    public float fireRate = 1.5f;
    private float fireTimer;

    private Transform player;
    private Rigidbody rb;
    private Transform gunBarrel;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Find player
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null)
        {
            player = pObj.transform;
        }

        // Find gun barrel
        Transform gunTransform = transform.Find("HumanoidVisuals/RightArm/Gun");
        if (gunTransform != null)
        {
            gunBarrel = gunTransform;
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        // 1. Aiming (Look at player)
        // Only rotate on the Y axis to stay upright
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; 

        if (directionToPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(directionToPlayer);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 10f));
        }

        // 2. Movement Logic
        float distance = Vector3.Distance(transform.position, player.position);
        Vector3 newVelocity = rb.linearVelocity;
        
        // Move horizontally toward or away from player depending on distance
        Vector3 moveDirection = directionToPlayer.normalized;

        if (distance > preferredDistance)
        {
            // Chase
            newVelocity.x = moveDirection.x * moveSpeed;
            newVelocity.z = moveDirection.z * moveSpeed;
        }
        else if (distance < backupDistance)
        {
            // Retreat
            newVelocity.x = -moveDirection.x * moveSpeed;
            newVelocity.z = -moveDirection.z * moveSpeed;
        }
        else
        {
            // Stop and shoot (stand still horizontally)
            newVelocity.x = 0;
            newVelocity.z = 0;
        }

        rb.linearVelocity = newVelocity;
    }

    private void Update()
    {
        if (player == null || bulletPrefab == null || gunBarrel == null) return;

        // 3. Firing Logic
        fireTimer += Time.deltaTime;
        
        // Check if within reasonable shooting distance (not miles away)
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= preferredDistance + 5f)
        {
            if (fireTimer >= fireRate)
            {
                FireBullet();
                fireTimer = 0f;
            }
        }
    }

    private void FireBullet()
    {
        // Calculate the direction from the gun barrel directly to the player's chest/head
        Vector3 targetPoint = player.position + Vector3.up * 0.5f;
        Vector3 fireDirection = (targetPoint - gunBarrel.position).normalized;

        // Instantiate bullet slightly ahead of the gun so it doesn't collide with the enemy immediately
        Vector3 spawnPos = gunBarrel.position + fireDirection * 0.5f;
        
        // Align the bullet with the fire direction
        Quaternion bulletRot = Quaternion.LookRotation(fireDirection);

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, bulletRot);
        Bullet b = bulletObj.GetComponent<Bullet>();
        if (b != null)
        {
            b.isPlayerBullet = false;
        }
    }
}
