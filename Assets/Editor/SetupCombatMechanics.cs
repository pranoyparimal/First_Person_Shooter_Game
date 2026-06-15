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
            tempBullet.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f); // Much smaller bullet
            
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
        else
        {
            // If it already exists, just update its scale
            GameObject tempBullet = PrefabUtility.LoadPrefabContents(bulletPrefabPath);
            tempBullet.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            PrefabUtility.SaveAsPrefabAsset(tempBullet, bulletPrefabPath);
            PrefabUtility.UnloadPrefabContents(tempBullet);
        }

        // 4. Update Enemy Prefab
        string enemyPrefabPath = "Assets/Prefabs/Enemy.prefab";
        GameObject enemyRoot = PrefabUtility.LoadPrefabContents(enemyPrefabPath);
        if (enemyRoot != null)
        {
            Rigidbody enemyRb = enemyRoot.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }

            EnemyController ec = enemyRoot.GetComponent<EnemyController>();
            if (ec != null)
            {
                SerializedObject ecSo = new SerializedObject(ec);
                ecSo.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
                ecSo.ApplyModifiedProperties();
            }

            if (enemyRoot.GetComponent<Health>() == null)
                enemyRoot.AddComponent<Health>();

            PrefabUtility.SaveAsPrefabAsset(enemyRoot, enemyPrefabPath);
            PrefabUtility.UnloadPrefabContents(enemyRoot);
        }

        // 5. Update Player Prefab
        string playerPrefabPath = "Assets/Prefabs/PlayerCharacter.prefab";
        GameObject playerRoot = PrefabUtility.LoadPrefabContents(playerPrefabPath);
        if (playerRoot != null)
        {
            if (playerRoot.GetComponent<Health>() == null)
                playerRoot.AddComponent<Health>();

            PlayerCombat pc = playerRoot.GetComponent<PlayerCombat>();
            if (pc == null)
                pc = playerRoot.AddComponent<PlayerCombat>();
            
            SerializedObject pcSo = new SerializedObject(pc);
            pcSo.FindProperty("bulletPrefab").objectReferenceValue = bulletPrefab;
            pcSo.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(playerRoot, playerPrefabPath);
            PrefabUtility.UnloadPrefabContents(playerRoot);
        }
        
        Debug.Log("Combat Mechanics Setup Complete! Health, Player Combat, and Bullets configured.");
    }
}
