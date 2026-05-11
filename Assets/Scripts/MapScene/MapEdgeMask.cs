using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.MapScene
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class MapEdgeMask : MonoBehaviour
    {
        const string DefaultWaterPlaneName = "WaterPlane";

        static readonly int EdgeColorId = Shader.PropertyToID("_EdgeColor");
        static readonly int OpacityId = Shader.PropertyToID("_Opacity");
        static readonly int WorldCenterId = Shader.PropertyToID("_WorldCenter");
        static readonly int WorldRadiiId = Shader.PropertyToID("_WorldRadii");
        static readonly int NoiseStrengthId = Shader.PropertyToID("_NoiseStrength");
        static readonly int NoiseScaleId = Shader.PropertyToID("_NoiseScale");

        static Mesh sharedQuadMesh;
        static MaterialPropertyBlock sharedPropertyBlock;

        [Header("References")]
        [SerializeField] Renderer waterPlaneRenderer;
        [SerializeField] string waterPlaneName = DefaultWaterPlaneName;
        [SerializeField] Material maskMaterial;

        [Header("Shape")]
        [SerializeField] Vector2 ellipseScale = new Vector2(0.92f, 0.92f);
        [SerializeField, Min(0f)] float edgePadding = 40f;
        [SerializeField, Min(0.001f)] float featherWidth = 42f;
        [SerializeField] float heightOffset = 0.15f;

        [Header("Style")]
        [SerializeField] Color edgeColor = new Color(0.64f, 0.46f, 0.27f, 1f);
        [SerializeField, Range(0f, 1f)] float opacity = 0.78f;
        [SerializeField, Range(0f, 1f)] float noiseStrength = 0.16f;
        [SerializeField, Min(0.0001f)] float noiseScale = 0.025f;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;

        public Renderer WaterPlaneRenderer => waterPlaneRenderer;

        void Reset()
        {
            waterPlaneName = DefaultWaterPlaneName;
            TryAutoFindWaterPlane();
            RefreshMask();
        }

        void OnEnable()
        {
            RefreshMask();
        }

        void OnValidate()
        {
            ellipseScale.x = Mathf.Max(0.001f, ellipseScale.x);
            ellipseScale.y = Mathf.Max(0.001f, ellipseScale.y);
            edgePadding = Mathf.Max(0f, edgePadding);
            featherWidth = Mathf.Max(0.001f, featherWidth);
            opacity = Mathf.Clamp01(opacity);
            noiseStrength = Mathf.Clamp01(noiseStrength);
            noiseScale = Mathf.Max(0.0001f, noiseScale);

            RefreshMask();
        }

        void Update()
        {
            RefreshMask();
        }

        void RefreshMask()
        {
            EnsureComponents();

            if (waterPlaneRenderer == null)
            {
                TryAutoFindWaterPlane();
            }

            if (waterPlaneRenderer == null)
            {
                meshRenderer.enabled = false;
                return;
            }

            if (maskMaterial != null && meshRenderer.sharedMaterial != maskMaterial)
            {
                meshRenderer.sharedMaterial = maskMaterial;
            }

            if (meshRenderer.sharedMaterial == null)
            {
                meshRenderer.enabled = false;
                return;
            }

            Bounds waterBounds = waterPlaneRenderer.bounds;
            Vector2 waterExtents = new Vector2(
                Mathf.Max(0.001f, waterBounds.extents.x),
                Mathf.Max(0.001f, waterBounds.extents.z));
            Vector2 ellipseRadii = new Vector2(
                Mathf.Max(0.001f, waterExtents.x * ellipseScale.x),
                Mathf.Max(0.001f, waterExtents.y * ellipseScale.y));

            float halfWidth = Mathf.Max(waterExtents.x, ellipseRadii.x) + edgePadding;
            float halfDepth = Mathf.Max(waterExtents.y, ellipseRadii.y) + edgePadding;
            ApplyWorldTransform(waterBounds, halfWidth, halfDepth);

            if (!meshRenderer.enabled)
            {
                meshRenderer.enabled = true;
            }

            if (sharedPropertyBlock == null)
            {
                sharedPropertyBlock = new MaterialPropertyBlock();
            }
            sharedPropertyBlock.Clear();
            sharedPropertyBlock.SetColor(EdgeColorId, edgeColor);
            sharedPropertyBlock.SetFloat(OpacityId, opacity);
            sharedPropertyBlock.SetVector(WorldCenterId, new Vector4(waterBounds.center.x, waterBounds.center.z, 0f, 0f));
            sharedPropertyBlock.SetVector(WorldRadiiId, new Vector4(ellipseRadii.x, ellipseRadii.y, featherWidth, 0f));
            sharedPropertyBlock.SetFloat(NoiseStrengthId, noiseStrength);
            sharedPropertyBlock.SetFloat(NoiseScaleId, noiseScale);
            meshRenderer.SetPropertyBlock(sharedPropertyBlock);
        }

        void EnsureComponents()
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = GetSharedQuadMesh();
            }

            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            meshRenderer.lightProbeUsage = LightProbeUsage.Off;
            meshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            meshRenderer.allowOcclusionWhenDynamic = false;
        }

        void TryAutoFindWaterPlane()
        {
            string targetName = string.IsNullOrWhiteSpace(waterPlaneName)
                ? DefaultWaterPlaneName
                : waterPlaneName;
            GameObject waterPlane = GameObject.Find(targetName);
            if (waterPlane != null)
            {
                waterPlaneRenderer = waterPlane.GetComponent<Renderer>();
            }
        }

        void ApplyWorldTransform(Bounds waterBounds, float halfWidth, float halfDepth)
        {
            Vector3 targetPosition = new Vector3(
                waterBounds.center.x,
                waterBounds.max.y + heightOffset,
                waterBounds.center.z);
            Vector3 targetScale = new Vector3(halfWidth * 2f, 1f, halfDepth * 2f);

            if (!Approximately(transform.position, targetPosition))
            {
                transform.position = targetPosition;
            }

            if (transform.rotation != Quaternion.identity)
            {
                transform.rotation = Quaternion.identity;
            }

            if (!Approximately(transform.localScale, targetScale))
            {
                transform.localScale = targetScale;
            }
        }

        static bool Approximately(Vector3 a, Vector3 b)
        {
            return (a - b).sqrMagnitude < 0.000001f;
        }

        static Mesh GetSharedQuadMesh()
        {
            if (sharedQuadMesh != null)
            {
                return sharedQuadMesh;
            }

            sharedQuadMesh = new Mesh
            {
                name = "Map Edge Mask Quad",
                hideFlags = HideFlags.HideAndDontSave
            };
            sharedQuadMesh.vertices = new[]
            {
                new Vector3(-0.5f, 0f, -0.5f),
                new Vector3(-0.5f, 0f, 0.5f),
                new Vector3(0.5f, 0f, 0.5f),
                new Vector3(0.5f, 0f, -0.5f)
            };
            sharedQuadMesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f)
            };
            sharedQuadMesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            sharedQuadMesh.RecalculateBounds();
            sharedQuadMesh.RecalculateNormals();
            return sharedQuadMesh;
        }
    }
}
