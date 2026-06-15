using UnityEngine;
using UnityEditor;

public class SetupCombatMechanics
{
    [MenuItem("Tools/Setup Combat Mechanics")]
    public static void Setup()
    {
        // 1. Ensure folders exist
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
            
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Combat"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Combat");

        // 2. Create Bullet Material (Yellow / Glowing)
        Material bulletMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BulletMaterial.mat");
        if (bulletMat == null)
        {
            bulletMat = new Material(Shader.Find("Standard"));
            bulletMat.color = Color.yellow;
            bulletMat.EnableKeyword("_EMISSION");
            bulletMat.SetColor("_EmissionColor", Color.yellow * 2f);
            AssetDatabase.CreateAsset(bulletMat, "Assets/Materials/BulletMaterial.mat");
        }

        // 3. Create Bullet Prefab
        string bulletPrefabPath = "Assets/Prefabs/Combat/Bullet.prefab";
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bulletPrefabPath);
        if (bulletPrefab == null)
        {
            GameObject tempBullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tempBullet.name = "Bullet";
            tempBullet.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            
            MeshRenderer mr = tempBullet.GetComponent<MeshRenderer>();
            mr.sharedMaterial = bulletMat;

            SphereCollider sc = tempBullet.GetComponent<SphereCollider>();
            sc.isTrigger = true;

            Rigidbody rb = tempBullet.AddComponent<Rigidbody>();
            rb.useGravity = false; // Bullets fly straight

            tempBullet.AddComponent<Bullet>();

            bulletPrefab = PrefabUtility.SaveAsPrefabAsset(tempBullet, bulletPrefabPath);
            Object.DestroyImmediate(tempBullet);
        }

        // 4. Update Enemy Prefab
        string enemyPrefabPath = "Assets/Prefabs/Enemy.prefab";
        GameObject enemyRoot = PrefabUtility.LoadPrefabContents(enemyPrefabPath);
        if (enemyRoot != null)
        {
            Rigidbody enemyRb = enemyRoot.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                // Freeze X and Z rotation so the enemy doesn't tip over when walking
                enemyRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            EnemyController ec = enemyRoot.GetComponent<EnemyController>();
            if (ec != null)
            {
                SerializedObject ecSo = new SerializedObject(ec);
                ecSo.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
                ecSo.ApplyModifiedProperties();
            }

            PrefabUtility.SaveAsPrefabAsset(enemyRoot, enemyPrefabPath);
            PrefabUtility.UnloadPrefabContents(enemyRoot);
            
            Debug.Log("Combat Mechanics Setup Complete! Bullet Prefab generated and assigned to Enemy.");
        }
        else
        {
            Debug.LogError("Could not find Enemy.prefab. Did you run the Weapons & Enemies setup first?");
        }
    }
}
