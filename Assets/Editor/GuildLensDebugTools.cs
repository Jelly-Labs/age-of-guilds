using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GuildLensDebugTools
{
    const string RendererAssetPath = "Assets/Settings/PC_Renderer.asset";
    const string ReflectedIslandsName = "_Islands_Reflected";
    static readonly int DebugModeId = Shader.PropertyToID("_GuildLensDebugMode");

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/0 Normal")]
    static void SetDebugModeNormal() => SetDebugMode(0);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/1 Focus Mask")]
    static void SetDebugModeFocusMask() => SetDebugMode(1);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/3 Paper Mask")]
    static void SetDebugModePaperMask() => SetDebugMode(3);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/4 Visual Mask")]
    static void SetDebugModeVisualMask() => SetDebugMode(4);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/5 Final Stylization Mask")]
    static void SetDebugModeFinalStylizationMask() => SetDebugMode(5);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/9 Exclusion Mask")]
    static void SetDebugModeExclusionMask() => SetDebugMode(9);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/11 Cartoon Blend Mask")]
    static void SetDebugModeCartoonBlendMask() => SetDebugMode(11);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/12 Cartoon Edge Preserve")]
    static void SetDebugModeCartoonEdgePreserve() => SetDebugMode(12);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/13 Cartoon Pre-Papyrus Color")]
    static void SetDebugModeCartoonColor() => SetDebugMode(13);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/14 Papyrus Texture Field")]
    static void SetDebugModePapyrusTextureField() => SetDebugMode(14);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/15 Papyrus Material Mask")]
    static void SetDebugModePapyrusMaterialMask() => SetDebugMode(15);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/16 Water Freeze Mask")]
    static void SetDebugModeWaterFreezeMask() => SetDebugMode(16);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/17 Raw Outline Mask")]
    static void SetDebugModeRawOutlineMask() => SetDebugMode(17);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/18 Organic Outline Mask")]
    static void SetDebugModeOrganicOutlineMask() => SetDebugMode(18);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/19 Zoom Reveal Driver")]
    static void SetDebugModeZoomRevealDriver() => SetDebugMode(19);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/20 Papyrus Edge Reveal Mask")]
    static void SetDebugModePapyrusEdgeRevealMask() => SetDebugMode(20);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/21 Ink Body Mask")]
    static void SetDebugModeInkBodyMask() => SetDebugMode(21);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/22 Stroke Reveal Timing")]
    static void SetDebugModeStrokeRevealTiming() => SetDebugMode(22);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/23 Ink Bleed Mask")]
    static void SetDebugModeInkBleedMask() => SetDebugMode(23);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/24 Unified Outline Thickness")]
    static void SetDebugModeUnifiedOutlineThickness() => SetDebugMode(24);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/25 Ambient Papyrus Vignette")]
    static void SetDebugModeAmbientPapyrusVignette() => SetDebugMode(25);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/26 Water Texture Contribution")]
    static void SetDebugModeWaterTextureContribution() => SetDebugMode(26);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/27 Journal Texture Contribution")]
    static void SetDebugModeJournalTextureContribution() => SetDebugMode(27);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/28 Water Outline Suppression")]
    static void SetDebugModeWaterOutlineSuppression() => SetDebugMode(28);

    [MenuItem("Tools/Guild Lens Debug/Debug Mode/29 Miniature Focus Blur Mask")]
    static void SetDebugModeMiniatureFocusBlurMask() => SetDebugMode(29);

    [MenuItem("Tools/Guild Lens Debug/Focus/Disable Screen Focus")]
    static void DisableScreenFocus() => SetScreenFocusStrength(0f);

    [MenuItem("Tools/Guild Lens Debug/Focus/Restore Screen Focus")]
    static void RestoreScreenFocus() => SetScreenFocusStrength(1f);

    [MenuItem("Tools/Guild Lens Debug/Reflections/Disable _Islands_Reflected")]
    static void DisableReflectedIslands() => SetReflectedIslandsActive(false);

    [MenuItem("Tools/Guild Lens Debug/Reflections/Enable _Islands_Reflected")]
    static void EnableReflectedIslands() => SetReflectedIslandsActive(true);

    [MenuItem("Tools/Guild Lens Debug/Reset Diagnostic State")]
    static void ResetDiagnosticState()
    {
        SetDebugMode(0);
        SetScreenFocusStrength(1f);
        SetReflectedIslandsActive(true);
    }

    [MenuItem("Tools/Guild Lens Debug/Print Current State")]
    static void PrintCurrentState()
    {
        string reflectedState = TryFindSceneObject(ReflectedIslandsName, out GameObject reflected)
            ? reflected.activeSelf ? "active" : "inactive"
            : "not found";

        Debug.Log(
            $"Guild Lens debug state: debugMode={GetDebugMode()}, " +
            $"screenFocusStrength={GetScreenFocusStrength():0.###}, " +
            $"{ReflectedIslandsName}={reflectedState}");
    }

    static void SetDebugMode(int mode)
    {
        Object feature = FindGuildLensFeature();
        if (feature == null)
        {
            Debug.LogError($"Guild Lens Debug: Could not find Guild Lens renderer feature in {RendererAssetPath}.");
            return;
        }

        SerializedObject serializedFeature = new SerializedObject(feature);
        SerializedProperty debugMode = serializedFeature.FindProperty("settings.debugMode");
        if (debugMode == null)
        {
            Debug.LogError("Guild Lens Debug: Could not find settings.debugMode on the renderer feature.");
            return;
        }

        debugMode.intValue = mode;
        serializedFeature.ApplyModifiedProperties();
        EditorUtility.SetDirty(feature);
        AssetDatabase.SaveAssets();
        Shader.SetGlobalInt(DebugModeId, mode);

        Debug.Log($"Guild Lens Debug: set debugMode to {mode}.");
    }

    static int GetDebugMode()
    {
        Object feature = FindGuildLensFeature();
        if (feature == null)
        {
            return -1;
        }

        SerializedObject serializedFeature = new SerializedObject(feature);
        SerializedProperty debugMode = serializedFeature.FindProperty("settings.debugMode");
        return debugMode != null ? debugMode.intValue : -1;
    }

    static Object FindGuildLensFeature()
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(RendererAssetPath);
        foreach (Object asset in assets)
        {
            if (asset is GuildLensRendererFeature)
            {
                return asset;
            }
        }

        foreach (Object asset in assets)
        {
            if (asset != null && asset.name == "Guild Lens")
            {
                return asset;
            }
        }

        return null;
    }

    static void SetScreenFocusStrength(float strength)
    {
        bool changedAny = false;
        foreach (GuildLensFocusDriver driver in FindSceneFocusDrivers())
        {
            SerializedObject serializedDriver = new SerializedObject(driver);
            SerializedProperty screenFocusStrength = serializedDriver.FindProperty("screenFocusStrength");
            if (screenFocusStrength == null)
            {
                continue;
            }

            screenFocusStrength.floatValue = Mathf.Clamp01(strength);
            serializedDriver.ApplyModifiedProperties();
            EditorUtility.SetDirty(driver);
            EditorSceneManager.MarkSceneDirty(driver.gameObject.scene);
            changedAny = true;
        }

        if (!changedAny)
        {
            Debug.LogError("Guild Lens Debug: Could not find a scene GuildLensFocusDriver with screenFocusStrength.");
            return;
        }

        Debug.Log($"Guild Lens Debug: set screenFocusStrength to {Mathf.Clamp01(strength):0.###}.");
    }

    static float GetScreenFocusStrength()
    {
        foreach (GuildLensFocusDriver driver in FindSceneFocusDrivers())
        {
            SerializedObject serializedDriver = new SerializedObject(driver);
            SerializedProperty screenFocusStrength = serializedDriver.FindProperty("screenFocusStrength");
            if (screenFocusStrength != null)
            {
                return screenFocusStrength.floatValue;
            }
        }

        return -1f;
    }

    static GuildLensFocusDriver[] FindSceneFocusDrivers()
    {
        GuildLensFocusDriver[] drivers = Resources.FindObjectsOfTypeAll<GuildLensFocusDriver>();
        int writeIndex = 0;
        for (int i = 0; i < drivers.Length; i++)
        {
            GuildLensFocusDriver driver = drivers[i];
            if (driver == null ||
                EditorUtility.IsPersistent(driver) ||
                !driver.gameObject.scene.IsValid())
            {
                continue;
            }

            drivers[writeIndex++] = driver;
        }

        System.Array.Resize(ref drivers, writeIndex);
        return drivers;
    }

    static void SetReflectedIslandsActive(bool active)
    {
        if (!TryFindSceneObject(ReflectedIslandsName, out GameObject reflected))
        {
            Debug.LogError($"Guild Lens Debug: Could not find scene object named {ReflectedIslandsName}.");
            return;
        }

        reflected.SetActive(active);
        EditorUtility.SetDirty(reflected);
        EditorSceneManager.MarkSceneDirty(reflected.scene);

        Debug.Log($"Guild Lens Debug: set {ReflectedIslandsName} active={active}.");
    }

    static bool TryFindSceneObject(string objectName, out GameObject result)
    {
        foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (candidate == null ||
                candidate.name != objectName ||
                EditorUtility.IsPersistent(candidate) ||
                !candidate.scene.IsValid() ||
                candidate.hideFlags != HideFlags.None)
            {
                continue;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid() && candidate.scene != activeScene)
            {
                continue;
            }

            result = candidate;
            return true;
        }

        result = null;
        return false;
    }
}
