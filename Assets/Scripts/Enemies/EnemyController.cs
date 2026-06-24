using UnityEngine;
using FPSGame.Core;
using FPSGame.Combat;

namespace FPSGame.Enemies
{
    /// <summary>
    /// Controls enemy AI behavior including patrolling, player detection via a 90-degree vision cone,
    /// combat movement (chasing, retreating, strafing), and weapon firing.
    /// 
    /// Decoupled from the Player assembly:
    /// - Finds the player via PlayerIdentifier (Core assembly), not PlayerCombat.
    /// - Subscribes to Health.OnDamaged event for damage awareness, not direct coupling.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
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
        private UnityEngine.AI.NavMeshAgent agent;
        private Transform gunBarrel;

        /// <summary>
        /// Initializes references to the player, rigidbody, and gun barrel on spawn.
        /// Subscribes to the Health.OnDamaged event for damage awareness.
        /// </summary>
        private void Start()
        {
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.speed = moveSpeed;
            startYRotation = transform.eulerAngles.y;
            
            // Find player via the PlayerIdentifier marker component (lives in Core assembly).
            // This avoids referencing the Player assembly entirely.
            PlayerIdentifier pi = FindFirstObjectByType<PlayerIdentifier>();
            if (pi != null)
            {
                player = pi.transform;
            }

            // Find gun barrel
            Transform gunTransform = transform.Find("HumanoidVisuals/RightArm/Gun");
            if (gunTransform != null)
            {
                gunBarrel = gunTransform;
            }

            // Subscribe to our own Health's OnDamaged event for damage awareness.
            // This replaces the old tight coupling where Health directly called EnemyController.TriggerAlert().
            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.OnDamaged += TriggerAlert;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            Health health = GetComponent<Health>();
            if (health != null)
            {
                health.OnDamaged -= TriggerAlert;
            }
        }

        /// <summary>
        /// Instantly switches the enemy to an alert state, making them aggressively track and attack the player.
        /// Now triggered via the Health.OnDamaged event instead of direct coupling.
        /// </summary>
        public void TriggerAlert()
        {
            isAlert = true;
        }

        /// <summary>
        /// Handles movement and non-physics logic such as weapon firing timers while in combat.
        /// </summary>
        private void Update()
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
                
