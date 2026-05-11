using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to permanently fix FBX axis issues by baking a rotation
/// directly into the mesh vertex data. Select one or more mesh assets in the
/// Project window, then run: Tools > Fix Mesh Axis (Save New Asset)
///
/// This produces a new .asset file next to the source with "_Fixed" suffix.
/// The original FBX is left untouched.
/// </summary>
public static class MeshAxisFixer
{
    [MenuItem("Tools/Fix Mesh Axis (Save New Asset)")]
    private static void FixSelectedMeshes()
    {
        Object[] selected = Selection.objects;
        if (selected == null || selected.Length == 0)
        {
            EditorUtility.DisplayDialog("Fix Mesh Axis", "Select one or more Mesh assets (FBX or Mesh) in the Project window first.", "OK");
            return;
        }

        int fixedCount = 0;

        foreach (Object obj in selected)
        {
            // Handle both direct Mesh assets and GameObjects inside FBXs
            Mesh sourceMesh = obj as Mesh;

            if (sourceMesh == null)
            {
                // Try to extract mesh from a selected FBX asset
                string assetPath = AssetDatabase.GetAssetPath(obj);
                Mesh[] meshes = GetMeshesFromAsset(assetPath);
                foreach (Mesh m in meshes)
                {
                    FixAndSave(m, assetPath);
                    fixedCount++;
                }
                continue;
            }

            string path = AssetDatabase.GetAssetPath(sourceMesh);
            FixAndSave(sourceMesh, path);
            fixedCount++;
        }

        AssetDatabase.Refresh();

        if (fixedCount > 0)
            Debug.Log($"[MeshAxisFixer] Fixed and saved {fixedCount} mesh(es). Look for '_Fixed.asset' next to your source files.");
        else
            EditorUtility.DisplayDialog("Fix Mesh Axis", "No valid meshes found in selection. Select an FBX or Mesh asset in the Project window.", "OK");
    }

    [MenuItem("Tools/Fix Mesh Axis (Save New Asset)", true)]
    private static bool ValidateSelection()
    {
        return Selection.objects != null && Selection.objects.Length > 0;
    }

    private static Mesh[] GetMeshesFromAsset(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath)) return new Mesh[0];
        Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        var meshList = new System.Collections.Generic.List<Mesh>();
        foreach (Object a in allAssets)
            if (a is Mesh m) meshList.Add(m);
        return meshList.ToArray();
    }

    private static void FixAndSave(Mesh sourceMesh, string originalPath)
    {
        // ── The Fix: Rotate all vertices/normals by -90° on X ─────────────────
        // This converts from Blender Z-up to Unity Y-up.
        // If your model needs a different correction, change this quaternion.
        Quaternion fix = Quaternion.Euler(-90f, 0f, 0f);

        Vector3[] vertices  = sourceMesh.vertices;
        Vector3[] normals   = sourceMesh.normals;
        Vector4[] tangents  = sourceMesh.tangents;

        // Apply fix to every vertex
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = fix * vertices[i];

        // Apply fix to every normal
        for (int i = 0; i < normals.Length; i++)
            normals[i] = fix * normals[i];

        // Apply fix to every tangent (preserve w handedness sign)
        for (int i = 0; i < tangents.Length; i++)
        {
            Vector3 t = fix * new Vector3(tangents[i].x, tangents[i].y, tangents[i].z);
            tangents[i] = new Vector4(t.x, t.y, t.z, tangents[i].w);
        }

        // ── Build the fixed mesh ───────────────────────────────────────────────
        Mesh fixedMesh = new Mesh();
        fixedMesh.name = sourceMesh.name + "_Fixed";
        fixedMesh.indexFormat = sourceMesh.indexFormat;
        fixedMesh.vertices    = vertices;
        fixedMesh.normals     = normals;
        fixedMesh.tangents    = tangents;
        fixedMesh.uv          = sourceMesh.uv;
        fixedMesh.uv2         = sourceMesh.uv2;
        fixedMesh.colors      = sourceMesh.colors;
        fixedMesh.subMeshCount = sourceMesh.subMeshCount;
        for (int s = 0; s < sourceMesh.subMeshCount; s++)
            fixedMesh.SetTriangles(sourceMesh.GetTriangles(s), s);
        fixedMesh.RecalculateBounds();

        // ── Save next to the original ──────────────────────────────────────────
        string dir      = Path.GetDirectoryName(originalPath);
        string baseName = Path.GetFileNameWithoutExtension(originalPath);
        string savePath = Path.Combine(dir, $"{baseName}_{sourceMesh.name}_Fixed.asset").Replace("\\", "/");

        // Avoid overwriting an existing fixed asset
        savePath = AssetDatabase.GenerateUniqueAssetPath(savePath);

        AssetDatabase.CreateAsset(fixedMesh, savePath);
        Debug.Log($"[MeshAxisFixer] Saved fixed mesh → {savePath}");
    }
}
