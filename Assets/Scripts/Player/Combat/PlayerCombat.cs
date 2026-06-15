using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public GameObject bulletPrefab;
    public float fireRate = 0.2f;
    
    private float fireTimer;
    private PerspectiveSwitcher perspectiveSwitcher;
    private Transform gunBarrel;

    private void Start()
    {
        perspectiveSwitcher = GetComponent<PerspectiveSwitcher>();

        Transform gunTransform = transform.Find("HumanoidVisuals/RightArm/Gun");
        if (gunTransform != null)
        {
            gunBarrel = gunTransform;
        }
    }

    private void Update()
    {
        if (bulletPrefab == null || gunBarrel == null || perspectiveSwitcher == null) return;

        fireTimer += Time.deltaTime;

        // Use the new InputSystem directly to check for left mouse click
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (fireTimer >= fireRate)
            {
                Shoot();
                fireTimer = 0f;
            }
        }
    }

    private void Shoot()
    {
        Transform activeCamera = perspectiveSwitcher.ActiveCameraTransform;
        if (activeCamera == null) return;

        // In 3D shooters, the bullet goes towards where the camera is looking.
        // We will spawn the bullet at the gun barrel, but angle it to fly parallel to the camera's forward vector.
        Vector3 fireDirection = activeCamera.forward;
        
        // Spawn slightly ahead of the gun barrel so it doesn't collide with the player immediately
        Vector3 spawnPos = gunBarrel.position + fireDirection * 0.5f;
        Quaternion bulletRot = Quaternion.LookRotation(fireDirection);

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, bulletRot);
        Bullet b = bulletObj.GetComponent<Bullet>();
        if (b != null)
        {
            b.isPlayerBullet = true;
        }
    }
}
