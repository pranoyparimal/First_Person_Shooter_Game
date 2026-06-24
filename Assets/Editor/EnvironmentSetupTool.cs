using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using UnityEditor.AI;

namespace FPSGame.EditorTools
{
    public class EnvironmentSetupTool
    {
        [MenuItem("Tools/Setup Environment and NavMesh")]
        public static void SetupEnvironment()
        {
            Debug.Log("Starting Environment Setup...");

            // 1. Find or create an "Environment" container
            GameObject envContainer = GameObject.Find("Environment");
            if (envContainer == null)
            {
                envContainer = new GameObject("Environment");
            }

            // Find or create "Obstacles" container inside Environment
            Transform obstaclesContainer = envContainer.transform.Find("Obstacles");
            if (obstaclesContainer == null)
            {
                GameObject obsObj = new GameObject("Obstacles");
                obsObj.transform.parent = envContainer.transform;
                obstaclesContainer = obsObj.transform;
            }

            // 2. Generate random obstacles
            int numberOfObstacles = 20;
            float spawnRadius = 20f;

            for (int i = 0; i < numberOfObstacles; i++)
            {
                bool isCube = Random.value > 0.5f;
                GameObject obstacle = GameObject.CreatePrimitive(isCube ? PrimitiveType.Cube : PrimitiveType.Cylinder);
                
                // Random position on XZ plane
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                obstacle.transform.position = new Vector3(randomCircle.x, obstacle.transform.localScale.y / 2f, randomCircle.y);
                
                // Random scale
                float scaleX = Random.Range(1f, 3f);
                float scaleY = Random.Range(1f, 4f);
                float scaleZ = Random.Range(1f, 3f);
                obstacle.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
                
                // Random rotation
                obstacle.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                obstacle.transform.parent = obstaclesContainer;

                // Set static flags for NavMesh (Suppress obsolete warning since we are using the built-in baker)
#pragma warning disable 0618
                GameObjectUtility.SetStaticEditorFlags(obstacle, StaticEditorFlags.NavigationStatic);
#pragma warning restore 0618
            }

            // 3. Find the floor (usually a Plane or Cube named "Floor" or similar) and make it static
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var go in allObjects)
            {
                if (go.name.ToLower().Contains("floor") || go.name.ToLower().Contains("ground") || go.name.ToLower().Contains("plane"))
                {
#pragma warning disable 0618
                    GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.NavigationStatic);
#pragma warning restore 0618
                }
            }

            // 4. Bake NavMesh
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            Debug.Log("NavMesh built successfully with new obstacles.");

            // 5. Update Enemy Prefab to ensure it has a NavMeshAgent
            string[] enemyGuids = AssetDatabase.FindAssets("Enemy t:Prefab", new[] { "Assets/Prefabs" });
            if (enemyGuids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(enemyGuids[0]);
                GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (enemyPrefab != null)
                {
                    bool modified = false;
                    if (enemyPrefab.GetComponent<NavMeshAgent>() == null)
                    {
                        NavMeshAgent agent = enemyPrefab.AddComponent<NavMeshAgent>();
                        agent.speed = 4f;
                        agent.stoppingDistance = 2f;
                        agent.acceleration = 8f;
                        agent.angularSpeed = 120f;
                        modified = true;
                        Debug.Log("Added NavMeshAgent to Enemy prefab.");
                    }

                    // Optional: remove Rigidbody components if NavMesh handles it, 
                    // but we will keep it and set isKinematic to true to prevent physics fighting NavMesh
                    Rigidbody rb = enemyPrefab.GetComponent<Rigidbody>();
                    if (rb != null && !rb.isKinematic)
                    {
                        rb.isKinematic = true;
                        modified = true;
                        Debug.Log("Set Rigidbody to isKinematic on Enemy prefab to prevent physics conflict with NavMesh.");
                    }

                    if (modified)
                    {
                        EditorUtility.SetDirty(enemyPrefab);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
            else
            {
                Debug.LogWarning("Enemy prefab not found in Assets/Prefabs. Could not add NavMeshAgent.");
            }

            Debug.Log("Environment Setup Complete!");
        }
    }
}
