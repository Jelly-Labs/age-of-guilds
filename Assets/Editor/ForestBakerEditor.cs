using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(ForestBaker))]
public class ForestBakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ForestBaker baker = (ForestBaker)target;
        DrawDefaultInspector();

        GUILayout.Space(15);
        GUIStyle header = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
        GUILayout.Label("--- GPU Forest Baking ---", header);
        GUILayout.Space(5);

        if (baker.clusterMesh == null)
            EditorGUILayout.HelpBox("Assign a Cluster Mesh!", MessageType.Warning);
        if (baker.targetIsland == null)
            EditorGUILayout.HelpBox("Assign a Target Island!", MessageType.Warning);
        if (baker.outputData == null)
            EditorGUILayout.HelpBox("Assign an Output ForestData asset!", MessageType.Warning);

        if (baker.exclusionMask != null)
        {
            string path = AssetDatabase.GetAssetPath(baker.exclusionMask);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null && !ti.isReadable)
                EditorGUILayout.HelpBox("Exclusion Mask must have 'Read/Write Enabled' in Import Settings!", MessageType.Error);
        }

        GUILayout.Space(5);

        bool ready = baker.clusterMesh != null && baker.targetIsland != null && baker.outputData != null;
        GUI.enabled = ready;

        if (GUILayout.Button("Bake Forest", GUILayout.Height(40)))
            BakeForest(baker);

        GUI.enabled = true;
        GUI.color = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("Clear Forest Data", GUILayout.Height(25)))
            ClearData(baker);
        GUI.color = Color.white;

        if (baker.outputData != null && baker.outputData.Count > 0)
        {
            GUILayout.Space(5);
            EditorGUILayout.HelpBox($"Baked: {baker.outputData.Count:N0} clusters", MessageType.Info);
        }
    }

    private void BakeForest(ForestBaker baker)
    {
        bool addedCollider = false;
        MeshCollider col = baker.targetIsland.GetComponent<MeshCollider>();
        if (col == null)
        {
            col = baker.targetIsland.AddComponent<MeshCollider>();
            addedCollider = true;
        }

        Bounds bounds = col.bounds;

        // Pre-bake base rotation quaternion once — this is the axis-correction for the FBX
        Quaternion baseRot = Quaternion.Euler(baker.baseRotationOffset);

        // Each Matrix4x4 = 16 floats. We build a fully-corrected TRS per instance.
        // Spatial grid for minimum-spacing rejection
        var grid = new ForestBaker.GridHash(baker.minimumSpacing);

        List<float> flatMatrices = new List<float>();
        int hits = 0;

        EditorUtility.DisplayProgressBar("Baking Forest", "Casting rays...", 0f);

        for (int i = 0; i < baker.spawnAttempts; i++)
        {
            if (i % 5000 == 0)
                EditorUtility.DisplayProgressBar("Baking Forest",
                    $"Attempt {i:N0} / {baker.spawnAttempts:N0}  |  Placed: {hits:N0}",
                    (float)i / baker.spawnAttempts);

            float rx = Random.Range(bounds.min.x, bounds.max.x);
            float rz = Random.Range(bounds.min.z, bounds.max.z);
            Ray ray = new Ray(new Vector3(rx, bounds.max.y + 100f, rz), Vector3.down);

            if (col.Raycast(ray, out RaycastHit hit, 2000f))
            {
                if (baker.IsValidPlacement(hit))
                {
                    // Reject if another cluster is already too close
                    if (baker.minimumSpacing > 0 && grid.IsTooClose(hit.point, baker.minimumSpacing))
                        continue;

                    float scale = Random.Range(baker.minScale, baker.maxScale);

                    // Combine: axis-correction first, then random Y spin
                    Quaternion rotation = baseRot * Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                    // Bake the full TRS matrix — the GPU never needs to rebuild this
                    Matrix4x4 m = Matrix4x4.TRS(hit.point, rotation, new Vector3(scale, scale, scale));

                    // Register position so nearby attempts get rejected
                    grid.Add(hit.point);

                    // Flatten to 16 floats (column-major, matches GPU float4x4 layout)
                    for (int col2 = 0; col2 < 4; col2++)
                        for (int row = 0; row < 4; row++)
                            flatMatrices.Add(m[row, col2]);

                    hits++;
                }
            }
        }

        EditorUtility.ClearProgressBar();
        if (addedCollider) DestroyImmediate(col);

        baker.outputData.matrixData    = flatMatrices.ToArray();
        baker.outputData.instanceCount = hits;
        EditorUtility.SetDirty(baker.outputData);
        AssetDatabase.SaveAssets();

        Debug.Log($"[ForestBaker] Done! Baked {hits:N0} clusters ({baker.spawnAttempts:N0} attempts).");
        EditorUtility.SetDirty(baker);
    }

    private void ClearData(ForestBaker baker)
    {
        if (baker.outputData == null) return;
        baker.outputData.matrixData    = null;
        baker.outputData.instanceCount = 0;
        EditorUtility.SetDirty(baker.outputData);
        AssetDatabase.SaveAssets();
        Debug.Log("[ForestBaker] Forest data cleared.");
    }
}
