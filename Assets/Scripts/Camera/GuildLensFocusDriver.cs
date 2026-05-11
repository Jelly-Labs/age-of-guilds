using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class GuildLensFocusDriver : MonoBehaviour
{
    public readonly struct FocusState
    {
        public readonly bool Active;
        public readonly float Distance;
        public readonly Vector3 WorldPoint;
        public readonly Vector4 ScreenPoint;
        public readonly Vector2 ScreenRadii;
        public readonly Vector2 ScreenOffset;
        public readonly float ScreenRotation;
        public readonly float DepthContribution;
        public readonly float ScreenSoftness;
        public readonly float ScreenStrength;

        public FocusState(
            bool active,
            float distance,
            Vector3 worldPoint,
            Vector4 screenPoint,
            Vector2 screenRadii,
            Vector2 screenOffset,
            float screenRotation,
            float depthContribution,
            float screenSoftness,
            float screenStrength)
        {
            Active = active;
            Distance = distance;
            WorldPoint = worldPoint;
            ScreenPoint = screenPoint;
            ScreenRadii = screenRadii;
            ScreenOffset = screenOffset;
            ScreenRotation = screenRotation;
            DepthContribution = depthContribution;
            ScreenSoftness = screenSoftness;
            ScreenStrength = screenStrength;
        }
    }

    static readonly int FocusDistanceId = Shader.PropertyToID("_GuildLensFocusDistance");
    static readonly int FocusDriverActiveId = Shader.PropertyToID("_GuildLensFocusDriverActive");
    static readonly int FocusWorldPointId = Shader.PropertyToID("_GuildLensFocusWorldPoint");
    static readonly int FocusScreenPointId = Shader.PropertyToID("_GuildLensFocusScreenPoint");
    static readonly int FocusEllipseId = Shader.PropertyToID("_GuildLensFocusEllipse");
    static readonly int FocusScreenSoftnessId = Shader.PropertyToID("_GuildLensFocusScreenSoftness");

    [SerializeField] Transform focusTarget = null;
    [SerializeField] CameraOrbitController orbitFallback;
    [SerializeField, Min(0.01f)] float focusDistanceFallback = 55f;
    [SerializeField, Min(0f)] float focusSmoothing = 8f;
    [SerializeField] float focusHeightOffset = 0f;
    [SerializeField] bool useScreenFocus = true;
    [FormerlySerializedAs("screenFocusRadius")]
    [SerializeField, Min(0.001f)] float screenFocusRadiusX = 0.65f;
    [SerializeField, Min(0.001f)] float screenFocusRadiusY = 0.34f;
    [SerializeField] Vector2 screenFocusOffset = Vector2.zero;
    [SerializeField] float screenFocusRotation = 0f;
    [SerializeField, Range(0f, 1f)] float depthFocusContribution = 0.2f;
    [SerializeField, Range(0f, 1f)] float screenFocusSoftness = 0.18f;
    [SerializeField, Range(0f, 1f)] float screenFocusStrength = 1f;

    Camera targetCamera;
    float currentFocusDistance;
    Vector2 currentFocusScreenPoint = new Vector2(0.5f, 0.5f);
    bool hasFocusDistance;
    bool hasFocusScreenPoint;

    public static FocusState CurrentFocus { get; private set; } = new FocusState(
        false,
        55f,
        Vector3.zero,
        new Vector4(0.5f, 0.5f, 0f, 0f),
        new Vector2(0.65f, 0.34f),
        Vector2.zero,
        0f,
        0.2f,
        0.18f,
        0f);

    void OnEnable()
    {
        ResolveReferences();
        currentFocusDistance = Mathf.Max(0.01f, focusDistanceFallback);
        currentFocusScreenPoint = new Vector2(0.5f, 0.5f);
        hasFocusDistance = false;
        hasFocusScreenPoint = false;
        PushFocus();
    }

    void LateUpdate()
    {
        PushFocus();
    }

    void OnValidate()
    {
        focusDistanceFallback = Mathf.Max(0.01f, focusDistanceFallback);
        focusSmoothing = Mathf.Max(0f, focusSmoothing);
        screenFocusRadiusX = Mathf.Max(0.001f, screenFocusRadiusX);
        screenFocusRadiusY = Mathf.Max(0.001f, screenFocusRadiusY);
        depthFocusContribution = Mathf.Clamp01(depthFocusContribution);
        screenFocusSoftness = Mathf.Clamp01(screenFocusSoftness);
        screenFocusStrength = Mathf.Clamp01(screenFocusStrength);

        if (isActiveAndEnabled)
        {
            PushFocus();
        }
    }

    void OnDisable()
    {
        Shader.SetGlobalFloat(FocusDriverActiveId, 0f);
        Shader.SetGlobalVector(FocusScreenPointId, new Vector4(0.5f, 0.5f, 0f, 0f));
        CurrentFocus = new FocusState(
            false,
            currentFocusDistance,
            Vector3.zero,
            new Vector4(0.5f, 0.5f, 0f, 0f),
            new Vector2(screenFocusRadiusX, screenFocusRadiusY),
            screenFocusOffset,
            screenFocusRotation * Mathf.Deg2Rad,
            depthFocusContribution,
            screenFocusSoftness,
            0f);
    }

    void ResolveReferences()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }

        if (orbitFallback == null)
        {
            orbitFallback = GetComponent<CameraOrbitController>();
        }

        if (orbitFallback == null)
        {
            orbitFallback = FindAnyObjectByType<CameraOrbitController>();
        }
    }

    void PushFocus()
    {
        ResolveReferences();
        if (targetCamera == null)
        {
            return;
        }

        Vector3 focusPoint;
        float targetDistance = ResolveFocusDistance(out focusPoint);
        Vector3 viewportPoint = targetCamera.WorldToViewportPoint(focusPoint);
        Vector2 targetScreenPoint = new Vector2(viewportPoint.x, viewportPoint.y);
        bool screenPointUsable = viewportPoint.z > targetCamera.nearClipPlane;

        if (!hasFocusDistance || !hasFocusScreenPoint || !Application.isPlaying || focusSmoothing <= 0f)
        {
            currentFocusDistance = targetDistance;
            currentFocusScreenPoint = targetScreenPoint;
            hasFocusDistance = true;
            hasFocusScreenPoint = true;
        }
        else
        {
            float blend = 1f - Mathf.Exp(-focusSmoothing * Time.deltaTime);
            currentFocusDistance = Mathf.Lerp(currentFocusDistance, targetDistance, blend);
            currentFocusScreenPoint = Vector2.Lerp(currentFocusScreenPoint, targetScreenPoint, blend);
        }

        float activeScreenStrength = useScreenFocus && screenPointUsable ? screenFocusStrength : 0f;
        Vector2 focusRadii = new Vector2(screenFocusRadiusX, screenFocusRadiusY);
        Vector2 offsetScreenPoint = currentFocusScreenPoint + screenFocusOffset;
        float focusRotationRadians = screenFocusRotation * Mathf.Deg2Rad;
        Shader.SetGlobalFloat(FocusDistanceId, currentFocusDistance);
        Shader.SetGlobalFloat(FocusDriverActiveId, 1f);
        Shader.SetGlobalVector(FocusWorldPointId, focusPoint);
        Shader.SetGlobalVector(FocusScreenPointId, new Vector4(
            offsetScreenPoint.x,
            offsetScreenPoint.y,
            screenPointUsable ? 1f : 0f,
            activeScreenStrength));
        Shader.SetGlobalVector(FocusEllipseId, new Vector4(
            focusRadii.x,
            focusRadii.y,
            focusRotationRadians,
            depthFocusContribution));
        Shader.SetGlobalFloat(FocusScreenSoftnessId, screenFocusSoftness);
        CurrentFocus = new FocusState(
            true,
            currentFocusDistance,
            focusPoint,
            new Vector4(
                offsetScreenPoint.x,
                offsetScreenPoint.y,
                screenPointUsable ? 1f : 0f,
                activeScreenStrength),
            focusRadii,
            screenFocusOffset,
            focusRotationRadians,
            depthFocusContribution,
            screenFocusSoftness,
            useScreenFocus ? screenFocusStrength : 0f);
    }

    float ResolveFocusDistance(out Vector3 focusPoint)
    {
        if (focusTarget != null)
        {
            focusPoint = focusTarget.position;
        }
        else if (orbitFallback != null)
        {
            focusPoint = orbitFallback.FocusPoint;
        }
        else
        {
            focusPoint = targetCamera.transform.position + targetCamera.transform.forward * focusDistanceFallback;
        }

        focusPoint.y += focusHeightOffset;
        float cameraSpaceDistance = Vector3.Dot(
            focusPoint - targetCamera.transform.position,
            targetCamera.transform.forward);

        if (cameraSpaceDistance > targetCamera.nearClipPlane)
        {
            return cameraSpaceDistance;
        }

        focusPoint = targetCamera.transform.position + targetCamera.transform.forward * focusDistanceFallback;
        return Mathf.Max(0.01f, focusDistanceFallback);
    }
}
