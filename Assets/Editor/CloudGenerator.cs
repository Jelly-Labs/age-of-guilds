#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class CloudGenerator
{
    [MenuItem("Age of Guilds/Generate Atmospheric Clouds")]
    public static void GenerateClouds()
    {
        GameObject root = GameObject.Find("_Atmosphere_Clouds");
        if (root != null) GameObject.DestroyImmediate(root);
        
        root = new GameObject("_Atmosphere_Clouds");
        
        Shader cloudShader = Shader.Find("Custom/ProceduralClouds");
        if (cloudShader == null)
        {
            Debug.LogError("Could not find Custom/ProceduralClouds shader!");
            return;
        }

        Material CreateLayerMat(string name, Color baseC, Color shadowC, float scale, float cov, float soft, Vector2 wind)
        {
            string path = "Assets/Materials/" + name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(cloudShader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else { mat.shader = cloudShader; }
            
            mat.SetColor("_BaseColor", baseC);
            mat.SetColor("_ShadowColor", shadowC);
            mat.SetFloat("_Scale", scale);
            mat.SetFloat("_Coverage", cov);
            mat.SetFloat("_Softness", soft);
            mat.SetVector("_WindSpeed", new Vector4(wind.x, wind.y, 0, 0));
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // Layer 1: Fast, small, transparent mist near the water
        Material seaMistMat = CreateLayerMat("Layer1_SeaMistMat", new Color(1,1,1,0.4f), new Color(0.9f,0.95f,1f,0.4f), 0.06f, 0.25f, 0.4f, new Vector2(0.8f, 0.1f));
        // Layer 2: Core Puff Clouds overhead, casting shadows
        Material puffCloudMat = CreateLayerMat("Layer2_PuffCloudsMat", new Color(1f,0.98f,0.95f,0.95f), new Color(0.7f,0.75f,0.85f,0.9f), 0.015f, 0.5f, 0.08f, new Vector2(0.4f, 0.2f));
        // Layer 3: High altitude slow wisps
        Material skyWreathMat = CreateLayerMat("Layer3_SkyWreathsMat", new Color(1f,1f,1f,0.6f), new Color(1f,1f,1f,0.6f), 0.008f, 0.6f, 0.5f, new Vector2(-0.05f, 0.05f));
        
        // Define shadow tint aggressively visible so user sees it right away
        puffCloudMat.SetColor("_TerrainShadowTint", new Color(0.5f, 0.55f, 0.6f, 0.8f));

        GameObject CreateQuad(string name, float yHeight, Material mat)
        {
            GameObject q = GameObject.CreatePrimitive(PrimitiveType.Quad);
            q.name = name;
            q.transform.SetParent(root.transform);
            // 90 degree rotation makes the Quad flat to the horizon
            q.transform.rotation = Quaternion.Euler(90, 0, 0);
            q.transform.position = new Vector3(0, yHeight, 0);
            // Enormous scale to cover max zoom out
            q.transform.localScale = new Vector3(3000, 3000, 1);
            
            // Cleanup colliders safely
            GameObject.DestroyImmediate(q.GetComponent<MeshCollider>());
            
            // Assign Mat and disable Unity's native shadow system (performance win)
            MeshRenderer rend = q.GetComponent<MeshRenderer>();
            rend.sharedMaterial = mat;
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
            
            return q;
        }

        CreateQuad("1_SeaMist", 0.5f, seaMistMat);
        CreateQuad("2_PuffClouds", 8.0f, puffCloudMat);
        CreateQuad("3_SkyWreaths", 16.0f, skyWreathMat);

        // Hook up the global shadow parameter driver
        GlobalCloudManager manager = root.AddComponent<GlobalCloudManager>();
        manager.puffCloudMaterial = puffCloudMat;

        AssetDatabase.SaveAssets();
        Debug.Log("Age of Guilds: Procedural Clouds architecture generated successfully!");
    }
}
#endif
