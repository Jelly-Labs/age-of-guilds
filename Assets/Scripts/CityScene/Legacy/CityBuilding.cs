using UnityEngine;

namespace Assets.Scripts.CityScene
{
    [DisallowMultipleComponent]
    public class CityBuilding : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

#if UNITY_EDITOR
        const string SelectorBoundsMenuPath = "Tools/City/Show Building Selector Bounds";
        const string SelectorBoundsPrefKey = "AgeOfGuilds.City.ShowBuildingSelectorBounds";

        static bool ShowSelectorBounds => UnityEditor.EditorPrefs.GetBool(SelectorBoundsPrefKey, true);
#endif

        [SerializeField] CityBuildingDefinition definition;
        [SerializeField] Transform artRoot;
        [SerializeField] Transform focusTarget;
        [SerializeField] Transform iconAnchor;
        [SerializeField] Transform cameraView;
        [Range(20f, 70f)]
        [SerializeField] float cameraViewFov = 34f;
        [SerializeField] BoxCollider interactionCollider;
        [SerializeField] float hoverTintStrength = 0.16f;
        [SerializeField] float selectedTintStrength = 0.28f;
        [SerializeField] float highlightSmoothTime = 0.12f;

        Renderer[] renderers;
        Color[] baseColors;
        MaterialPropertyBlock propertyBlock;
        float currentHighlight;
        float highlightVelocity;
        bool isHovered;
        bool isSelected;

        public CityBuildingDefinition Definition => definition;
        public Transform FocusTarget => focusTarget != null ? focusTarget : transform;
        public Transform IconAnchor => iconAnchor != null ? iconAnchor : transform;
        public Transform CameraView => cameraView;
        public bool HasCameraView => cameraView != null;
        public float CameraViewFov
        {
            get
            {
                Camera markerCamera = cameraView != null ? cameraView.GetComponent<Camera>() : null;
                return markerCamera != null ? markerCamera.fieldOfView : cameraViewFov;
            }
        }
        public string DisplayName => definition != null ? definition.DisplayName : name;
        public Color AccentColor => definition != null ? definition.AccentColor : Color.white;
        public BoxCollider InteractionCollider => interactionCollider;

        void Awake()
        {
            CacheRenderers();
        }

        void OnEnable()
        {
            ApplyInstantState();
        }

        void Update()
        {
            float target = isSelected ? selectedTintStrength : isHovered ? hoverTintStrength : 0f;
            currentHighlight = Mathf.SmoothDamp(currentHighlight, target, ref highlightVelocity, highlightSmoothTime);
            ApplyHighlight(currentHighlight);
        }

        public void BindDefinition(CityBuildingDefinition buildingDefinition)
        {
            definition = buildingDefinition;
        }

        public void SetReferences(
            Transform newArtRoot,
            Transform newFocusTarget,
            Transform newIconAnchor,
            Transform newCameraView,
            float newCameraViewFov,
            BoxCollider newCollider)
        {
            artRoot = newArtRoot;
            focusTarget = newFocusTarget;
            iconAnchor = newIconAnchor;
            cameraView = newCameraView;
            cameraViewFov = newCameraViewFov;
            interactionCollider = newCollider;
            CacheRenderers();
            ApplyInstantState();
        }

        public void SetHovered(bool hovered)
        {
            isHovered = hovered;
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
        }

