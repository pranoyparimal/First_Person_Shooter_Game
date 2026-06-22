using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using FPSGame.Core;
using FPSGame.Combat;
using FPSGame.UI;

namespace FPSGame.Editor
{
    /// <summary>
    /// Editor tool that creates all UI prefabs with properly configured Canvas hierarchies.
    /// Run via: Tools → Setup UI System
    /// 
    /// Creates prefabs in Assets/Prefabs/UI/ that can be manually edited in the Inspector:
    /// - PlayerHUD.prefab (health bar + ammo counter)
    /// - DamageIndicator.prefab (directional damage chevrons)
    /// - PauseMenu.prefab (pause overlay)
    /// - GameOverScreen.prefab (game over + stats)
    /// - MainMenu.prefab (title screen menu)
    /// 
    /// Also adds GameManager and PlayerIdentifier to the player prefab.
    /// </summary>
    public class SetupUI
    {
        private static readonly Color DARK_BG = new Color(0.1f, 0.1f, 0.12f, 0.85f);
        private static readonly Color BUTTON_NORMAL = new Color(0.2f, 0.5f, 0.8f, 1f);
        private static readonly Color BUTTON_HOVER = new Color(0.3f, 0.6f, 0.9f, 1f);
        private static readonly Color RED_ACCENT = new Color(0.9f, 0.2f, 0.2f, 1f);

        [MenuItem("Tools/Setup UI System")]
        public static void SetupAllUI()
        {
            // Ensure folders exist
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
            }

            CreatePlayerHUDPrefab();
            CreateDamageIndicatorPrefab();
            CreatePauseMenuPrefab();
            CreateGameOverPrefab();
            CreateMainMenuPrefab();
            SetupPlayerPrefab();
            SetupGameManagerPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("=== UI System Setup Complete! ===\n" +
                      "Prefabs created in Assets/Prefabs/UI/\n" +
                      "Next steps:\n" +
                      "1. Drag PlayerHUD, DamageIndicator, PauseMenu, and GameOverScreen prefabs into your game scene\n" +
                      "2. Create a MainMenu scene and drag MainMenu prefab into it\n" +
                      "3. Add both scenes to File → Build Settings → Scenes in Build\n" +
                      "4. Or run Tools → Setup Main Menu Scene to automate steps 2-3");
        }

        // ═══════════════════════════════════════════════════════════════
        //  PLAYER HUD PREFAB
        // ═══════════════════════════════════════════════════════════════
        private static void CreatePlayerHUDPrefab()
        {
            GameObject root = CreateCanvas("PlayerHUD");

            // ─── Health Bar (Bottom-Left) ─────────────────────────
            GameObject healthPanel = CreatePanel(root, "HealthPanel",
                new Vector2(0, 0), new Vector2(0, 0),    // anchors bottom-left
                new Vector2(20, 20), new Vector2(250, 50)); // offset

            // Health bar background (dark grey)
            GameObject healthBg = CreateImage(healthPanel, "HealthBarBG",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.15f, 0.15f, 0.15f, 0.8f));

            // Health bar outline
            GameObject healthOutline = CreateImage(healthPanel, "HealthBarOutline",
                Vector2.zero, Vector2.one,
                new Vector2(-2, -2), new Vector2(2, 2),
                new Color(0.6f, 0.6f, 0.6f, 0.8f));

            // Health bar fill (green, uses filled image type)
            GameObject healthFill = CreateImage(healthPanel, "HealthBarFill",
                Vector2.zero, Vector2.one,
                new Vector2(2, 2), new Vector2(-2, -2),
                Color.green);
            Image fillImage = healthFill.GetComponent<Image>();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = 1f;

            // Health text overlay
            GameObject healthText = CreateText(healthPanel, "HealthText",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                "100 / 100", 16, Color.white, TextAnchor.MiddleCenter);

            // Health icon label
            GameObject healthLabel = CreateText(healthPanel, "HealthLabel",
                new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(0, 2), new Vector2(100, 22),
                "HEALTH", 11, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleLeft);

            // ─── Ammo Counter (Bottom-Right) ──────────────────────
            GameObject ammoPanel = CreatePanel(root, "AmmoPanel",
                new Vector2(1, 0), new Vector2(1, 0),    // anchors bottom-right
                new Vector2(-200, 20), new Vector2(-20, 70));

