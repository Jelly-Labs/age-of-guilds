using System;
using UnityEngine;

namespace Assets.Scripts.MapScene
{
    [ExecuteAlways]
    public class IslandRoadAnchorBinder : MonoBehaviour
    {
        const int MaxAnchors = 8;

        static readonly int RoadAnchorCountId = Shader.PropertyToID("_RoadAnchorCount");
        static readonly int RoadStrengthId = Shader.PropertyToID("_RoadStrength");
        static readonly int[] RoadAnchorIds =
        {
            Shader.PropertyToID("_RoadAnchor0"),
            Shader.PropertyToID("_RoadAnchor1"),
            Shader.PropertyToID("_RoadAnchor2"),
            Shader.PropertyToID("_RoadAnchor3"),
            Shader.PropertyToID("_RoadAnchor4"),
            Shader.PropertyToID("_RoadAnchor5"),
            Shader.PropertyToID("_RoadAnchor6"),
            Shader.PropertyToID("_RoadAnchor7")
        };

        [Header("Island Renderers")]
        public Renderer[] islandRenderers = Array.Empty<Renderer>();
        public bool autoFindIslandRenderers = true;

        [Header("Road Anchors")]
        public bool autoFindTownObjects = true;
        public Transform[] manualAnchors = Array.Empty<Transform>();
        public float defaultAnchorRadius = 24f;
        [Range(0f, 1f)]
        public float defaultAnchorStrength = 1f;
        public bool updateInPlayMode = true;

        readonly Vector4[] anchorBuffer = new Vector4[MaxAnchors];
        Renderer[] cachedAutoRenderers = Array.Empty<Renderer>();
        MaterialPropertyBlock propertyBlock;

        void OnEnable()
        {
            RefreshAutoIslandRenderers();
            ApplyRoadAnchors();
        }

        void OnValidate()
        {
            defaultAnchorRadius = Mathf.Max(0.01f, defaultAnchorRadius);
            defaultAnchorStrength = Mathf.Clamp01(defaultAnchorStrength);
            RefreshAutoIslandRenderers();
            ApplyRoadAnchors();
        }

        void Update()
        {
            if (Application.isPlaying && !updateInPlayMode)
                return;

            ApplyRoadAnchors();
        }

        [ContextMenu("Apply Road Anchors")]
        public void ApplyRoadAnchors()
        {
            int anchorCount = BuildAnchorBuffer();
            ApplyAnchorsToRenderers(anchorCount);
        }

        [ContextMenu("Refresh Auto Island Renderers")]
        public void RefreshAutoIslandRenderers()
        {
            if (!autoFindIslandRenderers)
            {
                cachedAutoRenderers = Array.Empty<Renderer>();
                return;
            }

            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude);
            int writeIndex = 0;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (UsesRoadShader(renderers[i]))
                    renderers[writeIndex++] = renderers[i];
            }

            if (writeIndex == renderers.Length)
            {
                cachedAutoRenderers = renderers;
                return;
            }

            cachedAutoRenderers = new Renderer[writeIndex];
            Array.Copy(renderers, cachedAutoRenderers, writeIndex);
        }

        int BuildAnchorBuffer()
        {
            Array.Clear(anchorBuffer, 0, anchorBuffer.Length);
            int count = 0;

            if (autoFindTownObjects)
            {
                TownObject[] towns = FindObjectsByType<TownObject>(FindObjectsInactive.Exclude);
                for (int i = 0; i < towns.Length && count < MaxAnchors; i++)
                    AddAnchor(towns[i] != null ? towns[i].transform : null, ref count);
            }

            if (manualAnchors != null)
            {
                for (int i = 0; i < manualAnchors.Length && count < MaxAnchors; i++)
                    AddAnchor(manualAnchors[i], ref count);
            }

            return count;
        }

        void AddAnchor(Transform anchor, ref int count)
        {
            if (anchor == null)
                return;

            Vector3 position = anchor.position;
            anchorBuffer[count] = new Vector4(
                position.x,
                position.z,
                Mathf.Max(defaultAnchorRadius, 0.01f),
                Mathf.Clamp01(defaultAnchorStrength));
            count++;
        }

        void ApplyAnchorsToRenderers(int anchorCount)
        {
            Renderer[] targets = ResolveTargetRenderers();
            if (targets == null || targets.Length == 0)
                return;

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            for (int i = 0; i < targets.Length; i++)
            {
                Renderer target = targets[i];
                if (target == null)
                    continue;

                propertyBlock.Clear();
                target.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(RoadAnchorCountId, anchorCount);

                for (int anchorIndex = 0; anchorIndex < MaxAnchors; anchorIndex++)
                    propertyBlock.SetVector(RoadAnchorIds[anchorIndex], anchorBuffer[anchorIndex]);

                target.SetPropertyBlock(propertyBlock);
            }
        }

        Renderer[] ResolveTargetRenderers()
        {
            if (islandRenderers != null && islandRenderers.Length > 0)
                return islandRenderers;

            if (autoFindIslandRenderers)
            {
                if (cachedAutoRenderers == null || cachedAutoRenderers.Length == 0)
                    RefreshAutoIslandRenderers();

                return cachedAutoRenderers;
            }

            return Array.Empty<Renderer>();
        }

        static bool UsesRoadShader(Renderer renderer)
        {
            if (renderer == null)
                return false;

            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material != null && material.HasProperty(RoadStrengthId))
                    return true;
            }

            return false;
        }
    }
}