                if (bulletPrefab != null && gunBarrel != null)
                {
                    HandleCombatFiring(distance);
                }
            }
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
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }
            
            // Sweep left and right like a security camera
            float angle = Mathf.Sin(Time.time * patrolSpeed * Mathf.Deg2Rad) * patrolAngle;
            Quaternion rot = Quaternion.Euler(0, startYRotation + angle, 0);
            transform.rotation = rot;

            // 2. Vision Check
            if (distanceToPlayer <= preferredDistance + 5f)
            {
                float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
                
                // Is player inside the vision cone?
                if (angleToPlayer <= fieldOfView / 2f)
                {
                    Vector3 targetPoint = GetPlayerCenter();
                    
                    if (CheckLineOfSight(targetPoint, distanceToPlayer + 2f))
                    {
                        isAlert = true; // Spotted!
                        Debug.Log("<color=green>[Enemy Vision]</color> Spotted the player!");
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
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            if (agent == null || !agent.isOnNavMesh) return;

            if (distanceToPlayer > preferredDistance)
            {
                // Chase
                agent.isStopped = false;
                agent.speed = moveSpeed;
                agent.SetDestination(player.position);
            }
            else if (distanceToPlayer < backupDistance)
            {
                // Retreat
                agent.isStopped = false;
                agent.speed = moveSpeed;
                Vector3 retreatDirection = -directionToPlayer.normalized;
                Vector3 retreatPosition = transform.position + retreatDirection * 2f;
                agent.SetDestination(retreatPosition);
            }
            else
            {
                // Stop and shoot
                agent.isStopped = true;
            }
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
                Vector3 targetPoint = GetPlayerCenter();

                if (CheckLineOfSight(targetPoint, distanceToPlayer + 2f))
                {
                    if (fireTimer >= fireRate)
                    {
                        FireBullet();
                        fireTimer = 0f;
                    }
                }
            }
        }

        /// <summary>
        /// Performs a robust line-of-sight check that ignores the enemy's own colliders using RaycastAll.
        /// Uses PlayerIdentifier (Core assembly) instead of PlayerCombat to identify the player.
        /// </summary>
        private bool CheckLineOfSight(Vector3 targetPoint, float maxDistance, bool logDebug = true)
        {
            Vector3 rayDir = (targetPoint - gunBarrel.position).normalized;
            RaycastHit[] hits = Physics.RaycastAll(gunBarrel.position, rayDir, maxDistance);
            
            float closestDist = float.MaxValue;
            RaycastHit closestHit = new RaycastHit();
            bool foundValidHit = false;

            foreach (var hit in hits)
            {
                // Ignore anything attached to this enemy
                if (hit.transform.IsChildOf(this.transform))
                    continue;

                // Ignore triggers (like bullet triggers)
                if (hit.collider.isTrigger)
                    continue;

                if (hit.distance < closestDist)
                {
                    closestDist = hit.distance;
                    closestHit = hit;
                    foundValidHit = true;
                }
            }

            if (foundValidHit)
            {
                // Use PlayerIdentifier from Core assembly instead of PlayerCombat from Player assembly
                if (closestHit.collider.GetComponentInParent<PlayerIdentifier>() != null)
                {
                    Debug.Log($"Enemy Raycast hit the player: {closestHit.collider.gameObject.name}");
                    return true; // We hit the player!
                }
                else
                {
                    if (logDebug) Debug.Log($"<color=orange>[LOS Blocked]</color> Enemy vision blocked by: {closestHit.collider.name}");
                    return false; // We hit a wall or obstacle
                }
            }
            
            return false; // Hit nothing
        }

        /// <summary>
        /// Instantiates a bullet projectile and fires it in the direction of the player's chest.
        /// </summary>
        private void FireBullet()
        {
            // Calculate the direction from the gun barrel directly to the player's chest/head
            Vector3 targetPoint = GetPlayerCenter();
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

        /// <summary>
        /// Gets the true center of the player's body by finding their CapsuleCollider.
        /// </summary>
        private Vector3 GetPlayerCenter()
        {
            if (player == null) return Vector3.zero;

            CapsuleCollider cap = player.GetComponent<CapsuleCollider>();
            if (cap != null)
            {
                return player.TransformPoint(cap.center);
            }
            
            // Fallback if no colliders are found
            return player.position;
        }

        /// <summary>
        /// Draws debug gizmos in the Scene view to visualize the enemy's vision cone,
        /// line-of-sight raycast, and target point on the player.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || player == null || gunBarrel == null) return;

            Vector3 targetPoint = GetPlayerCenter();
            Vector3 rayDir = (targetPoint - gunBarrel.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            float maxDist = distanceToPlayer + 2f;

            // Draw Vision Cone boundaries roughly
            Gizmos.color = new Color(1, 1, 0, 0.3f); // Semi-transparent yellow
            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, fieldOfView / 2f, 0) * transform.forward * 5f);
            Gizmos.DrawRay(transform.position, Quaternion.Euler(0, -fieldOfView / 2f, 0) * transform.forward * 5f);

            // Draw Line of Sight
            bool canSeePlayer = CheckLineOfSight(targetPoint, maxDist, false); // false to avoid spamming console
            
            Gizmos.color = canSeePlayer ? Color.green : Color.red;
            Gizmos.DrawLine(gunBarrel.position, gunBarrel.position + rayDir * maxDist);

            // Draw the exact point we are trying to look at
            Gizmos.DrawWireSphere(targetPoint, 0.2f);
        }
    }
}
