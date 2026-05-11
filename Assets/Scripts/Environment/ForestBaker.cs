using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Editor-side scatter configuration. Attach to any GameObject.
/// Use the ForestBakerEditor custom inspector to bake the forest into a ForestData asset.
/// </summary>
public class ForestBaker : MonoBehaviour
{
    [Header("Mesh & Output")]
    [Tooltip("The single optimized cluster FBX mesh to scatter.")]
    public Mesh clusterMesh;

    [Tooltip("The island GameObject to scatter trees onto (must have MeshCollider or we'll add one temporarily).")]
    public GameObject targetIsland;

    [Tooltip("The ForestData asset to write results into. Create one via Assets > Create > Age of Guilds > Forest Data.")]
    public ForestData outputData;

    [Header("Exclusion Mask")]
    [Tooltip("Optional greyscale texture. Black = no trees, White = trees allowed. Must be Read/Write enabled in import settings!")]
    public Texture2D exclusionMask;

    [Tooltip("UV channel of the island mesh used for exclusion mask sampling.")]
    public int exclusionMaskUVChannel = 0;

    [Header("Density")]
    [Range(1000, 500000)]
    public int spawnAttempts = 50000;

    [Header("Scale & Rotation")]
    public float minScale = 0.8f;
    public float maxScale = 1.3f;
    [Tooltip("Base rotation applied to every instance BEFORE the random Y spin. Use this to fix sideways FBX models. e.g. set X = -90 or X = 90.")]
    public Vector3 baseRotationOffset = Vector3.zero;

    [Header("Height & Slope Filters")]
    [Tooltip("Set these to slightly below the lowest grass on your island.")]
    public float forestHeightMin = -10f;
    [Tooltip("Set this to just below the snow line on your island.")]
    public float forestHeightMax = 200f;
    [Tooltip("Normal.y dot product threshold. Lower = steeper slopes allowed.")]
    [Range(0.1f, 1.0f)]
    public float cliffSlopeThreshold = 0.5f;

    [Header("Shader Forest Noise Sync")]
    [Tooltip("Match this to _ForestNoiseScale in the IslandShader material.")]
    public float forestNoiseScale = 0.05f;
    [Tooltip("Lower = more trees. Raise = only spawn in dense forest blobs. Try 0.2 first!")]
    [Range(0f, 1f)]
    public float forestNoiseThreshold = 0.2f;
    [Tooltip("Feathers the hard edge of the forest zone. Ramp from 0 (hard) to 0.3 (soft cloud-like edges).")]
    [Range(0f, 0.4f)]
    public float forestEdgeSoftness = 0.1f;

    [Header("Biome Variation")]
    [Tooltip("Large-scale second noise layer that thins out forest in patches, giving organic biome look.")]
    [Range(0f, 1f)]
    public float biomeVariationStrength = 0.3f;
    [Tooltip("Scale of the biome variation. Smaller = larger biome patches.")]
    public float biomeVariationScale = 0.008f;

    [Header("Minimum Spacing")]
    [Tooltip("Minimum world-space distance between two clusters. Prevents ugly overlapping clumps.")]
    public float minimumSpacing = 1.5f;

    // ── C# translation of IslandShader HLSL VNoise ─────────────────────────
    public float Hash(Vector2 p)
    {
        float dot = p.x * 127.1f + p.y * 311.7f;
        float s = Mathf.Sin(dot % (Mathf.PI * 2f)) * 43758.5453f;
        return s - Mathf.Floor(s);
    }

    public float VNoise(Vector2 p)
    {
        Vector2 i = new Vector2(Mathf.Floor(p.x), Mathf.Floor(p.y));
        Vector2 f = new Vector2(p.x - i.x, p.y - i.y);
        Vector2 u = new Vector2(f.x * f.x * (3f - 2f * f.x), f.y * f.y * (3f - 2f * f.y));
        return Mathf.Lerp(
            Mathf.Lerp(Hash(i), Hash(i + new Vector2(1, 0)), u.x),
            Mathf.Lerp(Hash(i + new Vector2(0, 1)), Hash(i + new Vector2(1, 1)), u.x),
            u.y);
    }

    public bool IsValidPlacement(RaycastHit hit)
    {
        // 1. Height
        if (hit.point.y < forestHeightMin || hit.point.y > forestHeightMax) return false;

        // 2. Slope
        if (Vector3.Dot(hit.normal, Vector3.up) < cliffSlopeThreshold) return false;

        // 3. Forest noise zone (matches IslandShader painted canopy)
        float n1 = VNoise(new Vector2(hit.point.x, hit.point.z) * forestNoiseScale);
        float n2 = VNoise(new Vector2(hit.point.x, hit.point.z) * forestNoiseScale * 3.1f + new Vector2(7.3f, 7.3f));
        float forestRaw = n1 - n2 * 0.35f;

        // Edge softness: instead of hard cutoff, ramp probability near the threshold
        float edge = Mathf.Max(forestEdgeSoftness, 0.001f);
        float forestProb = Mathf.InverseLerp(forestNoiseThreshold - edge, forestNoiseThreshold + edge, forestRaw);
        if (Random.value > forestProb) return false;

        // 4. Biome variation (large-scale organic density shift)
        float biome = VNoise(new Vector2(hit.point.x, hit.point.z) * biomeVariationScale);
        if (Random.value > Mathf.Lerp(1f, biome, biomeVariationStrength)) return false;

        // 5. Exclusion Mask (UV-based)
        if (exclusionMask != null)
        {
            float maskValue = exclusionMask.GetPixelBilinear(hit.textureCoord.x, hit.textureCoord.y).r;
            if (maskValue < 0.5f) return false;
        }

        return true;
    }

    // ── Spatial grid hash for minimum-spacing rejection ─────────────────────
    public class GridHash
    {
        private readonly float cellSize;
        private readonly System.Collections.Generic.Dictionary<long, System.Collections.Generic.List<Vector3>> cells
            = new System.Collections.Generic.Dictionary<long, System.Collections.Generic.List<Vector3>>();

        public GridHash(float spacing) { cellSize = Mathf.Max(spacing, 0.01f); }

        private long Key(int x, int z) => ((long)(x + 100000) << 32) | (uint)(z + 100000);

        public bool IsTooClose(Vector3 pos, float minDist)
        {
            int cx = Mathf.FloorToInt(pos.x / cellSize);
            int cz = Mathf.FloorToInt(pos.z / cellSize);
            float minSqr = minDist * minDist;
            for (int dx = -2; dx <= 2; dx++)
            for (int dz = -2; dz <= 2; dz++)
            {
                if (!cells.TryGetValue(Key(cx + dx, cz + dz), out var pts)) continue;
                foreach (Vector3 p in pts)
                    if ((p - pos).sqrMagnitude < minSqr) return true;
            }
            return false;
        }

        public void Add(Vector3 pos)
        {
            int cx = Mathf.FloorToInt(pos.x / cellSize);
            int cz = Mathf.FloorToInt(pos.z / cellSize);
            long key = Key(cx, cz);
            if (!cells.TryGetValue(key, out var list))
                cells[key] = list = new System.Collections.Generic.List<Vector3>();
            list.Add(pos);
        }
    }
}