        public Bounds GetWorldBounds()
        {
            CacheRenderers();

            if (renderers == null || renderers.Length == 0)
            {
                return new Bounds(transform.position, Vector3.one * 4f);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        public Vector3 GetFocusPoint()
        {
            return FocusTarget.position;
        }

#if UNITY_EDITOR
        [ContextMenu("Preview Camera View")]
        void PreviewCameraView()
        {
            Camera sceneCamera = Camera.main;
            if (sceneCamera == null || cameraView == null)
            {
                return;
            }

            UnityEditor.Undo.RecordObject(sceneCamera.transform, "Preview City Building Camera View");
            UnityEditor.Undo.RecordObject(sceneCamera, "Preview City Building Camera View");
            sceneCamera.transform.SetPositionAndRotation(cameraView.position, cameraView.rotation);
            sceneCamera.fieldOfView = CameraViewFov;
            UnityEditor.Selection.activeTransform = sceneCamera.transform;
            UnityEditor.SceneView.RepaintAll();
        }

        [ContextMenu("Bake Current Camera Into View")]
        void BakeCurrentCameraIntoView()
        {
            Camera sceneCamera = Camera.main;
            if (sceneCamera == null || cameraView == null)
            {
                return;
            }

            UnityEditor.Undo.RecordObject(cameraView, "Bake City Building Camera View");
            UnityEditor.Undo.RecordObject(this, "Bake City Building Camera View");
            cameraView.SetPositionAndRotation(sceneCamera.transform.position, sceneCamera.transform.rotation);
            cameraViewFov = sceneCamera.fieldOfView;
            Camera markerCamera = cameraView.GetComponent<Camera>();
            if (markerCamera != null)
            {
                UnityEditor.Undo.RecordObject(markerCamera, "Bake City Building Camera View");
                markerCamera.fieldOfView = cameraViewFov;
                UnityEditor.EditorUtility.SetDirty(markerCamera);
            }

            UnityEditor.EditorUtility.SetDirty(cameraView);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneView.RepaintAll();
        }
#endif

        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (ShowSelectorBounds)
            {
                DrawInteractionBounds(new Color(0.2f, 0.85f, 1f, 0.55f));
            }
#endif
        }

        void OnDrawGizmosSelected()
        {
            DrawInteractionBounds(new Color(1f, 0.82f, 0.28f, 0.95f));

            if (cameraView == null)
            {
                return;
            }

            Gizmos.color = new Color(0.32f, 0.78f, 1f, 0.95f);
            Gizmos.DrawWireSphere(cameraView.position, 1.2f);
            Gizmos.DrawLine(cameraView.position, GetFocusPoint());

            Matrix4x4 previous = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(cameraView.position, cameraView.rotation, Vector3.one);
            Gizmos.DrawFrustum(Vector3.zero, CameraViewFov, 18f, 1.5f, 16f / 9f);
            Gizmos.matrix = previous;
        }

        void DrawInteractionBounds(Color color)
        {
            BoxCollider selectorCollider = interactionCollider != null ? interactionCollider : GetComponent<BoxCollider>();
            if (selectorCollider == null || !selectorCollider.enabled)
            {
                return;
            }

            Matrix4x4 previous = Gizmos.matrix;
            Gizmos.matrix = selectorCollider.transform.localToWorldMatrix;
            Gizmos.color = color;
            Gizmos.DrawWireCube(selectorCollider.center, selectorCollider.size);
            Gizmos.matrix = previous;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem(SelectorBoundsMenuPath)]
        static void ToggleSelectorBounds()
        {
            bool nextValue = !ShowSelectorBounds;
            UnityEditor.EditorPrefs.SetBool(SelectorBoundsPrefKey, nextValue);
            UnityEditor.Menu.SetChecked(SelectorBoundsMenuPath, nextValue);
            UnityEditor.SceneView.RepaintAll();
        }

        [UnityEditor.MenuItem(SelectorBoundsMenuPath, true)]
        static bool ToggleSelectorBoundsValidate()
        {
            UnityEditor.Menu.SetChecked(SelectorBoundsMenuPath, ShowSelectorBounds);
            return true;
        }
#endif

        void ApplyInstantState()
        {
            currentHighlight = isSelected ? selectedTintStrength : isHovered ? hoverTintStrength : 0f;
            highlightVelocity = 0f;
            ApplyHighlight(currentHighlight);
        }

        void CacheRenderers()
        {
            Transform searchRoot = artRoot != null ? artRoot : transform;
            renderers = searchRoot.GetComponentsInChildren<Renderer>(true);
            baseColors = new Color[renderers.Length];

            for (int i = 0; i < renderers.Length; i++)
            {
                Material material = renderers[i].sharedMaterial;
                if (material != null && material.HasProperty(BaseColorId))
                {
                    baseColors[i] = material.GetColor(BaseColorId);
                }
                else if (material != null && material.HasProperty(ColorId))
                {
                    baseColors[i] = material.GetColor(ColorId);
                }
                else
                {
                    baseColors[i] = Color.white;
                }
            }
        }

        void ApplyHighlight(float amount)
        {
            if (renderers == null)
            {
                CacheRenderers();
            }

            if (propertyBlock == null)
            {
                propertyBlock = new MaterialPropertyBlock();
            }

            Color accent = AccentColor;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer targetRenderer = renderers[i];
                if (targetRenderer == null)
                {
                    continue;
                }

                Color baseColor = i < baseColors.Length ? baseColors[i] : Color.white;
                Color tinted = Color.Lerp(baseColor, accent, amount);
                Color emission = accent * Mathf.Clamp01(amount * 1.4f);
                emission.a = 1f;

                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorId, tinted);
                propertyBlock.SetColor(ColorId, tinted);
                propertyBlock.SetColor(EmissionColorId, emission);
                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}
