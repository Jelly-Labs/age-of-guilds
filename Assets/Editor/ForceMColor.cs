using UnityEditor;
using UnityEngine;

public class ForceMColor : EditorWindow
{
    [MenuItem("Tools/Force Material Opacity")]
    public static void Run()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ShipWakeMat.mat");
        if (mat != null) {
            Color c = mat.GetColor("_Color");
            c.a = 1.0f; // Force baseline alpha to 1.0
            mat.SetColor("_Color", c);
            AssetDatabase.SaveAssets();
            Debug.Log("Forced base color alpha to true 100%");
        }
    }
}
