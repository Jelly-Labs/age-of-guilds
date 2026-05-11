using UnityEngine;
using UnityEditor;

public class GenerateReflections : EditorWindow
{
    private const string WaterLayerName = "Water";
    private static Material reflectionMat;
    private static Vector3 mirrorPlanePos = Vector3.zero; // Ocean surface at Y=0

    [MenuItem("Age of Guilds/Generate Flipped Reflections")]
    public static void GenerateReflectionsForSelected()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("Please select at least one GameObject to generate reflections for.");
            return;
        }

        // 1. Ensure material exists
        if (reflectionMat == null)
        {
            reflectionMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/FakeReflectionMat.mat");
            if (reflectionMat == null)
            {
                Shader shader = Shader.Find("Custom/FakeReflection");
                if (shader == null)
                {
                    Debug.LogError("Error: Cannot find Custom/FakeReflection shader!");
                    return;
                }
                reflectionMat = new Material(shader);
                AssetDatabase.CreateAsset(reflectionMat, "Assets/Materials/FakeReflectionMat.mat");
            }
        }

        // 2. Setup Reflection root
        GameObject reflectionRoot = GameObject.Find("_Reflections");
        if (reflectionRoot == null)
        {
            reflectionRoot = new GameObject("_Reflections");
        }
        int reflectionLayer = ResolveReflectionLayer(reflectionRoot.layer);
        reflectionRoot.layer = reflectionLayer;

        // 3. Process Selection
        foreach (GameObject obj in Selection.gameObjects)
        {
            CreateReflection(obj, reflectionRoot.transform, reflectionLayer);
        }
        
        Debug.Log($"Successfully generated planar reflections for {Selection.gameObjects.Length} objects.");
    }

    private static void CreateReflection(GameObject source, Transform root, int reflectionLayer)
    {
        if (source.name.EndsWith("_Reflected")) return;
        
        Transform oldRefl = root.Find(source.name + "_Reflected");
        if (oldRefl != null) DestroyImmediate(oldRefl.gameObject);

        // Instantiate will duplicate geometry and all kids
        GameObject refl = Instantiate(source);
        refl.name = source.name + "_Reflected";
        refl.transform.SetParent(root, false);
        SetLayerRecursively(refl, reflectionLayer);
        
        // Align globally
        refl.transform.position = source.transform.position;
        refl.transform.rotation = source.transform.rotation;
        
        // Invert Y scale to create the upside-down mirroring effect
        Vector3 scl = source.transform.localScale;
        refl.transform.localScale = new Vector3(scl.x, -scl.y, scl.z);

        // Move to correct mirrored position beneath the water
        Vector3 pos = refl.transform.position;
        float distToPlane = pos.y - mirrorPlanePos.y;
        pos.y -= distToPlane * 2f; 
        refl.transform.position = pos;

        // Strip colliders/scripts and override materials
        ProcessReflectionHierarchy(source.transform, refl.transform, reflectionMat);
    }

    private static int ResolveReflectionLayer(int fallbackLayer)
    {
        int waterLayer = LayerMask.NameToLayer(WaterLayerName);
        if (waterLayer >= 0)
        {
            return waterLayer;
        }

        Debug.LogWarning($"Layer '{WaterLayerName}' was not found. Generated reflections will keep layer {fallbackLayer}.");
        return fallbackLayer;
    }

    private static void SetLayerRecursively(GameObject root, int layer)
    {
        root.layer = layer;
        foreach (Transform child in root.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private static void ProcessReflectionHierarchy(Transform sourceNode, Transform reflNode, Material mat)
    {
        // Destroy anything that isn't essential for rendering
        Component[] components = reflNode.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (!(comp is Transform) && !(comp is MeshFilter) && !(comp is MeshRenderer))
            {
                DestroyImmediate(comp);
            }
        }

        Renderer sRend = sourceNode.GetComponent<Renderer>();
        Renderer rRend = reflNode.GetComponent<Renderer>();

        if (sRend != null && rRend != null)
        {
            // Override with reflection material
            Material[] mats = new Material[rRend.sharedMaterials.Length];
            for(int i=0; i<mats.Length; i++) mats[i] = mat;
            rRend.sharedMaterials = mats;

            // Fetch original texture securely
            Texture mainTex = null;
            if (sRend.sharedMaterial != null)
            {
                if (sRend.sharedMaterial.HasProperty("_BaseMap"))
                    mainTex = sRend.sharedMaterial.GetTexture("_BaseMap");
                else if (sRend.sharedMaterial.HasProperty("_MainTex"))
                    mainTex = sRend.sharedMaterial.GetTexture("_MainTex");
            }

            // Bind the texture to this specific instance using a Property Block
            if (mainTex != null)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                rRend.GetPropertyBlock(propBlock);
                propBlock.SetTexture("_MainTex", mainTex);
                rRend.SetPropertyBlock(propBlock);
            }
        }

        // Recursively clean up children
        for (int i = 0; i < sourceNode.childCount; i++)
        {
            if (i < reflNode.childCount)
            {
                ProcessReflectionHierarchy(sourceNode.GetChild(i), reflNode.GetChild(i), mat);
            }
        }
    }
}
