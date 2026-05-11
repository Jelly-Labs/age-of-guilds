using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SDFBaker : EditorWindow
{
    private float worldSizeX = 1000f;
    private Transform boundsTarget;
    private int resolution = 4096;
    private float raycastHeight = 100f;
    private float seaLevelY = 0f;
    private float maxDist = 100f;
    private bool flipX = false;
    private bool flipY = true;

    // Explicit island list — only these GameObjects get temp colliders.
    // Avoids false positives from sea mist, cloud, and atmosphere planes.
    private List<GameObject> islandObjects = new List<GameObject>();

    // Objects to hide during bake (clouds, atmosphere, sea mist, etc.)
    // They are automatically re-enabled after baking.
    private List<GameObject> hideObjects = new List<GameObject>();

    [MenuItem("Tools/Globe/Coastal SDF Baker Window")]
    public static void ShowWindow()
    {
        GetWindow<SDFBaker>("SDF Baker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene-Based SDF Baker", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Assign your Water Plane here. The tool extracts its exact size and world position so the generated flow map matches its UV mapping 1:1.", MessageType.Info);
        boundsTarget = (Transform)EditorGUILayout.ObjectField("Water Plane (Target)", boundsTarget, typeof(Transform), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Island Objects", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Drag each island GameObject here. ONLY these objects get temporary colliders for raycasting — avoids false hits from clouds, mist, and sky planes.", MessageType.Info);

        if (islandObjects == null) islandObjects = new List<GameObject>();
        for (int i = 0; i < islandObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            islandObjects[i] = (GameObject)EditorGUILayout.ObjectField($"Island {i + 1}", islandObjects[i], typeof(GameObject), true);
            if (GUILayout.Button("✕", GUILayout.Width(24))) { islandObjects.RemoveAt(i); break; }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("+ Add Island")) islandObjects.Add(null);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Hide During Bake", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Drag clouds, sea mist, atmosphere, or any object that blocks raycasts. They will be temporarily hidden during the bake and re-enabled after.", MessageType.None);

        if (hideObjects == null) hideObjects = new List<GameObject>();
        for (int i = 0; i < hideObjects.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            hideObjects[i] = (GameObject)EditorGUILayout.ObjectField($"Hide {i + 1}", hideObjects[i], typeof(GameObject), true);
            if (GUILayout.Button("✕", GUILayout.Width(24))) { hideObjects.RemoveAt(i); break; }
            EditorGUILayout.EndHorizontal();
        }
        if (GUILayout.Button("+ Add Object to Hide")) hideObjects.Add(null);

        EditorGUILayout.Space();
        resolution       = EditorGUILayout.IntField("Texture Resolution",    resolution);
        raycastHeight    = EditorGUILayout.FloatField("Raycast Start Height", raycastHeight);
        seaLevelY        = EditorGUILayout.FloatField("Sea Level Y",          seaLevelY);
        maxDist          = EditorGUILayout.FloatField("Max Dist Normalize",   maxDist);
        flipX            = EditorGUILayout.Toggle("Flip X Axis",              flipX);
        flipY            = EditorGUILayout.Toggle("Flip Y Axis",              flipY);

        EditorGUILayout.Space();
        if (GUILayout.Button("Bake SDF from Bounds Target", GUILayout.Height(36)))
        {
            if (boundsTarget == null) { Debug.LogError("SDF Baker: Please assign the Water Plane!"); return; }
            Renderer r = boundsTarget.GetComponent<Renderer>();
            if (r == null) { Debug.LogError("SDF Baker: Target must have a MeshRenderer!"); return; }
            if (islandObjects.Count == 0 || islandObjects.TrueForAll(o => o == null))
            { Debug.LogError("SDF Baker: Add at least one island GameObject to the list!"); return; }

            Bounds b = r.bounds;
            BakeMapFromScene(centerX: b.center.x, centerZ: b.center.z,
                             sizeX:   b.size.x,   sizeZ:   b.size.z,
                             islandRoots: islandObjects,
                             hideList:     hideObjects,
                             res: resolution, topY: raycastHeight,
                             seaLevel: seaLevelY, distanceCap: maxDist,
                             flipX: flipX, flipY: flipY);
        }
    }

    public static void BakeMapFromScene(float centerX, float centerZ, float sizeX, float sizeZ,
                                        List<GameObject> islandRoots,
                                        List<GameObject> hideList,
                                        int res, float topY, float seaLevel, float distanceCap,
                                        bool flipX, bool flipY)
    {
        int width = res;
        int height = res;
        
        int[] landSeeds = new int[width * height];
        int[] waterSeeds = new int[width * height];
        bool[] isLand = new bool[width * height];
        
        Debug.Log($"Starting Scene Raycast across {sizeX}x{sizeZ} centered at ({centerX}, {centerZ})...");

        // --- HIDE INTERFERING OBJECTS ---
        // Disable clouds, atmosphere, sea mist etc. so they don't intercept raycasts.
        // Track original active state so we can restore exactly what was on/off.
        var hiddenStates = new System.Collections.Generic.Dictionary<GameObject, bool>();
        if (hideList != null)
        {
            foreach (var obj in hideList)
            {
                if (obj == null) continue;
                hiddenStates[obj] = obj.activeSelf;
                obj.SetActive(false);
            }
        }

        // --- TEMPORARY COLLIDER GENERATION ---
        // Only adds MeshColliders to the user-specified island GameObjects (and their children).
        // This avoids false hits from clouds, sea mist, atmosphere planes, etc.
        List<Collider> tempColliders = new List<Collider>();
        foreach (var root in islandRoots)
        {
            if (root == null) continue;
            foreach (var rend in root.GetComponentsInChildren<MeshRenderer>(includeInactive: false))
            {
                if (rend.GetComponent<Collider>() == null)
                {
                    MeshCollider mc = rend.gameObject.AddComponent<MeshCollider>();
                    tempColliders.Add(mc);
                }
            }
        }
        
        // Raycasting MUST happen on main thread
        float startX = centerX - sizeX / 2f;
        float startZ = centerZ - sizeZ / 2f;
        float stepX = sizeX / width;
        float stepZ = sizeZ / height;
        
        int landCount = 0;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                Vector3 origin = new Vector3(startX + x * stepX, topY, startZ + y * stepZ);
                
                // Raycast downwards
                bool hitLand = false;
                RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, topY * 2f);
                foreach (var hit in hits)
                {
                    if (hit.collider.isTrigger) continue;
                    
                    // The core fix: anything hit MUST be physically above sea level to count as land!
                    // This naturally ignores the water plane or the flat seabed.
                    if (hit.point.y > seaLevel + 0.05f)
                    {
                        hitLand = true;
                        break;
                    }
                }
                
                isLand[i] = hitLand;
                if (hitLand)
                {
                    landSeeds[i] = i;
                    waterSeeds[i] = -1;
                    landCount++;
                }
                else
                {
                    landSeeds[i] = -1;
                    waterSeeds[i] = i;
                }
            }
        }
        
        // --- CLEANUP TEMPORARY COLLIDERS ---
        foreach (var c in tempColliders)
        {
            if (c != null)
                DestroyImmediate(c);
        }
        tempColliders.Clear();

        // --- RESTORE HIDDEN OBJECTS ---
        foreach (var kvp in hiddenStates)
            if (kvp.Key != null) kvp.Key.SetActive(kvp.Value);

        int waterCount = width * height - landCount;
        Debug.Log($"Raycast finished. Found {landCount} land pixels and {waterCount} water pixels. Starting Jump Flood...");
        Stopwatch sw = Stopwatch.StartNew();

        landSeeds = JumpFlood(landSeeds, width, height);
        waterSeeds = JumpFlood(waterSeeds, width, height);

        sw.Stop();
        Debug.Log($"Algorithm took {sw.ElapsedMilliseconds}ms. Writing map...");

        Texture2D exportTex = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
        Texture2D shorelineTex = new Texture2D(width, height, TextureFormat.RGBAFloat, false, true);
        Color[] exportPixels = new Color[width * height];
        float[] landInteriorDistances = new float[width * height];

        // Use pixel-to-world scalar
        float pixelToWorld = sizeX / width;
        float safeDistanceCap = Mathf.Max(distanceCap, 0.001f);

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                int exportX = flipX ? (width - 1) - x : x;
                int exportY = flipY ? (height - 1) - y : y;

                int seed = landSeeds[i];
                if (seed == -1)
                {
                    exportPixels[exportY * width + exportX] = new Color(0.5f, 0.5f, 1f, 0f);
                    continue;
                }

                int sx = seed % width;
                int sy = seed / width;

                float dx = sx - x;
                float dy = sy - y;

                Vector2 dir = new Vector2(dx, dy).normalized;
                
                // Invert the flow direction channel if we are flipping that axis
                if (flipX) dir.x *= -1f;
                if (flipY) dir.y *= -1f;

                float r = dir.x * 0.5f + 0.5f;
                float g = dir.y * 0.5f + 0.5f;
                
                float actualDistWorld = Mathf.Sqrt(dx * dx + dy * dy) * pixelToWorld;
                
                // Blue remains the water-side distance-to-land channel used by WaterShader.
                float b = Mathf.Clamp01(actualDistWorld / safeDistanceCap);
                
                // Pure black explicitly where island is
                if (isLand[i]) b = 0f;

                // Alpha is the land-side distance-to-water channel used by IslandShader.
                // Water pixels stay 0, while land pixels increase inland from the shoreline.
                float a = 0f;
                int waterSeed = waterSeeds[i];
                if (isLand[i] && waterSeed != -1)
                {
                    float landInteriorDistWorld = Mathf.Sqrt(CalculateDistSqr(x, y, waterSeed, width)) * pixelToWorld;
                    landInteriorDistances[i] = landInteriorDistWorld;
                    a = Mathf.Clamp01(landInteriorDistWorld / safeDistanceCap);
                }

                exportPixels[exportY * width + exportX] = new Color(r, g, b, a);
            }
        });

        float maxLandInteriorDistance = 0f;
        for (int i = 0; i < landInteriorDistances.Length; i++)
            maxLandInteriorDistance = Mathf.Max(maxLandInteriorDistance, landInteriorDistances[i]);

        Texture2D previewTex = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
        Color[] previewPixels = new Color[width * height];
        float previewDistanceScale = Mathf.Max(maxLandInteriorDistance, 0.001f);

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * width + x;
                int exportX = flipX ? (width - 1) - x : x;
                int exportY = flipY ? (height - 1) - y : y;
                int exportIndex = exportY * width + exportX;

                float preview = isLand[i] ? Mathf.Clamp01(landInteriorDistances[i] / previewDistanceScale) : 0f;
                previewPixels[exportIndex] = new Color(preview, preview, preview, 1f);

                if (isLand[i])
                {
                    Color productionPixel = exportPixels[exportIndex];
                    productionPixel.r = preview;
                    productionPixel.g = preview;
                    productionPixel.b = 0f;
                    exportPixels[exportIndex] = productionPixel;
                }
            }
        });

        exportTex.SetPixels(exportPixels);
        exportTex.Apply();
        shorelineTex.SetPixelData(exportPixels, 0);
        shorelineTex.Apply();
        previewTex.SetPixels(previewPixels);
        previewTex.Apply();

        string outPath = "Assets/Materials/Textures/CoastalFlowMap_AgeOfGuilds.png";
        string shorelineOutPath = "Assets/Materials/Textures/CoastalLandDistance_AgeOfGuilds.exr";
        string previewOutPath = "Assets/Materials/Textures/CoastalLandDistancePreview_AgeOfGuilds.png";
        
        string fullOutPath = Path.Combine(Application.dataPath, "Materials/Textures/CoastalFlowMap_AgeOfGuilds.png");
        string fullOutDir = Path.GetDirectoryName(fullOutPath);
        if (!Directory.Exists(fullOutDir)) {
            Directory.CreateDirectory(fullOutDir);
        }
        
        byte[] bytes = exportTex.EncodeToPNG();
        File.WriteAllBytes(fullOutPath, bytes);

        string fullShorelineOutPath = Path.Combine(Application.dataPath, "Materials/Textures/CoastalLandDistance_AgeOfGuilds.exr");
        byte[] shorelineBytes = shorelineTex.EncodeToEXR(Texture2D.EXRFlags.CompressZIP);
        File.WriteAllBytes(fullShorelineOutPath, shorelineBytes);

        string fullPreviewOutPath = Path.Combine(Application.dataPath, "Materials/Textures/CoastalLandDistancePreview_AgeOfGuilds.png");
        byte[] previewBytes = previewTex.EncodeToPNG();
        File.WriteAllBytes(fullPreviewOutPath, previewBytes);

        AssetDatabase.Refresh();

        ConfigureLinearUncompressedTexture(outPath);
        ConfigureLinearUncompressedTexture(shorelineOutPath);
        ConfigureLinearUncompressedTexture(previewOutPath);
        AssignShorelineDistanceMapToIslandMaterials(islandRoots, shorelineOutPath, distanceCap);

        int maxAlphaByte = Mathf.RoundToInt(Mathf.Clamp01(maxLandInteriorDistance / safeDistanceCap) * 255f);
        Debug.Log($"Successfully baked CoastalFlowMap_AgeOfGuilds.png and CoastalLandDistance_AgeOfGuilds.exr! B=water distance to land, A=land distance to water. Land pixels: {landCount}. Max inland distance: {maxLandInteriorDistance:F2} world units. Max PNG alpha: {maxAlphaByte}/255. Preview: {previewOutPath}");
    }

    static void ConfigureLinearUncompressedTexture(string assetPath)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        if (importer == null) return;

        importer.sRGBTexture = false;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 4096;
        importer.alphaIsTransparency = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    static void AssignShorelineDistanceMapToIslandMaterials(List<GameObject> islandRoots, string shorelineAssetPath, float distanceCap)
    {
        Texture2D shorelineMap = AssetDatabase.LoadAssetAtPath<Texture2D>(shorelineAssetPath);
        if (shorelineMap == null) return;

        foreach (var root in islandRoots)
        {
            if (root == null) continue;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(includeInactive: false))
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null || !material.HasProperty("_ShorelineDistanceMap")) continue;
                    material.SetTexture("_ShorelineDistanceMap", shorelineMap);
                    material.SetFloat("_ShorelineMapMaxDistance", distanceCap);
                    EditorUtility.SetDirty(material);
                }
            }
        }

        AssetDatabase.SaveAssets();
    }

    static int[] JumpFlood(int[] seeds, int width, int height)
    {
        int step = Mathf.Max(width, height) / 2;
        int[] nextSeeds = new int[width * height];

        while (step >= 1)
        {
            seeds.CopyTo(nextSeeds, 0);

            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    int i = y * width + x;
                    int bestSeed = seeds[i];
                    float bestDist = bestSeed == -1 ? float.MaxValue : CalculateDistSqr(x, y, bestSeed, width);

                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;

                            int nx = x + dx * step;
                            int ny = y + dy * step;

                            // Assuming flat map now, no spherical wrap
                            if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                            int ni = ny * width + nx;
                            int neighborSeed = seeds[ni];

                            if (neighborSeed != -1)
                            {
                                float dist = CalculateDistSqr(x, y, neighborSeed, width);
                                if (dist < bestDist)
                                {
                                    bestDist = dist;
                                    bestSeed = neighborSeed;
                                }
                            }
                        }
                    }

                    nextSeeds[i] = bestSeed;
                }
            });

            int[] temp = seeds;
            seeds = nextSeeds;
            nextSeeds = temp;

            step /= 2;
        }

        return seeds;
    }

    static float CalculateDistSqr(int x, int y, int seed, int width)
    {
        int sx = seed % width;
        int sy = seed / width;
        float dx = sx - x;
        float dy = sy - y;
        return dx * dx + dy * dy;
    }
}
