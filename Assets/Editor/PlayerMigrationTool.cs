using UnityEngine;
using UnityEditor;
using FPSGame.Player.Movement;

namespace FPSGame.EditorTools
{
    public class PlayerMigrationTool
    {
        [MenuItem("Tools/Create Character Controller Player")]
        public static void CreateCCPlayer()
        {
            string originalPath = "Assets/Prefabs/PlayerCharacter.prefab";
            string newPath = "Assets/Prefabs/PlayerCharacter_CC.prefab";

            if (!System.IO.File.Exists(originalPath))
            {
                Debug.LogError("Original PlayerCharacter prefab not found at " + originalPath);
                return;
            }

            // 1. Copy prefab
            AssetDatabase.CopyAsset(originalPath, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 2. Load new prefab for modification
            GameObject newPrefab = PrefabUtility.LoadPrefabContents(newPath);

            // 3. Strip old physics
            Rigidbody rb = newPrefab.GetComponent<Rigidbody>();
            if (rb != null) Object.DestroyImmediate(rb, true);

            CapsuleCollider col = newPrefab.GetComponent<CapsuleCollider>();
            if (col != null) Object.DestroyImmediate(col, true);

            GroundChecker gc = newPrefab.GetComponentInChildren<GroundChecker>();
            if (gc != null) Object.DestroyImmediate(gc, true); // Destroy only the component

            PlayerMovement oldMovement = newPrefab.GetComponent<PlayerMovement>();
            if (oldMovement != null && oldMovement.GetType() == typeof(PlayerMovement))
            {
                Object.DestroyImmediate(oldMovement, true);
            }

            // 4. Add CC components
            CharacterController cc = newPrefab.GetComponent<CharacterController>();
            if (cc == null)
            {
                cc = newPrefab.AddComponent<CharacterController>();
                cc.center = new Vector3(0, 0f, 0); // Standard center for capsule primitives
                cc.radius = 0.5f;
                cc.height = 2f;
            }

            CharacterControllerMovement ccMovement = newPrefab.GetComponent<CharacterControllerMovement>();
            if (ccMovement == null)
            {
                ccMovement = newPrefab.AddComponent<CharacterControllerMovement>();
            }

            // 5. Re-link PlayerController
            PlayerController pController = newPrefab.GetComponent<PlayerController>();
            if (pController != null)
            {
                // We use SerializedObject to assign the movement field which is private
                SerializedObject so = new SerializedObject(pController);
                SerializedProperty moveProp = so.FindProperty("movement");
                if (moveProp != null)
                {
                    moveProp.objectReferenceValue = ccMovement;
                    so.ApplyModifiedProperties();
                }
            }

            // 6. Save Prefab
            PrefabUtility.SaveAsPrefabAsset(newPrefab, newPath);
            PrefabUtility.UnloadPrefabContents(newPrefab);

            Debug.Log("<color=green>Successfully created Character Controller player</color> at: " + newPath);
            Debug.Log("Please open the prefab, insert your downloaded Character Model, and replace the old player in the scene.");
        }
    }
}
