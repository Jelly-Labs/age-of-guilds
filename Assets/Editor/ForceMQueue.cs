using UnityEditor;
using UnityEngine;

public class ForceMQueue : EditorWindow
{
    [MenuItem("Tools/Force Material Queue")]
    public static void Run()
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/ShipWakeMat.mat");
        if (mat != null) {
            mat.renderQueue = 3000;
            AssetDatabase.SaveAssets();
            Debug.Log("Fixed Render Queue transparent override!");
        }
    }
}
