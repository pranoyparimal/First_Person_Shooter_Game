using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [Header("Awareness & Stealth")]
    public float fieldOfView = 90f;
    public float patrolSpeed = 30f;
    public float patrolAngle = 45f;
    public bool isAlert = false;
    private float startYRotation;

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

    /// <summary>
    /// Initializes references to the player, rigidbody, and gun barrel on spawn.
    /// </summary>
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        startYRotation = transform.eulerAngles.y;
        
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

    /// <summary>
    /// Instantly switches the enemy to an alert state, making them aggressively track and attack the player.
    /// Called when the enemy takes damage.
    /// </summary>
    public void TriggerAlert()
    {
        isAlert = true;
    }

    /// <summary>
    /// Handles physics-based movement and rotation based on the current alert state.
    /// </summary>
    private void FixedUpdate()
    {
        if (player == null) return;

        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; 
        float distance = Vector3.Distance(transform.position, player.position);

        if (!isAlert)
        {
            HandleIdlePatrolAndVision(distance, directionToPlayer);
        }
        else
        {
            HandleCombatMovement(distance, directionToPlayer);
        }
    }

    /// <summary>
    /// Handles non-physics logic such as weapon firing timers while in combat.
    /// </summary>
    private void Update()
    {
        if (player == null || bulletPrefab == null || gunBarrel == null || !isAlert) return;

        float distance = Vector3.Distance(transform.position, player.position);
        HandleCombatFiring(distance);
    }

    /// <summary>
    /// Controls idle patrol behavior (rotating side to side) and checks if the player 
    /// has entered the 90-degree vision cone and is visible via raycast.
    /// </summary>
    /// <param name="distanceToPlayer">Distance to the player.</param>
    /// <param name="directionToPlayer">Vector direction towards the player.</param>
    private void HandleIdlePatrolAndVision(float distanceToPlayer, Vector3 directionToPlayer)
    {
        // 1. Idle Patrol (Scanning)
        // Stop moving horizontally
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
        
        // Sweep left and right like a security camera
        float angle = Mathf.Sin(Time.time * patrolSpeed * Mathf.Deg2Rad) * patrolAngle;
        Quaternion rot = Quaternion.Euler(0, startYRotation + angle, 0);
        rb.MoveRotation(rot);

        // 2. Vision Check
        if (distanceToPlayer <= preferredDistance + 5f)
        {
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            
            // Is player inside the vision cone?
            if (angleToPlayer <= fieldOfView / 2f)
            {
                // Check Line of Sight
                Vector3 targetPoint = player.position + Vector3.up * 0.5f;
                Vector3 rayDir = (targetPoint - gunBarrel.position).normalized;
                
                if (Physics.Raycast(gunBarrel.position, rayDir, out RaycastHit hit, distanceToPlayer))
                {
                    if (hit.collider.GetComponentInParent<PlayerCombat>() != null)
                    {
                        isAlert = true; // Spotted!
                    }
                }
            }
        }
    }

    /// <summary>
    /// Controls chasing, retreating, and aiming mechanics once the enemy is alerted to the player's presence.
    /// </summary>
    /// <param name="distanceToPlayer">Distance to the player.</param>
    /// <param name="directionToPlayer">Vector direction towards the player.</param>
    private void HandleCombatMovement(float distanceToPlayer, Vector3 directionToPlayer)
    {
        // Aim at player
        if (directionToPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(directionToPlayer);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 10f));
        }

        Vector3 newVelocity = rb.linearVelocity;
        Vector3 moveDirection = directionToPlayer.normalized;

        if (distanceToPlayer > preferredDistance)
        {
            // Chase
            newVelocity.x = moveDirection.x * moveSpeed;
            newVelocity.z = moveDirection.z * moveSpeed;
        }
        else if (distanceToPlayer < backupDistance)
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

    /// <summary>
    /// Validates line-of-sight and manages the fire rate timer to shoot at the player.
    /// </summary>
    /// <param name="distanceToPlayer">Distance to the player.</param>
    private void HandleCombatFiring(float distanceToPlayer)
    {
        fireTimer += Time.deltaTime;
        
        // Check if within reasonable shooting distance (not miles away)
        if (distanceToPlayer <= preferredDistance + 5f)
        {
            // Check line of sight so they don't shoot walls
            Vector3 targetPoint = player.position + Vector3.up * 0.5f;
            Vector3 fireDirection = (targetPoint - gunBarrel.position).normalized;

            if (Physics.Raycast(gunBarrel.position, fireDirection, out RaycastHit hit, distanceToPlayer))
            {
                // Only shoot if the raycast hits the player (or their arm/gun)
                if (hit.collider.GetComponentInParent<PlayerCombat>() != null)
                {
                    if (fireTimer >= fireRate)
                    {
                        FireBullet();
                        fireTimer = 0f;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Instantiates a bullet projectile and fires it in the direction of the player's chest.
    /// </summary>
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
            b.shooter = gameObject;
        }
    }
}
