using UnityEditor;
using UnityEngine;

public class SwitchIslandShaders
{
    [MenuItem("Tools/Switch Islands to Custom Shader")]
    public static void Run()
    {
        string[] matPaths = {
            "Assets/Materials/Island01.mat",
            "Assets/Materials/Island02.mat",
            "Assets/Materials/Island03.mat",
            "Assets/Materials/Island04.mat"
        };

        Shader customShader = Shader.Find("Custom/URPLitIsland");
        if (customShader == null)
        {
            Debug.LogError("Custom/URPLitIsland shader not found! Make sure it compiled correctly.");
            return;
        }

        Texture2D shorelineMap = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/Textures/CoastalLandDistance_AgeOfGuilds.exr");
        if (shorelineMap == null)
            shorelineMap = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Materials/Textures/CoastalFlowMap_AgeOfGuilds.png");

        foreach (string path in matPaths)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) { Debug.LogWarning($"Material not found: {path}"); continue; }

            mat.shader = customShader;

            // Set sensible defaults for the custom terrain shader
            mat.SetColor("_GrassColor", new Color(0.22f, 0.48f, 0.14f));
            mat.SetColor("_SandColor",  new Color(0.78f, 0.69f, 0.48f, 1f));
            mat.SetFloat("_BeachHeightMax", 0.8f);
            mat.SetFloat("_BeachBlend", 0.3f);
            mat.SetFloat("_BeachHeight", 0.8f);
            mat.SetFloat("_Blend", 0.3f);
            mat.SetFloat("_LegacyBeachStrength", 0f);

            if (shorelineMap != null)
                mat.SetTexture("_ShorelineDistanceMap", shorelineMap);

            mat.SetVector("_ShorelineMapOrigin", new Vector4(-150f, -150f, 0f, 0f));
            mat.SetVector("_ShorelineMapSize", new Vector4(300f, 300f, 0f, 0f));
            mat.SetFloat("_ShorelineMapFlipX", 0f);
            mat.SetFloat("_ShorelineMapFlipY", 1f);
            mat.SetFloat("_ShorelineMapMaxDistance", 100f);
            mat.SetFloat("_ShorelineBeachWidth", 2.5f);
            mat.SetFloat("_ShorelineBeachSoftness", 0.75f);
            mat.SetFloat("_ShorelineBeachStrength", 1f);
            mat.SetColor("_ShorelineOverlayColor", new Color(0.78f, 0.69f, 0.48f, 1f));
            mat.SetFloat("_ShorelineOverlayStrength", 1f);
            mat.SetFloat("_ShorelineTextureOverride", 1f);
            mat.SetFloat("_ShorelineNormalOverride", 1f);
            mat.SetFloat("_ShorelineBreakupScale", 0.2f);
            mat.SetFloat("_ShorelineBreakupStrength", 0.15f);
            mat.SetFloat("_ShorelineCliffExclusionSoftness", 0.08f);
            mat.SetFloat("_ShorelineCliffOverride", 0f);
            mat.SetFloat("_ShorelineSlopeFadeStart", 0.2f);
            mat.SetFloat("_ShorelineSlopeFadeEnd", 0.7f);
            mat.SetFloat("_ShorelineSteepStrength", 1f);
            mat.SetFloat("_ShorelineDebugMode", 0f);

            mat.SetFloat("_RoadStrength", 1f);
            mat.SetColor("_RoadColor", new Color(0.55f, 0.43f, 0.28f, 1f));
            mat.SetFloat("_RoadWidth", 0.65f);
            mat.SetFloat("_RoadSoftness", 0.35f);
            mat.SetFloat("_RoadSeed", 11f);
            mat.SetFloat("_RoadDensity", 0.7f);
            mat.SetFloat("_RoadBranchCount", 5f);
            mat.SetFloat("_RoadRadius", 18f);
            mat.SetFloat("_RoadMeanderScale", 0.25f);
            mat.SetFloat("_RoadMeanderStrength", 1.4f);
            mat.SetFloat("_RoadHeightMin", -10f);
            mat.SetFloat("_RoadHeightMax", 200f);
            mat.SetFloat("_RoadSlopeMax", 0.2f);
            mat.SetFloat("_RoadSmoothness", 0.08f);
            mat.SetFloat("_RoadNormalDamp", 0.85f);
            mat.SetFloat("_RoadTextureOverride", 0.4f);
            mat.SetFloat("_RoadMaxDarken", 0.18f);
            mat.SetFloat("_RoadMaskCutoff", 0.25f);
            mat.SetFloat("_RoadMaskContrast", 3f);
            mat.SetFloat("_RoadDebugMode", 0f);
            mat.SetFloat("_RoadAnchorCount", 0f);
            for (int i = 0; i < 8; i++)
                mat.SetVector($"_RoadAnchor{i}", Vector4.zero);

            EditorUtility.SetDirty(mat);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Island01.mat through Island04.mat switched to Custom/URPLitIsland successfully!");
    }
}
