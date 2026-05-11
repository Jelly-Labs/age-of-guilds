using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(River))]
public class RiverEditor : Editor
{
    private River river;

    private void OnEnable()
    {
        river = (River)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        if (EditorGUI.EndChangeCheck())
        {
            river.GenerateMesh();
            EditorUtility.SetDirty(river);
        }

        GUILayout.Space(15);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("--- River Drafting Tools ---", headerStyle);
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Node", GUILayout.Height(30)))
        {
            Undo.RecordObject(river, "Add River Node");
            Vector3 newNodePos = river.transform.position;
            if (river.nodes.Count > 0)
            {
                // Place it slightly forward from the last node
                newNodePos = river.nodes[river.nodes.Count - 1] + new Vector3(2f, 0, 2f);
            }
            river.nodes.Add(newNodePos);
            river.GenerateMesh();
            EditorUtility.SetDirty(river);
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Remove Last", GUILayout.Height(30)) && river.nodes.Count > 0)
        {
            Undo.RecordObject(river, "Remove River Node");
            river.nodes.RemoveAt(river.nodes.Count - 1);
            river.GenerateMesh();
            EditorUtility.SetDirty(river);
            SceneView.RepaintAll();
        }
        GUILayout.EndHorizontal();
    }

    private void OnSceneGUI()
    {
        if (river.nodes == null) return;

        // Hide the default Unity Transform widget so it doesn't overlap and hide our custom cyan nodes!
        Tools.current = Tool.None;

        bool isDragging = false;

        for (int i = 0; i < river.nodes.Count; i++)
        {
            EditorGUI.BeginChangeCheck();
            
            // Standard move arrows per node!
            Vector3 newPos = Handles.PositionHandle(river.nodes[i], Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(river, "Move River Node");
                river.nodes[i] = newPos;
                isDragging = true;
            }
        }

        if (isDragging)
        {
            river.GenerateMesh();
            EditorUtility.SetDirty(river);
        }
    }
}
