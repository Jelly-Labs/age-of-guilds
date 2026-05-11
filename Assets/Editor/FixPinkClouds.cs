using UnityEditor;
using UnityEngine;
public class FixPinkClouds {
    [MenuItem("Age of Guilds/Fix Pink Cloud Materials")]
    public static void Fix() {
        Shader s = Shader.Find("Custom/ProceduralClouds");
        string[] paths = {"Assets/Materials/Layer1_SeaMistMat.mat", "Assets/Materials/Layer2_PuffCloudsMat.mat", "Assets/Materials/Layer3_SkyWreathsMat.mat"};
        foreach(var p in paths) {
            Material m = AssetDatabase.LoadAssetAtPath<Material>(p);
            if (m != null) { m.shader = s; EditorUtility.SetDirty(m); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Clouds fixed!");
    }
}
