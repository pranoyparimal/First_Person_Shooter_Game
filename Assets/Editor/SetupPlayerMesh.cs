using UnityEngine;
using UnityEditor;

public class SetupPlayerMesh
{
    [MenuItem("Tools/Setup Humanoid Player Mesh")]
    public static void SetupMesh()
    {
        string prefabPath = "Assets/Prefabs/PlayerCharacter.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        if (prefabRoot == null)
        {
            Debug.LogError("Could not load PlayerCharacter.prefab");
            return;
        }

        // 1. Make the player scale a little bit smaller.
        // We'll scale the root object to 0.75x to make the whole character smaller.
        prefabRoot.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);

        // 2. Destroy the original cube mesh so it doesn't overlap the new humanoid
        // Destroying is safer than disabling, because the scene instance might have a prefab override (e.g. a material) keeping it enabled.
        MeshRenderer rootRenderer = prefabRoot.GetComponent<MeshRenderer>();
        if (rootRenderer != null)
        {
            Object.DestroyImmediate(rootRenderer, true);
        }
        MeshFilter rootFilter = prefabRoot.GetComponent<MeshFilter>();
        if (rootFilter != null)
        {
            Object.DestroyImmediate(rootFilter, true);
        }

        // 3. Create a Visuals container so we can easily delete/recreate if run multiple times
        Transform existingVisuals = prefabRoot.transform.Find("HumanoidVisuals");
        if (existingVisuals != null)
        {
            Object.DestroyImmediate(existingVisuals.gameObject);
        }

        GameObject visualsRoot = new GameObject("HumanoidVisuals");
        visualsRoot.transform.SetParent(prefabRoot.transform);
        visualsRoot.transform.localPosition = Vector3.zero;
        visualsRoot.transform.localRotation = Quaternion.identity;
        visualsRoot.transform.localScale = Vector3.one;

        // 4. Find the PlayerMaterial
        Material playerMat = null;
        string[] matGuids = AssetDatabase.FindAssets("PlayerMaterial t:Material");
        if (matGuids.Length > 0)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(matGuids[0]);
            playerMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        }

        // Helper to create body parts
        GameObject CreatePart(PrimitiveType type, string name, Vector3 pos, Vector3 scale)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(visualsRoot.transform);
            part.transform.localPosition = pos;
            part.transform.localScale = scale;

            if (playerMat != null)
            {
                MeshRenderer renderer = part.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = playerMat;
                }
            }

            // Remove colliders from visuals so they don't interfere with the main BoxCollider/Rigidbody
            Collider col = part.GetComponent<Collider>();
            if (col != null)
            {
                Object.DestroyImmediate(col);
            }

            return part;
        }

        // Torso (Cube) - Cube is 1x1x1 by default
        CreatePart(PrimitiveType.Cube, "Torso", new Vector3(0, 0.05f, 0), new Vector3(0.5f, 0.5f, 0.3f));

        // Head (Sphere) - Sphere is 1 unit diameter by default
        CreatePart(PrimitiveType.Sphere, "Head", new Vector3(0, 0.45f, 0), new Vector3(0.35f, 0.35f, 0.35f));

        // Note: Cylinder primitive is 2 units tall by default in Unity, so localScale.y is multiplied by 2 for the final height.
        
        // Arms (Cylinder)
        CreatePart(PrimitiveType.Cylinder, "LeftArm", new Vector3(-0.35f, 0.05f, 0), new Vector3(0.15f, 0.25f, 0.15f));
        CreatePart(PrimitiveType.Cylinder, "RightArm", new Vector3(0.35f, 0.05f, 0), new Vector3(0.15f, 0.25f, 0.15f));

        // Legs (Cylinder)
        CreatePart(PrimitiveType.Cylinder, "LeftLeg", new Vector3(-0.15f, -0.3f, 0), new Vector3(0.15f, 0.2f, 0.15f));
        CreatePart(PrimitiveType.Cylinder, "RightLeg", new Vector3(0.15f, -0.3f, 0), new Vector3(0.15f, 0.2f, 0.15f));

        // 4. Assign the HumanoidVisuals to the PerspectiveSwitcher so it gets hidden in First Person
        PerspectiveSwitcher switcher = prefabRoot.GetComponent<PerspectiveSwitcher>();
        if (switcher != null)
        {
            SerializedObject switcherSo = new SerializedObject(switcher);
            switcherSo.FindProperty("playerVisuals").objectReferenceValue = visualsRoot;
            switcherSo.ApplyModifiedProperties();
        }

        // 5. Change the root physical collider from BoxCollider to CapsuleCollider
        BoxCollider boxCol = prefabRoot.GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            Object.DestroyImmediate(boxCol, true);
        }
        
        CapsuleCollider capCol = prefabRoot.GetComponent<CapsuleCollider>();
        if (capCol == null)
        {
            capCol = prefabRoot.AddComponent<CapsuleCollider>();
            capCol.center = new Vector3(0, 0.06f, 0); // Center of the humanoid
            capCol.height = 1.15f; // Matches humanoid height
            capCol.radius = 0.35f; // Matches humanoid width
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("Successfully created Humanoid mesh for PlayerCharacter.prefab");
    }
}
