using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using FPSGame.Core;
using FPSGame.UI;

namespace FPSGame.Editor
{
    /// <summary>
    /// Editor tool that creates the MainMenu scene and adds both scenes to Build Settings.
    /// Run via: Tools → Setup Main Menu Scene
    /// </summary>
    public class SetupMainMenuScene
    {
        [MenuItem("Tools/Setup Main Menu Scene")]
        public static void Setup()
        {
            // 1. Save current scene
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // 2. Create new MainMenu scene
            Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 3. Set up camera background
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.backgroundColor = new Color(0.05f, 0.05f, 0.1f, 1f);
                mainCam.clearFlags = CameraClearFlags.SolidColor;
            }

            // 4. Add GameManager (it will DontDestroyOnLoad itself)
            GameObject gmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/GameManager.prefab");
            if (gmPrefab != null)
            {
                PrefabUtility.InstantiatePrefab(gmPrefab);
            }
            else
            {
                // Create inline if prefab doesn't exist
                GameObject gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
            }

            // 5. Instantiate MainMenu UI prefab
            GameObject menuPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/MainMenu.prefab");
            if (menuPrefab != null)
            {
                PrefabUtility.InstantiatePrefab(menuPrefab);
            }
            else
            {
                Debug.LogWarning("MainMenu.prefab not found! Run 'Tools → Setup UI System' first.");
            }

            // 6. Add an EventSystem if not present
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // 7. Save the scene
            string scenePath = "Assets/Scenes/MainMenu.unity";
            EditorSceneManager.SaveScene(menuScene, scenePath);

            // 8. Add both scenes to Build Settings
            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true),
                new EditorBuildSettingsScene("Assets/Scenes/SampleScene.unity", true)
            };
            EditorBuildSettings.scenes = buildScenes;

            Debug.Log("=== Main Menu Scene Setup Complete! ===\n" +
                      $"Scene saved to: {scenePath}\n" +
                      "Build Settings updated with MainMenu (index 0) and SampleScene (index 1).\n" +
                      "The game will now start from the MainMenu scene.");
        }
    }
}
