using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using FPSGame.Core;
using FPSGame.Core.Interfaces;
using FPSGame.Combat;
using FPSGame.Player.Movement;

namespace FPSGame.Player.Combat
{
    /// <summary>
    /// Handles the player's shooting mechanics, crosshair rendering, ammo tracking,
    /// and accurate camera-raycast-based aiming.
    /// Implements IAmmoProvider so the UI can read ammo data without referencing this assembly.
    /// </summary>
    public class PlayerCombat : MonoBehaviour, IAmmoProvider
    {
        [Header("Combat Settings")]
        public GameObject bulletPrefab;
        public float fireRate = 0.2f;

        [Header("Ammo Settings")]
        public int magazineSize = 30;
        public int totalAmmo = 120;
        public float reloadTime = 1.5f;

        // ─── IAmmoProvider Implementation ────────────────────
        public int CurrentAmmo { get; private set; }
        public int MaxAmmo => magazineSize;
        public int TotalAmmo => totalAmmo;
        public bool IsReloading { get; private set; }

        private float fireTimer;
        private Transform activeCamera;
        private Transform gunBarrel;

        private void Start()
        {
            if (Camera.main != null) activeCamera = Camera.main.transform;
            CurrentAmmo = magazineSize;

            Transform gunTransform = transform.Find("HumanoidVisuals/RightArm/Gun");
            if (gunTransform != null)
            {
                gunBarrel = gunTransform;
            }
        }

        private void Update()
        {
            if (bulletPrefab == null || gunBarrel == null || activeCamera == null) return;

            // Don't process input when game is not playing
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                return;

            fireTimer += Time.deltaTime;

            // Shoot on left mouse click
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (CurrentAmmo > 0 && !IsReloading && fireTimer >= fireRate)
                {
                    Shoot();
                    fireTimer = 0f;
                }
            }

            // Reload on R key press
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                if (!IsReloading && CurrentAmmo < magazineSize && totalAmmo > 0)
                {
                    StartCoroutine(ReloadCoroutine());
                }
            }

            // Auto-reload when magazine is empty and player tries to shoot
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame
                && CurrentAmmo <= 0 && !IsReloading && totalAmmo > 0)
            {
                StartCoroutine(ReloadCoroutine());
            }
        }

        private void Shoot()
        {
            if (activeCamera == null) return;

            CurrentAmmo--;

            // Track stats
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShotsFired++;
            }

            // Perform a raycast from the center of the screen/camera
            Ray ray = new Ray(activeCamera.position, activeCamera.forward);
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                targetPoint = hit.point;

                // Check if we hit something damageable (for accuracy tracking)
                if (hit.collider.GetComponent<Health>() != null ||
                    hit.collider.GetComponentInParent<Health>() != null)
                {
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ShotsHit++;
                    }
                }
            }
            else
            {
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

        private IEnumerator ReloadCoroutine()
        {
            IsReloading = true;
            Debug.Log("Reloading...");

            yield return new WaitForSeconds(reloadTime);

            int ammoNeeded = magazineSize - CurrentAmmo;
            int ammoToLoad = Mathf.Min(ammoNeeded, totalAmmo);

            CurrentAmmo += ammoToLoad;
            totalAmmo -= ammoToLoad;

            IsReloading = false;
            Debug.Log($"Reload complete! Magazine: {CurrentAmmo}/{magazineSize}, Reserve: {totalAmmo}");
        }

        private void OnGUI()
        {
            // Don't draw crosshair when not playing
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
                return;

            // Draw a clean, minimalist crosshair exactly in the center of the screen
            float size = 8f;
            float thickness = 2f;
            float gap = 4f;

            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);

            Color originalColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.8f);

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
}
