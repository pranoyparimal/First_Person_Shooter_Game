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

        // Perform a raycast from the center of the screen/camera
        Ray ray = new Ray(activeCamera.position, activeCamera.forward);
        Vector3 targetPoint;

        // Ignore the player's own colliders by layer or just let the bullet's own logic ignore shooter
        // We will do a general Raycast and if we hit something, that's our target.
        // maxDistance is 100f
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            // If we hit nothing (e.g. aiming at the sky), aim 100 meters forward
            targetPoint = ray.GetPoint(100f);
        }

        // Calculate fire direction from the gun barrel to the precise target point
        Vector3 fireDirection = (targetPoint - gunBarrel.position).normalized;
        
        // Spawn slightly ahead of the gun barrel
        Vector3 spawnPos = gunBarrel.position + fireDirection * 0.5f;
        Quaternion bulletRot = Quaternion.LookRotation(fireDirection);

        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, bulletRot);
        Bullet b = bulletObj.GetComponent<Bullet>();
        if (b != null)
        {
            b.shooter = gameObject;
        }
    }

    private void OnGUI()
    {
        // Draw a clean, minimalist crosshair exactly in the center of the screen
        float size = 8f;       // Length of the crosshair lines
        float thickness = 2f;  // Thickness of the lines
        float gap = 4f;        // Gap between the center and the lines

        Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // Optional: Save original color to restore later
        Color originalColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, 0.8f); // Slightly transparent white

        // Left Line
        GUI.DrawTexture(new Rect(center.x - gap - size, center.y - thickness / 2f, size, thickness), Texture2D.whiteTexture);
        // Right Line
        GUI.DrawTexture(new Rect(center.x + gap, center.y - thickness / 2f, size, thickness), Texture2D.whiteTexture);
        // Top Line
        GUI.DrawTexture(new Rect(center.x - thickness / 2f, center.y - gap - size, thickness, size), Texture2D.whiteTexture);
        // Bottom Line
        GUI.DrawTexture(new Rect(center.x - thickness / 2f, center.y + gap, thickness, size), Texture2D.whiteTexture);

        GUI.color = originalColor;
    }
}