            // Ammo label
            CreateText(ammoPanel, "AmmoLabel",
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 2), new Vector2(0, 22),
                "AMMO", 11, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleLeft);

            // Current / Max ammo
            GameObject ammoText = CreateText(ammoPanel, "AmmoText",
                new Vector2(0, 0), new Vector2(0.7f, 1),
                new Vector2(5, 0), new Vector2(-5, 0),
                "30 / 30", 22, Color.white, TextAnchor.MiddleLeft);

            // Separator line
            CreateText(ammoPanel, "Separator",
                new Vector2(0.7f, 0.2f), new Vector2(0.7f, 0.8f),
                new Vector2(-1, 0), new Vector2(1, 0),
                "|", 14, new Color(0.5f, 0.5f, 0.5f), TextAnchor.MiddleCenter);

            // Reserve ammo
            GameObject reserveText = CreateText(ammoPanel, "ReserveAmmoText",
                new Vector2(0.75f, 0), new Vector2(1, 1),
                new Vector2(5, 0), new Vector2(-5, 0),
                "120", 16, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleLeft);

            // ─── Reload Prompt (Center-Bottom) ────────────────────
            GameObject reloadPrompt = CreateText(root, "ReloadPromptText",
                new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f),
                new Vector2(-100, -10), new Vector2(100, 20),
                "Press R to Reload", 18, new Color(1f, 0.8f, 0.2f), TextAnchor.MiddleCenter);
            reloadPrompt.SetActive(false);

            // ─── Wire up the PlayerHUD component ──────────────────
            PlayerHUD hud = root.AddComponent<PlayerHUD>();
            SerializedObject hudSo = new SerializedObject(hud);
            hudSo.FindProperty("healthBarFill").objectReferenceValue = fillImage;
            hudSo.FindProperty("healthText").objectReferenceValue = healthText.GetComponent<Text>();
            hudSo.FindProperty("ammoText").objectReferenceValue = ammoText.GetComponent<Text>();
            hudSo.FindProperty("reserveAmmoText").objectReferenceValue = reserveText.GetComponent<Text>();
            hudSo.FindProperty("reloadPromptText").objectReferenceValue = reloadPrompt.GetComponent<Text>();
            hudSo.ApplyModifiedProperties();

            SavePrefab(root, "Assets/Prefabs/UI/PlayerHUD.prefab");
        }

        // ═══════════════════════════════════════════════════════════════
        //  DAMAGE INDICATOR PREFAB
        // ═══════════════════════════════════════════════════════════════
        private static void CreateDamageIndicatorPrefab()
        {
            GameObject root = CreateCanvas("DamageIndicator");

            // Create the indicator template image (a red arrow/chevron)
            // Positioned at center, rotated to point toward damage source
            GameObject indicator = new GameObject("IndicatorTemplate");
            indicator.transform.SetParent(root.transform, false);

            RectTransform rt = indicator.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(40, 200);
            rt.anchoredPosition = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            Image indicatorImage = indicator.AddComponent<Image>();
            indicatorImage.color = new Color(1f, 0.1f, 0.1f, 0.8f);

            // Create a child that acts as the visible chevron near the screen edge
            GameObject chevron = new GameObject("Chevron");
            chevron.transform.SetParent(indicator.transform, false);

            RectTransform chevronRt = chevron.AddComponent<RectTransform>();
            chevronRt.anchorMin = new Vector2(0.5f, 1f);
            chevronRt.anchorMax = new Vector2(0.5f, 1f);
            chevronRt.sizeDelta = new Vector2(20, 40);
            chevronRt.anchoredPosition = new Vector2(0, -10);

            Image chevronImage = chevron.AddComponent<Image>();
            chevronImage.color = new Color(1f, 0.1f, 0.1f, 0.9f);

            // Make the main indicator invisible (only the chevron is visible)
            indicatorImage.color = new Color(1f, 0.1f, 0.1f, 0f);

            // The DamageIndicator script uses the chevron image for cloning
            indicator.SetActive(false);

            // Wire up the DamageIndicator component
            DamageIndicator di = root.AddComponent<DamageIndicator>();
            SerializedObject diSo = new SerializedObject(di);
            diSo.FindProperty("indicatorTemplate").objectReferenceValue = chevronImage;
            diSo.FindProperty("displayDuration").floatValue = 1.0f;
            diSo.FindProperty("fadeSpeed").floatValue = 1.0f;
            diSo.ApplyModifiedProperties();

            SavePrefab(root, "Assets/Prefabs/UI/DamageIndicator.prefab");
        }

        // ═══════════════════════════════════════════════════════════════
        //  PAUSE MENU PREFAB
        // ═══════════════════════════════════════════════════════════════
        private static void CreatePauseMenuPrefab()
        {
            GameObject root = CreateCanvas("PauseMenu");

            // Dark overlay panel
            GameObject overlay = CreateImage(root, "PausePanel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0, 0, 0, 0.6f));

            // Title
            CreateText(overlay, "TitleText",
                new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f),
                new Vector2(-200, -20), new Vector2(200, 40),
                "PAUSED", 48, Color.white, TextAnchor.MiddleCenter,
                FontStyle.Bold);

            // Buttons container
            GameObject buttonContainer = CreatePanel(overlay, "ButtonContainer",
                new Vector2(0.5f, 0.45f), new Vector2(0.5f, 0.45f),
                new Vector2(-120, -75), new Vector2(120, 75));

            GameObject resumeBtn = CreateButton(buttonContainer, "ResumeButton",
                new Vector2(0, 0.7f), new Vector2(1, 1),
                Vector2.zero, Vector2.zero,
                "RESUME", 20, BUTTON_NORMAL);

            GameObject settingsBtn = CreateButton(buttonContainer, "SettingsButton",
                new Vector2(0, 0.35f), new Vector2(1, 0.65f),
                Vector2.zero, Vector2.zero,
                "SETTINGS", 20, BUTTON_NORMAL);

            GameObject quitBtn = CreateButton(buttonContainer, "QuitToMenuButton",
                new Vector2(0, 0), new Vector2(1, 0.3f),
                Vector2.zero, Vector2.zero,
                "QUIT TO MENU", 18, RED_ACCENT);

            // Wire up PauseMenuUI
            PauseMenuUI pause = root.AddComponent<PauseMenuUI>();
            SerializedObject pauseSo = new SerializedObject(pause);
            pauseSo.FindProperty("pausePanel").objectReferenceValue = overlay;
            pauseSo.FindProperty("resumeButton").objectReferenceValue = resumeBtn.GetComponent<Button>();
            pauseSo.FindProperty("settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
            pauseSo.FindProperty("quitToMenuButton").objectReferenceValue = quitBtn.GetComponent<Button>();
            pauseSo.ApplyModifiedProperties();

            SavePrefab(root, "Assets/Prefabs/UI/PauseMenu.prefab");
        }

        // ═══════════════════════════════════════════════════════════════
        //  GAME OVER SCREEN PREFAB
        // ═══════════════════════════════════════════════════════════════
        private static void CreateGameOverPrefab()
        {
            GameObject root = CreateCanvas("GameOverScreen");

            // Dark red overlay
            GameObject overlay = CreateImage(root, "GameOverPanel",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.15f, 0.02f, 0.02f, 0.85f));

            // GAME OVER title
            CreateText(overlay, "TitleText",
                new Vector2(0.5f, 0.78f), new Vector2(0.5f, 0.78f),
                new Vector2(-250, -25), new Vector2(250, 35),
                "GAME OVER", 56, RED_ACCENT, TextAnchor.MiddleCenter,
                FontStyle.Bold);

            // Stats panel
            GameObject statsPanel = CreatePanel(overlay, "StatsPanel",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-150, -80), new Vector2(150, 80));

            // Stats rows
            float rowHeight = 35f;
            float startY = 50f;

            CreateStatRow(statsPanel, "Kills", "0", startY, out GameObject killsValue);
            CreateStatRow(statsPanel, "Accuracy", "0%", startY - rowHeight, out GameObject accuracyValue);
            CreateStatRow(statsPanel, "Time", "00:00", startY - rowHeight * 2, out GameObject timeValue);
            CreateStatRow(statsPanel, "Score", "0", startY - rowHeight * 3, out GameObject scoreValue);

            // Buttons
            GameObject buttonContainer = CreatePanel(overlay, "ButtonContainer",
                new Vector2(0.5f, 0.15f), new Vector2(0.5f, 0.15f),
                new Vector2(-120, -30), new Vector2(120, 30));

            GameObject restartBtn = CreateButton(buttonContainer, "RestartButton",
                new Vector2(0, 0), new Vector2(0.48f, 1),
                Vector2.zero, Vector2.zero,
                "RESTART", 18, BUTTON_NORMAL);

            GameObject quitBtn = CreateButton(buttonContainer, "QuitToMenuButton",
                new Vector2(0.52f, 0), new Vector2(1, 1),
                Vector2.zero, Vector2.zero,
                "QUIT", 18, RED_ACCENT);

            // Wire up GameOverUI
            GameOverUI gameOver = root.AddComponent<GameOverUI>();
            SerializedObject goSo = new SerializedObject(gameOver);
            goSo.FindProperty("gameOverPanel").objectReferenceValue = overlay;
            goSo.FindProperty("killsText").objectReferenceValue = killsValue.GetComponent<Text>();
            goSo.FindProperty("accuracyText").objectReferenceValue = accuracyValue.GetComponent<Text>();
            goSo.FindProperty("timeText").objectReferenceValue = timeValue.GetComponent<Text>();
            goSo.FindProperty("scoreText").objectReferenceValue = scoreValue.GetComponent<Text>();
            goSo.FindProperty("restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
            goSo.FindProperty("quitToMenuButton").objectReferenceValue = quitBtn.GetComponent<Button>();
            goSo.ApplyModifiedProperties();

            SavePrefab(root, "Assets/Prefabs/UI/GameOverScreen.prefab");
        }

        // ═══════════════════════════════════════════════════════════════
        //  MAIN MENU PREFAB
        // ═══════════════════════════════════════════════════════════════
        private static void CreateMainMenuPrefab()
        {
            GameObject root = CreateCanvas("MainMenu");

            // Background panel (dark gradient)
            CreateImage(root, "Background",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0.05f, 0.05f, 0.1f, 1f));

            // Game title
            GameObject title = CreateText(root, "TitleText",
                new Vector2(0.5f, 0.72f), new Vector2(0.5f, 0.72f),
                new Vector2(-300, -30), new Vector2(300, 40),
                "FIRST PERSON SHOOTER", 52, Color.white, TextAnchor.MiddleCenter,
                FontStyle.Bold);

            // Subtitle
            CreateText(root, "SubtitleText",
                new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f),
                new Vector2(-200, -10), new Vector2(200, 15),
                "A Unity FPS Game", 18, new Color(0.6f, 0.6f, 0.7f), TextAnchor.MiddleCenter);

            // Button container
            GameObject buttonContainer = CreatePanel(root, "ButtonContainer",
                new Vector2(0.5f, 0.38f), new Vector2(0.5f, 0.38f),
                new Vector2(-120, -90), new Vector2(120, 90));

            GameObject playBtn = CreateButton(buttonContainer, "PlayButton",
                new Vector2(0, 0.7f), new Vector2(1, 1),
                Vector2.zero, Vector2.zero,
                "PLAY", 24, new Color(0.2f, 0.7f, 0.3f, 1f));

            GameObject settingsBtn = CreateButton(buttonContainer, "SettingsButton",
                new Vector2(0, 0.35f), new Vector2(1, 0.65f),
                Vector2.zero, Vector2.zero,
                "SETTINGS", 20, BUTTON_NORMAL);

            GameObject quitBtn = CreateButton(buttonContainer, "QuitButton",
                new Vector2(0, 0), new Vector2(1, 0.3f),
                Vector2.zero, Vector2.zero,
                "QUIT", 20, RED_ACCENT);

            // Version text
            CreateText(root, "VersionText",
                new Vector2(1, 0), new Vector2(1, 0),
                new Vector2(-120, 5), new Vector2(-10, 25),
                "v0.1.0", 12, new Color(0.4f, 0.4f, 0.4f), TextAnchor.MiddleRight);

            // Wire up MainMenuUI
            MainMenuUI menu = root.AddComponent<MainMenuUI>();
            SerializedObject menuSo = new SerializedObject(menu);
            menuSo.FindProperty("playButton").objectReferenceValue = playBtn.GetComponent<Button>();
            menuSo.FindProperty("settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
            menuSo.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
            menuSo.FindProperty("titleText").objectReferenceValue = title.GetComponent<Text>();
            menuSo.ApplyModifiedProperties();

            SavePrefab(root, "Assets/Prefabs/UI/MainMenu.prefab");
        }

        // ═══════════════════════════════════════════════════════════════
        //  PLAYER PREFAB SETUP
        // ═══════════════════════════════════════════════════════════════
        private static void SetupPlayerPrefab()
        {
            string playerPrefabPath = "Assets/Prefabs/PlayerCharacter.prefab";
            GameObject playerRoot = PrefabUtility.LoadPrefabContents(playerPrefabPath);
            if (playerRoot == null)
            {
                Debug.LogWarning("Could not find PlayerCharacter.prefab — skipping player setup.");
                return;
            }

            // Add PlayerIdentifier if not already present
            if (playerRoot.GetComponent<PlayerIdentifier>() == null)
            {
                playerRoot.AddComponent<PlayerIdentifier>();
            }

            // Configure Health for GameOver instead of instant restart
            Health health = playerRoot.GetComponent<Health>();
            if (health != null)
            {
                SerializedObject hSo = new SerializedObject(health);
                hSo.FindProperty("deathBehavior").enumValueIndex = (int)DeathBehavior.RestartScene;
                hSo.ApplyModifiedProperties();
            }

            PrefabUtility.SaveAsPrefabAsset(playerRoot, playerPrefabPath);
            PrefabUtility.UnloadPrefabContents(playerRoot);

            Debug.Log("PlayerCharacter.prefab updated: PlayerIdentifier added, Health configured.");
        }

        // ═══════════════════════════════════════════════════════════════
        //  GAME MANAGER PREFAB
        // ═══════════════════════════════════════════════════════════════
        private static void SetupGameManagerPrefab()
        {
            string path = "Assets/Prefabs/GameManager.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
            PrefabUtility.SaveAsPrefabAsset(gmObj, path);
            Object.DestroyImmediate(gmObj);

            Debug.Log("GameManager.prefab created.");
        }

        // ═══════════════════════════════════════════════════════════════
        //  HELPER METHODS
        // ═══════════════════════════════════════════════════════════════

        private static GameObject CreateCanvas(string name)
        {
            GameObject obj = new GameObject(name);
            Canvas canvas = obj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = obj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            obj.AddComponent<GraphicRaycaster>();
            return obj;
        }

        private static GameObject CreatePanel(GameObject parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent.transform, false);

            RectTransform rt = obj.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            return obj;
        }

        private static GameObject CreateImage(GameObject parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            Color color)
        {
            GameObject obj = CreatePanel(parent, name, anchorMin, anchorMax, offsetMin, offsetMax);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            return obj;
        }

        private static GameObject CreateText(GameObject parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            string content, int fontSize, Color color, TextAnchor alignment,
            FontStyle style = FontStyle.Normal)
        {
            GameObject obj = CreatePanel(parent, name, anchorMin, anchorMax, offsetMin, offsetMax);
            Text text = obj.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Fallback to Arial if legacy font not found
            if (text.font == null)
            {
                text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
            }

            return obj;
        }

        private static GameObject CreateButton(GameObject parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            string label, int fontSize, Color bgColor)
        {
            GameObject obj = CreateImage(parent, name, anchorMin, anchorMax, offsetMin, offsetMax, bgColor);
            Button btn = obj.AddComponent<Button>();

            // Button color tint
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            btn.colors = colors;

            // Label text
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(obj.transform, false);

            RectTransform labelRt = labelObj.AddComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            Text text = labelObj.AddComponent<Text>();
            text.text = label;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.fontStyle = FontStyle.Bold;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null)
            {
                text.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
            }

            return obj;
        }

        private static void CreateStatRow(GameObject parent, string label, string defaultValue,
            float yOffset, out GameObject valueObj)
        {
            // Label (left side)
            CreateText(parent, label + "Label",
                new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(10, yOffset - 12), new Vector2(120, yOffset + 12),
                label.ToUpper(), 16, new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleLeft);

            // Value (right side)
            valueObj = CreateText(parent, label + "Value",
                new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-120, yOffset - 12), new Vector2(-10, yOffset + 12),
                defaultValue, 20, Color.white, TextAnchor.MiddleRight,
                FontStyle.Bold);
        }

        private static void SavePrefab(GameObject obj, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            Object.DestroyImmediate(obj);
            Debug.Log($"Created prefab: {path}");
        }
    }
}
