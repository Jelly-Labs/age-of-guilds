using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnableDepthTexture {
    [MenuItem("Age of Guilds/Enable URP Depth Texture")]
    public static void EnableDepth() {
        var rpAssets = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");
        foreach(var guid in rpAssets) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (asset != null) {
                asset.supportsCameraDepthTexture = true;
                EditorUtility.SetDirty(asset);
                Debug.Log("Enabled Depth Texture on: " + asset.name);
            }
        }
        AssetDatabase.SaveAssets();
    }
}
