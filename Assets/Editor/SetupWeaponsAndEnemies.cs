using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using FPSGame.Player.Movement;
using FPSGame.Enemies;

namespace FPSGame.Editor
{
    public class SetupWeaponsAndEnemies
    {
        [MenuItem("Tools/Setup Weapons & Enemies")]
        public static void Setup()
        {
            // 1. Create Materials
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            Material gunMat = GetOrCreateMaterial("Assets/Materials/GunMaterial.mat", new Color(0.2f, 0.2f, 0.2f));
            Material enemyMat = GetOrCreateMaterial("Assets/Materials/EnemyMaterial.mat", new Color(0.8f, 0.1f, 0.1f));

            // 2. Setup Player Gun
            string playerPrefabPath = "Assets/Prefabs/PlayerCharacter.prefab";
            GameObject playerRoot = PrefabUtility.LoadPrefabContents(playerPrefabPath);
            
            if (playerRoot == null)
            {
                Debug.LogError("Could not load PlayerCharacter.prefab");
                return;
            }

            Transform pRightArm = playerRoot.transform.Find("HumanoidVisuals/RightArm");
            if (pRightArm != null && pRightArm.Find("Gun") == null)
            {
                AttachGun(pRightArm, gunMat);
            }
            
            PrefabUtility.SaveAsPrefabAsset(playerRoot, playerPrefabPath);
            PrefabUtility.UnloadPrefabContents(playerRoot);

            // 3. Clone to Enemy Prefab
            string enemyPrefabPath = "Assets/Prefabs/Enemy.prefab";
            AssetDatabase.CopyAsset(playerPrefabPath, enemyPrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 4. Strip Enemy Prefab
            GameObject enemyRoot = PrefabUtility.LoadPrefabContents(enemyPrefabPath);
            
            // Remove Player Components
            RemoveComponent<PlayerController>(enemyRoot);
            RemoveComponent<PlayerMovement>(enemyRoot);
            RemoveComponent<FirstPersonLook>(enemyRoot);
            RemoveComponent<PlayerInputReader>(enemyRoot);

            // Remove Cameras
            Transform camPitch = enemyRoot.transform.Find("CameraPitch");
            if (camPitch != null) Object.DestroyImmediate(camPitch.gameObject);
            
            Transform tpPivot = enemyRoot.transform.Find("ThirdPersonPivot");
            if (tpPivot != null) Object.DestroyImmediate(tpPivot.gameObject);

            // Re-color Humanoid Visuals
            Transform eVisuals = enemyRoot.transform.Find("HumanoidVisuals");
            if (eVisuals != null)
            {
                foreach (MeshRenderer mr in eVisuals.GetComponentsInChildren<MeshRenderer>())
                {
                    if (mr.gameObject.name != "Gun")
                    {
                        mr.sharedMaterial = enemyMat;
                    }
                }
            }

            // Add Enemy Controller
            if (enemyRoot.GetComponent<EnemyController>() == null)
            {
                enemyRoot.AddComponent<EnemyController>();
            }

            PrefabUtility.SaveAsPrefabAsset(enemyRoot, enemyPrefabPath);
            PrefabUtility.UnloadPrefabContents(enemyRoot);

            // 5. Create Spawner in Scene
            UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (activeScene.isLoaded)
            {
                GameObject spawnerObj = GameObject.Find("EnemySpawner");
                if (spawnerObj == null)
                {
                    spawnerObj = new GameObject("EnemySpawner");
                    EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();
                    
                    // Assign enemy prefab
                    spawner.enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPrefabPath);
                    
                    EditorSceneManager.MarkSceneDirty(activeScene);
                }
            }

            Debug.Log("Successfully created Weapons, Enemy Prefab, and Spawner!");
        }

        private static void AttachGun(Transform arm, Material gunMat)
        {
            GameObject gun = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gun.name = "Gun";
            gun.transform.SetParent(arm);
            
            gun.transform.localScale = new Vector3(0.5f, 0.5f, 2f);
            gun.transform.localPosition = new Vector3(0f, -0.8f, 1f); 
            
            Collider col = gun.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            MeshRenderer mr = gun.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = gunMat;
        }

        private static Material GetOrCreateMaterial(string path, Color color)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static void RemoveComponent<T>(GameObject root) where T : Component
        {
            T comp = root.GetComponent<T>();
            if (comp != null) Object.DestroyImmediate(comp, true);
        }
    }
}
