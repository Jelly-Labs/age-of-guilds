using UnityEditor;
using UnityEngine;

public class WaterSetup : EditorWindow
{
    [MenuItem("Tools/Setup Water Shader")]
    public static void RunSetup()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/WaterMaterial.mat");
        if (mat == null) {
            Debug.LogError("No WaterMaterial found at Assets/Materials/WaterMaterial.mat");
            return;
        }

        Shader newShader = Shader.Find("Custom/URPWater");
        if (newShader == null) {
            Debug.LogError("Custom/URPWater shader not found or did not compile!");
            return;
        }
        
        mat.shader = newShader;

        // Find textures automatically by name
        string[] wave1Guids = AssetDatabase.FindAssets("wave01 t:Texture2D");
        string[] wave2Guids = AssetDatabase.FindAssets("wave02 t:Texture2D");

        if (wave1Guids.Length > 0) {
            string p = AssetDatabase.GUIDToAssetPath(wave1Guids[0]);
            Texture2D tex1 = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            mat.SetTexture("_NormalMap1", tex1);
            mat.SetTextureScale("_NormalMap1", new Vector2(25, 25));
        } else {
            Debug.LogWarning("wave01 texture not found! Check if it is named correctly.");
        }

        if (wave2Guids.Length > 0) {
            string p = AssetDatabase.GUIDToAssetPath(wave2Guids[0]);
            Texture2D tex2 = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            mat.SetTexture("_NormalMap2", tex2);
            mat.SetTextureScale("_NormalMap2", new Vector2(18, 18));
        } else {
            Debug.LogWarning("wave02 texture not found! Check if it is named correctly.");
        }

        // Setup aesthetics
        mat.SetVector("_ScrollSpeed1", new Vector4(0.015f, 0.01f, 0, 0));
        mat.SetVector("_ScrollSpeed2", new Vector4(-0.01f, 0.015f, 0, 0));
        mat.SetFloat("_Smoothness", 0.96f);
        mat.SetColor("_BaseColor", new Color(0.12f, 0.35f, 0.55f));
        mat.SetColor("_SuperShallowColor", new Color(0.64f, 0.86f, 0.78f));
        mat.SetFloat("_SuperShallowStrength", 0.35f);
        mat.SetFloat("_SuperShallowWidth", 0.035f);
        mat.SetFloat("_SuperShallowSoftness", 0.025f);
        mat.SetFloat("_SuperShallowShorePull", 0.006f);
        mat.SetFloat("_DepthZone1", 0.055f);
        mat.SetFloat("_DepthZone2", 0.12f);
        mat.SetFloat("_NormalStrength", 0.8f);

        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        Debug.Log("Water Shader Bound Successfully!");
    }
}
