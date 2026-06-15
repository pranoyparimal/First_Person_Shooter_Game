using UnityEngine;
using UnityEditor;

public class SetupPerspective
{
    [MenuItem("Tools/Setup Third Person Perspective")]
    public static void Setup()
    {
        string prefabPath = "Assets/Prefabs/PlayerCube.prefab";
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

        if (prefabRoot == null)
        {
            Debug.LogError("Could not load PlayerCube.prefab");
            return;
        }

        // Add ThirdPersonLook
        ThirdPersonLook tpLook = prefabRoot.GetComponent<ThirdPersonLook>();
        if (tpLook == null)
        {
            tpLook = prefabRoot.AddComponent<ThirdPersonLook>();
        }

        // Add PerspectiveSwitcher
        PerspectiveSwitcher switcher = prefabRoot.GetComponent<PerspectiveSwitcher>();
        if (switcher == null)
        {
            switcher = prefabRoot.AddComponent<PerspectiveSwitcher>();
        }

        // Find existing First Person stuff
        FirstPersonLook fpLook = prefabRoot.GetComponent<FirstPersonLook>();
        
        Transform fpCameraPitch = prefabRoot.transform.Find("CameraPitch");
        GameObject fpCamera = null;
        if (fpCameraPitch != null)
        {
            Transform fpCamTrans = fpCameraPitch.Find("PlayerCamera");
            if (fpCamTrans != null) fpCamera = fpCamTrans.gameObject;
        }

        // Create Third Person rig
        Transform tpPivot = prefabRoot.transform.Find("ThirdPersonPivot");
        if (tpPivot == null)
        {
            GameObject pivotGo = new GameObject("ThirdPersonPivot");
            pivotGo.transform.SetParent(prefabRoot.transform);
            pivotGo.transform.localPosition = new Vector3(0, 0.5f, 0); // Centered a bit above base
            tpPivot = pivotGo.transform;
        }

        Transform tpCameraTrans = tpPivot.Find("ThirdPersonCamera");
        GameObject tpCamera = null;
        if (tpCameraTrans == null)
        {
            GameObject camGo = new GameObject("ThirdPersonCamera");
            camGo.transform.SetParent(tpPivot);
            camGo.transform.localPosition = new Vector3(0, 0, -4f); // offset
            camGo.AddComponent<Camera>();
            
            // Optionally add an AudioListener if one is needed, but we typically only want one active
            // AudioListener al = camGo.AddComponent<AudioListener>();
            
            tpCamera = camGo;
        }
        else
        {
            tpCamera = tpCameraTrans.gameObject;
        }

        // Configure ThirdPersonLook using SerializedObject to bypass private fields
        SerializedObject tpLookSo = new SerializedObject(tpLook);
        tpLookSo.FindProperty("cameraPivot").objectReferenceValue = tpPivot;
        tpLookSo.FindProperty("cameraTransform").objectReferenceValue = tpCamera.transform;
        tpLookSo.ApplyModifiedProperties();

        // Configure PerspectiveSwitcher
        SerializedObject switcherSo = new SerializedObject(switcher);
        switcherSo.FindProperty("firstPersonLook").objectReferenceValue = fpLook;
        switcherSo.FindProperty("thirdPersonLook").objectReferenceValue = tpLook;
        switcherSo.FindProperty("firstPersonCamera").objectReferenceValue = fpCamera;
        switcherSo.FindProperty("thirdPersonCamera").objectReferenceValue = tpCamera;
        switcherSo.ApplyModifiedProperties();

        // Configure PlayerController to use PerspectiveSwitcher
        PlayerController pc = prefabRoot.GetComponent<PlayerController>();
        if (pc != null)
        {
            SerializedObject pcSo = new SerializedObject(pc);
            pcSo.FindProperty("perspectiveSwitcher").objectReferenceValue = switcher;
            pcSo.ApplyModifiedProperties();
        }

        // Ensure third person components are disabled initially so they don't clash before Start()
        if (tpLook != null) tpLook.enabled = false;
        if (tpCamera != null) tpCamera.SetActive(false);
        if (fpLook != null) fpLook.enabled = true;
        if (fpCamera != null) fpCamera.SetActive(true);

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("Successfully setup Third Person Perspective on PlayerCube.prefab");
    }
}
