using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class River : MonoBehaviour
{
    [Header("Spline Data")]
    public List<Vector3> nodes = new List<Vector3>();
    
    [Header("River Shaping")]
    [Tooltip("Controls the width of the river from the source (T=0) to the mouth (T=1).")]
    public AnimationCurve widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 3f);
    public float globalWidthMultiplier = 1.0f;
    
    [Header("Mesh Resolution")]
    [Range(2, 30)]
    public int segmentsPerCurve = 10;

    [ContextMenu("Generate Mesh")]
    public void GenerateMesh()
    {
        if (nodes == null || nodes.Count < 2)
        {
            GetComponent<MeshFilter>().sharedMesh = null;
            return;
        }

        List<Vector3> riverPoints = new List<Vector3>();
        
        // --- 1. EVALUATE SPLINE POSITIONS ---
        // We use Catmull-Rom to generate a smooth path through all user nodes
        for (int i = 0; i < nodes.Count - 1; i++)
        {
            Vector3 p0 = (i == 0) ? nodes[0] + (nodes[0] - nodes[1]) : nodes[i - 1];
            Vector3 p1 = nodes[i];
            Vector3 p2 = nodes[i + 1];
            Vector3 p3 = (i == nodes.Count - 2) ? nodes[i + 1] + (nodes[i + 1] - nodes[i]) : nodes[i + 2];

            int currentSegments = (i == nodes.Count - 2) ? segmentsPerCurve + 1 : segmentsPerCurve;

            for (int s = 0; s < currentSegments; s++)
            {
                float t = s / (float)segmentsPerCurve;
                riverPoints.Add(GetCatmullRomPosition(t, p0, p1, p2, p3));
            }
        }

        if (riverPoints.Count < 2) return;

        // --- 2. CALCULATE DISTANCES FOR CORRECT UV MAPPING ---
        // V-axis must represent absolute physical distance down the river, so flow speed is constant
        float totalLength = 0f;
        float[] distances = new float[riverPoints.Count];
        distances[0] = 0f;
        for (int i = 1; i < riverPoints.Count; i++)
        {
            float dist = Vector3.Distance(riverPoints[i], riverPoints[i - 1]);
            totalLength += dist;
            distances[i] = totalLength;
        }

        // --- 3. BUILD RIBBON MESH VERTS & UVS ---
        Vector3[] vertices = new Vector3[riverPoints.Count * 2];
        Vector2[] uvs = new Vector2[riverPoints.Count * 2];
        int[] triangles = new int[(riverPoints.Count - 1) * 6];

        for (int i = 0; i < riverPoints.Count; i++)
        {
            Vector3 currentPos = riverPoints[i];
            Vector3 forward = Vector3.forward;

            if (i < riverPoints.Count - 1)
            {
                forward = (riverPoints[i + 1] - currentPos).normalized;
            }
            else
            {
                forward = (currentPos - riverPoints[i - 1]).normalized;
            }

            // Right vector assumes standard Y-Up terrain.
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            // Width logic using Animation Curve evaluated over total river distance
            float normalizedT = distances[i] / totalLength;
            float currentWidth = widthCurve.Evaluate(normalizedT) * globalWidthMultiplier;

            // Left Vertex (Local Space)
            vertices[i * 2] = transform.InverseTransformPoint(currentPos - right * (currentWidth * 0.5f));
            uvs[i * 2] = new Vector2(0f, distances[i]);

            // Right Vertex (Local Space)
            vertices[i * 2 + 1] = transform.InverseTransformPoint(currentPos + right * (currentWidth * 0.5f));
            uvs[i * 2 + 1] = new Vector2(1f, distances[i]);
        }

        // --- 4. BUILD TRIANGLES (Clockwise Winding for Unity) ---
        int tris = 0;
        for (int i = 0; i < riverPoints.Count - 1; i++)
        {
            int baseIndex = i * 2;

            // Triangle 1 (Left -> Next Left -> Right)
            triangles[tris++] = baseIndex;
            triangles[tris++] = baseIndex + 2;
            triangles[tris++] = baseIndex + 1;

            // Triangle 2 (Right -> Next Left -> Next Right)
            triangles[tris++] = baseIndex + 1;
            triangles[tris++] = baseIndex + 2;
            triangles[tris++] = baseIndex + 3;
        }

        // --- 5. ASSIGN MESH ---
        Mesh mesh = new Mesh();
        mesh.name = "Procedural River Mesh";
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        // Re-calculate tangents so Normal Maps render correctly!
        mesh.RecalculateTangents();

        MeshFilter filter = GetComponent<MeshFilter>();
        filter.sharedMesh = mesh;
    }

    private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;
        Vector3 pos = 0.5f * (a + (b * t) + (c * t * t) + (d * t * t * t));
        return pos;
    }
}
