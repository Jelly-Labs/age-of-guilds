using UnityEditor;
using UnityEngine;

public class CreateCloudQuad : Editor
{
    [MenuItem("GameObject/Age of Guilds/Fit Mist Quad to Selected", false, 0)]
    public static void CreateMistQuad()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Please select your Waterplane or Island mesh first!");
            return;
        }

        GameObject selected = Selection.activeGameObject;
        Renderer rend = selected.GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogWarning("Selected object has no Renderer to measure bounds!");
            return;
        }

        // Measure bounds mathematically
        Bounds bounds = rend.bounds;

        // Create standard primitive Quad
        GameObject mistQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        mistQuad.name = "Layer1_SeaMist_" + selected.name;

        // Strip collider for performance
        DestroyImmediate(mistQuad.GetComponent<MeshCollider>());

        // Position it at the center of the island bounds, but lifted slightly to hover
        mistQuad.transform.position = new Vector3(bounds.center.x, 0.5f, bounds.center.z);

        // Rotate Quad to lie flat on the ground
        mistQuad.transform.rotation = Quaternion.Euler(90, 0, 0);

        // Scale the Quad to exactly match the world-space width and depth of the island bounds
        mistQuad.transform.localScale = new Vector3(bounds.size.x, bounds.size.z, 1);

        // Try to assign the mist material automatically
        Material mistMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Layer1_SeaMistMat.mat");
        if (mistMat != null)
        {
            mistQuad.GetComponent<Renderer>().sharedMaterial = mistMat;
        }

        // Parent it or keep it clean
        mistQuad.transform.SetParent(selected.transform.parent);

        Selection.activeGameObject = mistQuad;
        Debug.Log("Perfectly sized Mist Quad created based on bounds of: " + selected.name);
    }
}
