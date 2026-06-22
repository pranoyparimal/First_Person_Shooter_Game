using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace FPSGame.Editor
{
    public class ScaleScene
    {
        [MenuItem("Tools/Scale Up Scene & Add Boundaries")]
        public static void ScaleAndAddBoundaries()
        {
            GameObject ground = GameObject.Find("Ground");
            if (ground == null)
            {
                Debug.LogError("Could not find a GameObject named 'Ground' in the scene.");
                return;
            }

            // Undo support
            Undo.RecordObject(ground.transform, "Scale Ground");

            // Scale ground up by 3
            ground.transform.localScale = new Vector3(ground.transform.localScale.x * 3f, ground.transform.localScale.y, ground.transform.localScale.z * 3f);
            
            Renderer r = ground.GetComponent<Renderer>();
            if (r == null)
            {
                Debug.LogError("Ground needs a Renderer to calculate boundaries.");
                return;
            }

            float worldSizeX = r.bounds.size.x;
            float worldSizeZ = r.bounds.size.z;

            float halfX = worldSizeX / 2f;
            float halfZ = worldSizeZ / 2f;

            // Group boundaries under a common parent
            GameObject boundariesRoot = GameObject.Find("Boundaries");
            if (boundariesRoot == null)
            {
                boundariesRoot = new GameObject("Boundaries");
                Undo.RegisterCreatedObjectUndo(boundariesRoot, "Create Boundaries Root");
            }

            // Helper to create boundary walls
            void CreateBoundary(string name, Vector3 position, Vector3 scale)
            {
                Transform existing = boundariesRoot.transform.Find(name);
                GameObject wall = existing != null ? existing.gameObject : new GameObject(name);
                if (existing == null)
                {
                    Undo.RegisterCreatedObjectUndo(wall, "Create Boundary");
                }
                else
                {
                    Undo.RecordObject(wall.transform, "Move Boundary");
                }

                wall.transform.SetParent(boundariesRoot.transform);
                wall.transform.position = position;
                wall.transform.localScale = scale;
                
                if (wall.GetComponent<BoxCollider>() == null)
                {
                    Undo.AddComponent<BoxCollider>(wall);
                }
            }

            float wallThickness = 5f;
            float wallHeight = 50f;
            
            Vector3 groundPos = ground.transform.position;

            CreateBoundary("Boundary_North", groundPos + new Vector3(0, wallHeight/2f, halfZ + wallThickness/2f), new Vector3(worldSizeX + wallThickness*2, wallHeight, wallThickness));
            CreateBoundary("Boundary_South", groundPos + new Vector3(0, wallHeight/2f, -halfZ - wallThickness/2f), new Vector3(worldSizeX + wallThickness*2, wallHeight, wallThickness));
            CreateBoundary("Boundary_East",  groundPos + new Vector3(halfX + wallThickness/2f, wallHeight/2f, 0), new Vector3(wallThickness, wallHeight, worldSizeZ));
            CreateBoundary("Boundary_West",  groundPos + new Vector3(-halfX - wallThickness/2f, wallHeight/2f, 0), new Vector3(wallThickness, wallHeight, worldSizeZ));

            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("Scaled ground by 3x and added invisible boundary colliders at the edges.");
        }
    }
}
