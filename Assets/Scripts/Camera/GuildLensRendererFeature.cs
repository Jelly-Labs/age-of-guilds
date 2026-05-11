using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public sealed class GuildLensRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public sealed class Settings
    {
        public bool effectEnabled = true;
        public Shader shader;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        [Tooltip("Objects on these layers are masked out of Guild Lens outline and oil smear only.")]
        public LayerMask outlineOilExclusionLayerMask = 1 << 4;

        [Min(0.01f)] public float focusDistanceFallback = 55f;
        [Min(0.01f)] public float focusRangeWidth = 18f;
        [Min(0f)] public float transitionSoftness = 10f;
        [Range(0f, 1f)] public float focusProtectionStrength = 1f;
        [Range(0.25f, 4f)] public float focusFalloffPower = 1.25f;
        [Tooltip("Normalized zoom where sailor-map reveal begins. Higher zoom values are farther from the map.")]
        [Range(0f, 1f)] public float mapRevealZoomStart = 0.72f;
        [Tooltip("Normalized zoom where sailor-map reveal reaches full strength. Lower zoom values are closer to the map.")]
        [Range(0f, 1f)] public float mapRevealZoomFull = 0.18f;
        [Min(0.001f)] public float mapRevealResponse = 1.25f;
        [Min(0f)] public float mapRevealSmoothing = 0.18f;
        [Range(0f, 1f)] public float mapRevealMotionBoost = 0.18f;

        [Range(0f, 1f)] public float globalStrength = 1f;
        [Range(0f, 1f)] public float worldClarity = 0.86f;
        [Range(0f, 1f)] public float nearStylizationStrength = 0.04f;
        [Range(0f, 1f)] public float farStylizationStrength = 0.55f;
        [Range(0f, 1f)] public float farHazeStrength = 0.45f;
        [Min(0.01f)] public float farHazeRange = 115f;
        [Min(0f)] public float oilSmearAmount = 1.35f;
        [Range(0f, 1f)] public float papyrusTextureStrength = 0.38f;
        [Range(0f, 1f)] public float paperVignetteStrength = 0.55f;
        [Range(0f, 1f)] public float paperVignetteRadius = 0.68f;
        [Range(0f, 1f)] public float ambientPaperVignetteStrength = 0.12f;
        [Range(0f, 1f)] public float horizonPaperStrength = 0.4f;
        [Range(0f, 1f)] public float inkEdgeDarkeningStrength = 0.24f;
        [Range(0f, 1f)] public float distanceColorFade = 0.18f;
        [Range(0f, 1f)] public float saturationPreserve = 0.9f;
        [Range(0f, 1f)] public float contrastPreserve = 0.9f;
        [Min(0.25f)] public float outlineWidthPixels = 1.15f;
        [Range(0f, 2f)] public float outlineStrength = 2f;
        public Color outlineColor = new Color(0.24f, 0.18f, 0.10f, 1f);
        [Range(0f, 1f)] public float outlineOpacity = 1f;
        [Range(0f, 2f)] public float outlineContrast = 0.9f;
        [Range(0.001f, 1f)] public float outlineEdgeSoftness = 0.45f;
        [Min(0f)] public float outlineAnimationSpeed = 1f;
        [Range(0f, 1f)] public float outlineRevealIntensity = 1f;
        [Range(0f, 1f)] public float outlineOrganicStrength = 0.9f;
        [Range(0f, 1f)] public float outlineRevealStrength = 0.85f;
        [Min(0.001f)] public float outlineNoiseScale = 32f;
        [Range(0.001f, 1f)] public float outlineRevealSoftness = 0.42f;
        [Range(0f, 1f)] public float outlineSettleStrength = 0.82f;
        [Range(0f, 1f)] public float outlineBleedStrength = 0.42f;
        [Min(0.25f)] public float outlineBleedWidthPixels = 2f;
        [Range(0.001f, 1f)] public float outlineBleedSoftness = 0.62f;
        [Range(0f, 1f)] public float outlineSegmentRandomness = 0.72f;
        [Range(0f, 1f)] public float outlineStrokeCrawlStrength = 0.65f;
        [Range(0f, 1f)] public float outlineLeadingEdgeStrength = 0.85f;
        [Range(0f, 1f)] public float outlineEdgePoolingStrength = 0.45f;
        public Color papyrusColor = new Color(0.86f, 0.75f, 0.54f, 1f);
        [Range(0f, 1f)] public float papyrusTintStrength = 0.42f;
        [Range(0f, 1f)] public float papyrusOverlayOpacity = 0.85f;
        [Range(0f, 1f)] public float papyrusSaturation = 0.72f;
        [Range(0f, 1f)] public float papyrusAgingDarkening = 0.22f;
        [Range(0f, 1f)] public float papyrusBlendStrength = 0.78f;
        [Range(0.001f, 1f)] public float papyrusEdgeSoftness = 0.55f;
        [Range(0f, 1f)] public float papyrusEdgeNoiseStrength = 0.55f;
        [Range(0f, 1f)] public float papyrusEdgeDarkening = 0.32f;
        [Min(0f)] public float papyrusEdgeAnimationSpeed = 0.85f;
        [Range(0f, 2f)] public float papyrusContrast = 0.75f;
        [Range(0f, 1f)] public float papyrusMaterialStrength = 1f;
        [Range(0f, 1f)] public float papyrusUnifyStrength = 0.78f;
        [Range(0f, 1f)] public float papyrusFiberStrength = 0.68f;
        [Range(0f, 1f)] public float papyrusStainStrength = 0.72f;
        [Range(0f, 1f)] public float papyrusBlueWashStrength = 0.38f;
        [Range(0f, 1f)] public float papyrusScrollFlowStrength = 0.55f;
        [Min(0f)] public float papyrusScrollWorldScale = 0.0015f;
        [Range(0f, 1f)] public float papyrusScrollMaterialBoost = 0.22f;
        [Range(0f, 1f)] public float inkWashStrength = 0.18f;
        public Color inkWashColor = new Color(0.18f, 0.13f, 0.08f, 1f);
        [Min(0.001f)] public float inkWashScale = 6f;
        [Range(0.001f, 1f)] public float inkWashSoftness = 0.55f;
        [Range(0f, 1f)] public float waterLensStrength = 0.65f;
        public Texture2D waterTexture;
        public bool waterTextureEnabled = true;
        [Range(0f, 1f)] public float waterTextureOpacity = 0.35f;
        [Min(0.0001f)] public float waterTextureScale = 0.012f;
        [Range(0f, 1f)] public float waterTextureDensity = 1f;
        public float waterTextureSeed = 11.3f;
        [Range(-180f, 180f)] public float waterTextureRotation = 0f;
        [Range(0f, 1f)] public float waterTextureRandomRotation = 0f;
        [Range(0f, 2f)] public float waterTextureContrast = 0.8f;
        [Range(0f, 2f)] public float waterTextureIntensity = 0.8f;
        public Texture2D journalTexture;
        public bool journalTextureEnabled = true;
        [Range(0f, 1f)] public float journalTextureOpacity = 0.16f;
        [Min(0.0001f)] public float journalTextureScale = 0.006f;
        [Range(0f, 1f)] public float journalTextureDensity = 0.18f;
        public float journalTextureSeed = 37.2f;
        [Range(-180f, 180f)] public float journalTextureRotation = -8f;
        [Range(0f, 1f)] public float journalTextureRandomRotation = 0.65f;
        [Range(0f, 2f)] public float journalTextureContrast = 0.75f;
        [Range(0f, 2f)] public float journalTextureIntensity = 0.55f;
        [Range(0f, 1f)] public float waterMotionSuppression = 1f;
        [Range(0f, 1f)] public float waterPapyrusOverrideStrength = 1f;
        [Range(0f, 1f)] public float waterPapyrusBlueStrength = 0.65f;
        [Range(0f, 1f)] public float waterPapyrusMatteSmoothness = 0.04f;
        [Range(0f, 1f)] public float cameraAngleEllipseStrength = 1f;
        [Range(0f, 90f)] public float ellipseAngleStart = 40f;
        [Range(0f, 90f)] public float ellipseAngleEnd = 75f;
        [Min(0.001f)] public float perspectiveRadiusX = 0.62f;
        [Min(0.001f)] public float perspectiveRadiusY = 0.34f;
        [Min(0.001f)] public float topDownRadiusX = 1.8f;
        [Min(0.001f)] public float topDownRadiusY = 1.25f;
        [Range(0f, 1f)] public float ellipseAngleSoftness = 0.5f;
        [Range(0f, 1f)] public float papyrusRevealStrength = 1f;
        [Min(0f)] public float papyrusRevealSpeed = 1.25f;
        [Min(0.001f)] public float papyrusRevealScale = 5f;
        [Range(0.001f, 1f)] public float papyrusRevealSoftness = 0.25f;
        [Range(0f, 1f)] public float cartoonStrength = 0.75f;
        [Min(0.25f)] public float cartoonSampleRadiusPixels = 1.35f;
        [Min(2f)] public float cartoonValueSteps = 5f;
        [Min(2f)] public float cartoonColorSteps = 6f;
        [Range(0f, 1f)] public float cartoonEdgePreserve = 0.65f;
        [Range(0f, 1f)] public float outlinePapyrusDependence = 0.45f;
        [Tooltip("How much outline is allowed inside the camera focus/protected area. 0 keeps focused targets clean, 1 outlines them normally.")]
        [Range(0f, 1f)] public float outlineFocusInfluence = 0.35f;
        [Tooltip("Adds outline only where water/reflection pixels meet non-water pixels, useful for shore and island silhouettes without outlining wave highlights.")]
        [Range(0f, 1f)] public float waterBoundaryOutlineStrength = 0.75f;
        [Tooltip("Width in pixels of the water/land boundary area allowed to receive outline.")]
        [Min(0.25f)] public float waterBoundaryOutlineWidthPixels = 1.15f;
        [Tooltip("How much the water-boundary outline can use its own width instead of the main outline width.")]
        [Range(0f, 1f)] public float waterBoundaryOutlineThicknessInfluence = 0f;
        [Tooltip("Where the water-boundary outline appears: 0 = land side, 0.5 = both sides, 1 = water side.")]
        [Range(0f, 1f)] public float waterBoundaryOutlineSide = 0.5f;
        [Range(0f, 1f)] public float oilPapyrusDependence = 0.25f;
        public bool miniatureFocusEnabled = true;
        [Range(0f, 1f)] public float miniatureFocusStrength = 0.28f;
        [Min(0.25f)] public float miniatureFocusRadiusPixels = 2.25f;
        [Range(0.001f, 1f)] public float miniatureFocusSoftness = 0.45f;
        [Range(0f, 1f)] public float miniatureFocusZoomInfluence = 0.65f;
        [Range(0f, 1f)] public float miniatureFocusPapyrusProtection = 0.4f;
        [Range(0f, 1f)] public float miniatureFocusEdgeBias = 0.25f;

        [Tooltip("Overall camera-motion sensitivity for the papyrus, outline, and oil trailing response.")]
        [Min(0f)] public float motionSensitivity = 1f;
        [Tooltip("Sensitivity to camera position speed.")]
        [Min(0f)] public float motionPositionSensitivity = 0.04f;
        [Tooltip("Sensitivity to camera rotation speed.")]
        [Min(0f)] public float motionRotationSensitivity = 0.015f;
        [Tooltip("Sensitivity to camera field-of-view changes.")]
        [Min(0f)] public float motionFovSensitivity = 0.05f;
        [Tooltip("Seconds of delayed chase before the Guild Lens motion response catches up.")]
        [Min(0f)] public float motionLagSeconds = 0.18f;
        [Tooltip("Smooths raw camera motion before the delayed response receives it.")]
        [Min(0f)] public float motionSmoothing = 0.12f;
        [Tooltip("How quickly the motion response grows when the camera starts moving.")]
        [Min(0f)] public float motionRiseSpeed = 4f;
        [Tooltip("How quickly the motion response settles when the camera stops moving.")]
        [Min(0f)] public float motionFallSpeed = 1.35f;
        [Range(0f, 1f)] public float maxMotionAmount = 1f;
        [Range(0f, 1f)] public float papyrusMotionInfluence = 0.45f;
        [Range(0f, 1f)] public float outlineMotionInfluence = 0.65f;
        [Range(0f, 1f)] public float oilMotionInfluence = 0.85f;
        [Tooltip("How quickly papyrus motion animation appears after camera movement.")]
        [Min(0f)] public float papyrusMotionRiseSpeed = 3f;
        [Tooltip("How slowly papyrus motion animation decays after camera movement. Lower values linger longer.")]
        [Min(0f)] public float papyrusMotionDecaySpeed = 0.8f;
        [Tooltip("How quickly outline motion emphasis appears after camera movement.")]
        [Min(0f)] public float outlineMotionRiseSpeed = 5f;
        [Tooltip("How slowly outline motion emphasis decays after camera movement. Lower values linger longer.")]
        [Min(0f)] public float outlineMotionDecaySpeed = 0.45f;
        [Tooltip("How quickly oil smear motion decays after camera movement. Lower values linger longer.")]
        [Min(0f)] public float oilMotionDecaySpeed = 0.75f;
        [Tooltip("Maximum screen-space UV drift for papyrus texture/wash motion.")]
        [Range(0f, 1f)] public float papyrusMotionDrift = 0.12f;
        [Tooltip("Multiplies camera movement before it becomes directional papyrus drift.")]
        [Min(0f)] public float papyrusMotionDriftSensitivity = 1.5f;
        [Tooltip("How quickly directional papyrus drift settles after camera movement. Lower values linger longer.")]
        [Min(0f)] public float papyrusMotionDriftDecaySpeed = 0.55f;
        [Range(0, 29)] public int debugMode = 0;
    }

    static readonly int FocusFallbackId = Shader.PropertyToID("_GuildLensFocusFallbackDistance");
    static readonly int FocusRangeId = Shader.PropertyToID("_GuildLensFocusRangeWidth");
    static readonly int TransitionSoftnessId = Shader.PropertyToID("_GuildLensTransitionSoftness");
    static readonly int FocusDistanceId = Shader.PropertyToID("_GuildLensFocusDistance");
    static readonly int FocusDriverActiveId = Shader.PropertyToID("_GuildLensFocusDriverActive");
    static readonly int FocusWorldPointId = Shader.PropertyToID("_GuildLensFocusWorldPoint");
    static readonly int FocusScreenPointId = Shader.PropertyToID("_GuildLensFocusScreenPoint");
    static readonly int FocusEllipseId = Shader.PropertyToID("_GuildLensFocusEllipse");
    static readonly int FocusScreenSoftnessId = Shader.PropertyToID("_GuildLensFocusScreenSoftness");
    static readonly int FocusProtectionStrengthId = Shader.PropertyToID("_GuildLensFocusProtectionStrength");
    static readonly int FocusFalloffPowerId = Shader.PropertyToID("_GuildLensFocusFalloffPower");
    static readonly int MapRevealId = Shader.PropertyToID("_GuildLensMapReveal");
    static readonly int GlobalStrengthId = Shader.PropertyToID("_GuildLensGlobalStrength");
    static readonly int WorldClarityId = Shader.PropertyToID("_GuildLensWorldClarity");
    static readonly int NearStrengthId = Shader.PropertyToID("_GuildLensNearStrength");
    static readonly int FarStrengthId = Shader.PropertyToID("_GuildLensFarStrength");
    static readonly int FarHazeStrengthId = Shader.PropertyToID("_GuildLensFarHazeStrength");
    static readonly int FarHazeRangeId = Shader.PropertyToID("_GuildLensFarHazeRange");
    static readonly int OilSmearId = Shader.PropertyToID("_GuildLensOilSmear");
    static readonly int PapyrusStrengthId = Shader.PropertyToID("_GuildLensPapyrusStrength");
    static readonly int PaperVignetteStrengthId = Shader.PropertyToID("_GuildLensPaperVignetteStrength");
    static readonly int PaperVignetteRadiusId = Shader.PropertyToID("_GuildLensPaperVignetteRadius");
    static readonly int AmbientPaperVignetteStrengthId = Shader.PropertyToID("_GuildLensAmbientPaperVignetteStrength");
    static readonly int HorizonPaperStrengthId = Shader.PropertyToID("_GuildLensHorizonPaperStrength");
    static readonly int InkStrengthId = Shader.PropertyToID("_GuildLensInkStrength");
    static readonly int DistanceFadeId = Shader.PropertyToID("_GuildLensDistanceFade");
    static readonly int SaturationPreserveId = Shader.PropertyToID("_GuildLensSaturationPreserve");
    static readonly int ContrastPreserveId = Shader.PropertyToID("_GuildLensContrastPreserve");
    static readonly int OutlineWidthId = Shader.PropertyToID("_GuildLensOutlineWidthPixels");
    static readonly int OutlineStrengthId = Shader.PropertyToID("_GuildLensOutlineStrength");
    static readonly int OutlineColorId = Shader.PropertyToID("_GuildLensOutlineColor");
    static readonly int OutlineOpacityId = Shader.PropertyToID("_GuildLensOutlineOpacity");
    static readonly int OutlineContrastId = Shader.PropertyToID("_GuildLensOutlineContrast");
    static readonly int OutlineEdgeSoftnessId = Shader.PropertyToID("_GuildLensOutlineEdgeSoftness");
    static readonly int OutlineAnimationSpeedId = Shader.PropertyToID("_GuildLensOutlineAnimationSpeed");
    static readonly int OutlineRevealIntensityId = Shader.PropertyToID("_GuildLensOutlineRevealIntensity");
    static readonly int OutlineOrganicStrengthId = Shader.PropertyToID("_GuildLensOutlineOrganicStrength");
    static readonly int OutlineRevealStrengthId = Shader.PropertyToID("_GuildLensOutlineRevealStrength");
    static readonly int OutlineNoiseScaleId = Shader.PropertyToID("_GuildLensOutlineNoiseScale");
    static readonly int OutlineRevealSoftnessId = Shader.PropertyToID("_GuildLensOutlineRevealSoftness");
    static readonly int OutlineSettleStrengthId = Shader.PropertyToID("_GuildLensOutlineSettleStrength");
    static readonly int OutlineBleedStrengthId = Shader.PropertyToID("_GuildLensOutlineBleedStrength");
    static readonly int OutlineBleedWidthId = Shader.PropertyToID("_GuildLensOutlineBleedWidthPixels");
    static readonly int OutlineBleedSoftnessId = Shader.PropertyToID("_GuildLensOutlineBleedSoftness");
    static readonly int OutlineSegmentRandomnessId = Shader.PropertyToID("_GuildLensOutlineSegmentRandomness");
    static readonly int OutlineStrokeCrawlStrengthId = Shader.PropertyToID("_GuildLensOutlineStrokeCrawlStrength");
    static readonly int OutlineLeadingEdgeStrengthId = Shader.PropertyToID("_GuildLensOutlineLeadingEdgeStrength");
    static readonly int OutlineEdgePoolingStrengthId = Shader.PropertyToID("_GuildLensOutlineEdgePoolingStrength");
    static readonly int PapyrusColorId = Shader.PropertyToID("_GuildLensPapyrusColor");
    static readonly int PapyrusTintStrengthId = Shader.PropertyToID("_GuildLensPapyrusTintStrength");
    static readonly int PapyrusOverlayOpacityId = Shader.PropertyToID("_GuildLensPapyrusOverlayOpacity");
    static readonly int PapyrusSaturationId = Shader.PropertyToID("_GuildLensPapyrusSaturation");
    static readonly int PapyrusAgingDarkeningId = Shader.PropertyToID("_GuildLensPapyrusAgingDarkening");
    static readonly int PapyrusBlendStrengthId = Shader.PropertyToID("_GuildLensPapyrusBlendStrength");
    static readonly int PapyrusEdgeSoftnessId = Shader.PropertyToID("_GuildLensPapyrusEdgeSoftness");
    static readonly int PapyrusEdgeNoiseStrengthId = Shader.PropertyToID("_GuildLensPapyrusEdgeNoiseStrength");
    static readonly int PapyrusEdgeDarkeningId = Shader.PropertyToID("_GuildLensPapyrusEdgeDarkening");
    static readonly int PapyrusEdgeAnimationSpeedId = Shader.PropertyToID("_GuildLensPapyrusEdgeAnimationSpeed");
    static readonly int PapyrusContrastId = Shader.PropertyToID("_GuildLensPapyrusContrast");
    static readonly int PapyrusMaterialStrengthId = Shader.PropertyToID("_GuildLensPapyrusMaterialStrength");
    static readonly int PapyrusUnifyStrengthId = Shader.PropertyToID("_GuildLensPapyrusUnifyStrength");
    static readonly int PapyrusFiberStrengthId = Shader.PropertyToID("_GuildLensPapyrusFiberStrength");
    static readonly int PapyrusStainStrengthId = Shader.PropertyToID("_GuildLensPapyrusStainStrength");
    static readonly int PapyrusBlueWashStrengthId = Shader.PropertyToID("_GuildLensPapyrusBlueWashStrength");
    static readonly int PapyrusScrollFlowId = Shader.PropertyToID("_GuildLensPapyrusScrollFlow");
    static readonly int InkWashStrengthId = Shader.PropertyToID("_GuildLensInkWashStrength");
    static readonly int InkWashColorId = Shader.PropertyToID("_GuildLensInkWashColor");
    static readonly int InkWashScaleId = Shader.PropertyToID("_GuildLensInkWashScale");
    static readonly int InkWashSoftnessId = Shader.PropertyToID("_GuildLensInkWashSoftness");
    static readonly int WaterLensStrengthId = Shader.PropertyToID("_GuildLensWaterLensStrength");
    static readonly int WaterTextureId = Shader.PropertyToID("_GuildLensWaterTexture");
    static readonly int WaterTextureEnabledId = Shader.PropertyToID("_GuildLensWaterTextureEnabled");
    static readonly int WaterTextureParamsId = Shader.PropertyToID("_GuildLensWaterTextureParams");
    static readonly int WaterTextureStyleId = Shader.PropertyToID("_GuildLensWaterTextureStyle");
    static readonly int JournalTextureId = Shader.PropertyToID("_GuildLensJournalTexture");
    static readonly int JournalTextureEnabledId = Shader.PropertyToID("_GuildLensJournalTextureEnabled");
    static readonly int JournalTextureParamsId = Shader.PropertyToID("_GuildLensJournalTextureParams");
    static readonly int JournalTextureStyleId = Shader.PropertyToID("_GuildLensJournalTextureStyle");
    static readonly int WaterMotionSuppressionId = Shader.PropertyToID("_GuildLensWaterMotionSuppression");
    static readonly int WaterPapyrusOverrideStrengthId = Shader.PropertyToID("_GuildLensWaterPapyrusOverrideStrength");
    static readonly int WaterPapyrusBlueStrengthId = Shader.PropertyToID("_GuildLensWaterPapyrusBlueStrength");
    static readonly int WaterPapyrusMatteSmoothnessId = Shader.PropertyToID("_GuildLensWaterPapyrusMatteSmoothness");
    static readonly int CameraPapyrusMaskId = Shader.PropertyToID("_GuildLensCameraPapyrusMask");
    static readonly int PapyrusRevealId = Shader.PropertyToID("_GuildLensPapyrusReveal");
    static readonly int CartoonStrengthId = Shader.PropertyToID("_GuildLensCartoonStrength");
    static readonly int CartoonSampleRadiusId = Shader.PropertyToID("_GuildLensCartoonSampleRadiusPixels");
    static readonly int CartoonValueStepsId = Shader.PropertyToID("_GuildLensCartoonValueSteps");
    static readonly int CartoonColorStepsId = Shader.PropertyToID("_GuildLensCartoonColorSteps");
    static readonly int CartoonEdgePreserveId = Shader.PropertyToID("_GuildLensCartoonEdgePreserve");
    static readonly int MotionId = Shader.PropertyToID("_GuildLensMotion");
    static readonly int MotionDriftId = Shader.PropertyToID("_GuildLensMotionDrift");
    static readonly int OutlinePapyrusDependenceId = Shader.PropertyToID("_GuildLensOutlinePapyrusDependence");
    static readonly int OutlineFocusInfluenceId = Shader.PropertyToID("_GuildLensOutlineFocusInfluence");
    static readonly int WaterBoundaryOutlineStrengthId = Shader.PropertyToID("_GuildLensWaterBoundaryOutlineStrength");
    static readonly int WaterBoundaryOutlineWidthId = Shader.PropertyToID("_GuildLensWaterBoundaryOutlineWidthPixels");
    static readonly int WaterBoundaryOutlineThicknessInfluenceId = Shader.PropertyToID("_GuildLensWaterBoundaryOutlineThicknessInfluence");
    static readonly int WaterBoundaryOutlineSideId = Shader.PropertyToID("_GuildLensWaterBoundaryOutlineSide");
    static readonly int OilPapyrusDependenceId = Shader.PropertyToID("_GuildLensOilPapyrusDependence");
    static readonly int MiniatureFocusEnabledId = Shader.PropertyToID("_GuildLensMiniatureFocusEnabled");
    static readonly int MiniatureFocusStrengthId = Shader.PropertyToID("_GuildLensMiniatureFocusStrength");
    static readonly int MiniatureFocusRadiusId = Shader.PropertyToID("_GuildLensMiniatureFocusRadiusPixels");
    static readonly int MiniatureFocusSoftnessId = Shader.PropertyToID("_GuildLensMiniatureFocusSoftness");
    static readonly int MiniatureFocusZoomInfluenceId = Shader.PropertyToID("_GuildLensMiniatureFocusZoomInfluence");
    static readonly int MiniatureFocusPapyrusProtectionId = Shader.PropertyToID("_GuildLensMiniatureFocusPapyrusProtection");
    static readonly int MiniatureFocusEdgeBiasId = Shader.PropertyToID("_GuildLensMiniatureFocusEdgeBias");
    static readonly int ExclusionMaskId = Shader.PropertyToID("_GuildLensExclusionMask");
    static readonly int DebugModeId = Shader.PropertyToID("_GuildLensDebugMode");

    [SerializeField] Settings settings = new Settings();

    GuildLensExclusionMaskPass exclusionMaskPass;
    GuildLensPass pass;
    MiniatureFocusPass miniatureFocusPass;
    Material material;
    float papyrusRevealProgress;
    float lastRevealRealtime = -1f;
    Camera lastMotionCamera;
    Vector3 lastCameraPosition;
    Quaternion lastCameraRotation = Quaternion.identity;
    float lastCameraFov;
    float lastMotionRealtime = -1f;
    float smoothedMotionTarget;
    float smoothedMotionTargetVelocity;
    float laggedMotionTarget;
    float laggedMotionTargetVelocity;
    float reactiveMotionAmount;
    float papyrusReactiveMotionAmount;
    float outlineReactiveMotionAmount;
    float oilReactiveMotionAmount;
    Vector2 motionDrift;
    Vector2 motionDriftVelocity;
    float smoothedMapReveal;
    float smoothedMapRevealVelocity;
    float mapRevealAnimationPhase;
    float lastMapRevealRealtime = -1f;

    public override void Create()
    {
        exclusionMaskPass = new GuildLensExclusionMaskPass();
        pass = new GuildLensPass();
        miniatureFocusPass = new MiniatureFocusPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.effectEnabled)
        {
            DisableLensGlobals();
            return;
        }

        if (renderingData.cameraData.cameraType == CameraType.Preview ||
            renderingData.cameraData.cameraType == CameraType.Reflection)
        {
            return;
        }

        Shader shader = settings.shader != null ? settings.shader : Shader.Find("Hidden/AgeOfGuilds/GuildLens");
        if (shader == null)
        {
            DisableLensGlobals();
            Debug.LogWarning("Guild Lens renderer feature skipped because its shader is missing.");
            return;
        }

        if (material == null || material.shader != shader)
        {
            CoreUtils.Destroy(material);
            material = CoreUtils.CreateEngineMaterial(shader);
            material.name = "Guild Lens Runtime Material";
        }

        ApplySettings(material, renderingData.cameraData.camera);
        exclusionMaskPass.Setup(settings.outlineOilExclusionLayerMask, material, settings.renderPassEvent);
        pass.Setup(material, settings.renderPassEvent);
        miniatureFocusPass.Setup(material, settings.renderPassEvent);
        renderer.EnqueuePass(exclusionMaskPass);
        renderer.EnqueuePass(pass);
        if (settings.miniatureFocusEnabled || settings.debugMode == 29)
        {
            renderer.EnqueuePass(miniatureFocusPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(material);
        material = null;
    }

    void ApplySettings(Material target, Camera renderCamera)
    {
        GuildLensFocusDriver.FocusState focus = GuildLensFocusDriver.CurrentFocus;
        bool hasActiveFocus = focus.Active;
        float focusDistance = Mathf.Max(0.01f, settings.focusDistanceFallback);
        Vector3 focusWorldPoint = Vector3.zero;
        Vector4 focusScreenPoint = new Vector4(0.5f, 0.5f, 0f, 0f);
        Vector2 focusRadii = new Vector2(
            Mathf.Max(0.001f, focus.ScreenRadii.x),
            Mathf.Max(0.001f, focus.ScreenRadii.y));
        focusRadii = ResolveAngleAdjustedRadii(renderCamera, focusRadii);
        Vector4 focusEllipse = new Vector4(
            focusRadii.x,
            focusRadii.y,
            focus.ScreenRotation,
            Mathf.Clamp01(focus.DepthContribution));
        float baseMotionAmount = UpdateCameraMotion(renderCamera);
        float papyrusMotionAmount = papyrusReactiveMotionAmount * Mathf.Clamp01(settings.papyrusMotionInfluence);
        float outlineMotionAmount = outlineReactiveMotionAmount * Mathf.Clamp01(settings.outlineMotionInfluence);
        float oilMotionAmount = oilReactiveMotionAmount * Mathf.Clamp01(settings.oilMotionInfluence);
        Vector4 mapReveal = UpdateMapReveal(renderCamera, baseMotionAmount);
        float cameraPapyrusMask = Mathf.Clamp01(mapReveal.x);
        float revealProgress = UpdatePapyrusRevealProgress(cameraPapyrusMask);

        if (hasActiveFocus)
        {
            focusWorldPoint = focus.WorldPoint;
            focusDistance = Mathf.Max(0.01f, focus.Distance);
            focusScreenPoint = focus.ScreenPoint;
        }

        SetFloat(target, FocusFallbackId, Mathf.Max(0.01f, settings.focusDistanceFallback));
        SetFloat(target, FocusRangeId, Mathf.Max(0.01f, settings.focusRangeWidth));
        SetFloat(target, TransitionSoftnessId, Mathf.Max(0f, settings.transitionSoftness));
        SetFloat(target, FocusDistanceId, focusDistance);
        SetFloat(target, FocusDriverActiveId, hasActiveFocus ? 1f : 0f);
        SetVector(target, FocusWorldPointId, focusWorldPoint);
        SetVector(target, FocusScreenPointId, focusScreenPoint);
        SetVector(target, FocusEllipseId, focusEllipse);
        SetFloat(target, FocusScreenSoftnessId, Mathf.Max(0.0001f, focus.ScreenSoftness));
        SetFloat(target, FocusProtectionStrengthId, Mathf.Clamp01(settings.focusProtectionStrength));
        SetFloat(target, FocusFalloffPowerId, Mathf.Clamp(settings.focusFalloffPower, 0.25f, 4f));
        SetVector(target, MapRevealId, mapReveal);
        SetFloat(target, GlobalStrengthId, Mathf.Clamp01(settings.globalStrength));
        SetFloat(target, WorldClarityId, Mathf.Clamp01(settings.worldClarity));
        SetFloat(target, NearStrengthId, Mathf.Clamp01(settings.nearStylizationStrength));
        SetFloat(target, FarStrengthId, Mathf.Clamp01(settings.farStylizationStrength));
        SetFloat(target, FarHazeStrengthId, Mathf.Clamp01(settings.farHazeStrength));
        SetFloat(target, FarHazeRangeId, Mathf.Max(0.01f, settings.farHazeRange));
        SetFloat(target, OilSmearId, Mathf.Max(0f, settings.oilSmearAmount));
        SetFloat(target, PapyrusStrengthId, Mathf.Clamp01(settings.papyrusTextureStrength));
        SetFloat(target, PaperVignetteStrengthId, Mathf.Clamp01(settings.paperVignetteStrength));
        SetFloat(target, PaperVignetteRadiusId, Mathf.Clamp01(settings.paperVignetteRadius));
        SetFloat(target, AmbientPaperVignetteStrengthId, Mathf.Clamp01(settings.ambientPaperVignetteStrength));
        SetFloat(target, HorizonPaperStrengthId, Mathf.Clamp01(settings.horizonPaperStrength));
        SetFloat(target, InkStrengthId, Mathf.Clamp01(settings.inkEdgeDarkeningStrength));
        SetFloat(target, DistanceFadeId, Mathf.Clamp01(settings.distanceColorFade));
        SetFloat(target, SaturationPreserveId, Mathf.Clamp01(settings.saturationPreserve));
        SetFloat(target, ContrastPreserveId, Mathf.Clamp01(settings.contrastPreserve));
        SetFloat(target, OutlineWidthId, Mathf.Max(0.25f, settings.outlineWidthPixels));
        SetFloat(target, OutlineStrengthId, Mathf.Clamp(settings.outlineStrength, 0f, 2f));
        SetColor(target, OutlineColorId, settings.outlineColor);
        SetFloat(target, OutlineOpacityId, Mathf.Clamp01(settings.outlineOpacity));
        SetFloat(target, OutlineContrastId, Mathf.Clamp(settings.outlineContrast, 0f, 2f));
        SetFloat(target, OutlineEdgeSoftnessId, Mathf.Clamp(settings.outlineEdgeSoftness, 0.001f, 1f));
        SetFloat(target, OutlineAnimationSpeedId, Mathf.Max(0f, settings.outlineAnimationSpeed));
        SetFloat(target, OutlineRevealIntensityId, Mathf.Clamp01(settings.outlineRevealIntensity));
        SetFloat(target, OutlineOrganicStrengthId, Mathf.Clamp01(settings.outlineOrganicStrength));
        SetFloat(target, OutlineRevealStrengthId, Mathf.Clamp01(settings.outlineRevealStrength));
        SetFloat(target, OutlineNoiseScaleId, Mathf.Max(0.001f, settings.outlineNoiseScale));
        SetFloat(target, OutlineRevealSoftnessId, Mathf.Clamp(settings.outlineRevealSoftness, 0.001f, 1f));
        SetFloat(target, OutlineSettleStrengthId, Mathf.Clamp01(settings.outlineSettleStrength));
        SetFloat(target, OutlineBleedStrengthId, Mathf.Clamp01(settings.outlineBleedStrength));
        SetFloat(target, OutlineBleedWidthId, Mathf.Max(0.25f, settings.outlineBleedWidthPixels));
        SetFloat(target, OutlineBleedSoftnessId, Mathf.Clamp(settings.outlineBleedSoftness, 0.001f, 1f));
        SetFloat(target, OutlineSegmentRandomnessId, Mathf.Clamp01(settings.outlineSegmentRandomness));
        SetFloat(target, OutlineStrokeCrawlStrengthId, Mathf.Clamp01(settings.outlineStrokeCrawlStrength));
        SetFloat(target, OutlineLeadingEdgeStrengthId, Mathf.Clamp01(settings.outlineLeadingEdgeStrength));
        SetFloat(target, OutlineEdgePoolingStrengthId, Mathf.Clamp01(settings.outlineEdgePoolingStrength));
        SetColor(target, PapyrusColorId, settings.papyrusColor);
        SetFloat(target, PapyrusTintStrengthId, Mathf.Clamp01(settings.papyrusTintStrength));
        SetFloat(target, PapyrusOverlayOpacityId, Mathf.Clamp01(settings.papyrusOverlayOpacity));
        SetFloat(target, PapyrusSaturationId, Mathf.Clamp01(settings.papyrusSaturation));
        SetFloat(target, PapyrusAgingDarkeningId, Mathf.Clamp01(settings.papyrusAgingDarkening));
        SetFloat(target, PapyrusBlendStrengthId, Mathf.Clamp01(settings.papyrusBlendStrength));
        SetFloat(target, PapyrusEdgeSoftnessId, Mathf.Clamp(settings.papyrusEdgeSoftness, 0.001f, 1f));
        SetFloat(target, PapyrusEdgeNoiseStrengthId, Mathf.Clamp01(settings.papyrusEdgeNoiseStrength));
        SetFloat(target, PapyrusEdgeDarkeningId, Mathf.Clamp01(settings.papyrusEdgeDarkening));
        SetFloat(target, PapyrusEdgeAnimationSpeedId, Mathf.Max(0f, settings.papyrusEdgeAnimationSpeed));
        SetFloat(target, PapyrusContrastId, Mathf.Clamp(settings.papyrusContrast, 0f, 2f));
        SetFloat(target, PapyrusMaterialStrengthId, Mathf.Clamp01(settings.papyrusMaterialStrength));
        SetFloat(target, PapyrusUnifyStrengthId, Mathf.Clamp01(settings.papyrusUnifyStrength));
        SetFloat(target, PapyrusFiberStrengthId, Mathf.Clamp01(settings.papyrusFiberStrength));
        SetFloat(target, PapyrusStainStrengthId, Mathf.Clamp01(settings.papyrusStainStrength));
        SetFloat(target, PapyrusBlueWashStrengthId, Mathf.Clamp01(settings.papyrusBlueWashStrength));
        SetVector(target, PapyrusScrollFlowId, ResolvePapyrusScrollFlow(renderCamera, baseMotionAmount));
        SetFloat(target, InkWashStrengthId, Mathf.Clamp01(settings.inkWashStrength));
        SetColor(target, InkWashColorId, settings.inkWashColor);
        SetFloat(target, InkWashScaleId, Mathf.Max(0.001f, settings.inkWashScale));
        SetFloat(target, InkWashSoftnessId, Mathf.Clamp(settings.inkWashSoftness, 0.001f, 1f));
        SetFloat(target, WaterLensStrengthId, Mathf.Clamp01(settings.waterLensStrength));
        SetTexture(target, WaterTextureId, settings.waterTexture);
        SetFloat(target, WaterTextureEnabledId, settings.waterTextureEnabled ? 1f : 0f);
        SetVector(target, WaterTextureParamsId, new Vector4(
            Mathf.Clamp01(settings.waterTextureOpacity),
            Mathf.Max(0.0001f, settings.waterTextureScale),
            Mathf.Clamp01(settings.waterTextureDensity),
            settings.waterTextureSeed));
        SetVector(target, WaterTextureStyleId, new Vector4(
            settings.waterTextureRotation * Mathf.Deg2Rad,
            Mathf.Clamp01(settings.waterTextureRandomRotation),
            Mathf.Clamp(settings.waterTextureContrast, 0f, 2f),
            Mathf.Clamp(settings.waterTextureIntensity, 0f, 2f)));
        SetTexture(target, JournalTextureId, settings.journalTexture);
        SetFloat(target, JournalTextureEnabledId, settings.journalTextureEnabled ? 1f : 0f);
        SetVector(target, JournalTextureParamsId, new Vector4(
            Mathf.Clamp01(settings.journalTextureOpacity),
            Mathf.Max(0.0001f, settings.journalTextureScale),
            Mathf.Clamp01(settings.journalTextureDensity),
            settings.journalTextureSeed));
        SetVector(target, JournalTextureStyleId, new Vector4(
            settings.journalTextureRotation * Mathf.Deg2Rad,
            Mathf.Clamp01(settings.journalTextureRandomRotation),
            Mathf.Clamp(settings.journalTextureContrast, 0f, 2f),
            Mathf.Clamp(settings.journalTextureIntensity, 0f, 2f)));
        SetFloat(target, WaterMotionSuppressionId, Mathf.Clamp01(settings.waterMotionSuppression));
        SetFloat(target, WaterPapyrusOverrideStrengthId, Mathf.Clamp01(settings.waterPapyrusOverrideStrength));
        SetFloat(target, WaterPapyrusBlueStrengthId, Mathf.Clamp01(settings.waterPapyrusBlueStrength));
        SetFloat(target, WaterPapyrusMatteSmoothnessId, Mathf.Clamp01(settings.waterPapyrusMatteSmoothness));
        SetFloat(target, CameraPapyrusMaskId, cameraPapyrusMask);
        SetVector(target, PapyrusRevealId, new Vector4(
            revealProgress,
            Mathf.Clamp01(settings.papyrusRevealStrength),
            Mathf.Max(0.001f, settings.papyrusRevealScale),
            Mathf.Clamp(settings.papyrusRevealSoftness, 0.001f, 1f)));
        SetFloat(target, CartoonStrengthId, Mathf.Clamp01(settings.cartoonStrength));
        SetFloat(target, CartoonSampleRadiusId, Mathf.Max(0.25f, settings.cartoonSampleRadiusPixels));
        SetFloat(target, CartoonValueStepsId, Mathf.Max(2f, settings.cartoonValueSteps));
        SetFloat(target, CartoonColorStepsId, Mathf.Max(2f, settings.cartoonColorSteps));
        SetFloat(target, CartoonEdgePreserveId, Mathf.Clamp01(settings.cartoonEdgePreserve));
        SetVector(target, MotionId, new Vector4(
            papyrusMotionAmount,
            baseMotionAmount,
            outlineMotionAmount,
            oilMotionAmount));
        SetVector(target, MotionDriftId, new Vector4(
            motionDrift.x,
            motionDrift.y,
            Mathf.Clamp(settings.papyrusMotionDrift, 0f, 1f),
            papyrusMotionAmount));
        SetFloat(target, OutlinePapyrusDependenceId, Mathf.Clamp01(settings.outlinePapyrusDependence));
        SetFloat(target, OutlineFocusInfluenceId, Mathf.Clamp01(settings.outlineFocusInfluence));
        SetFloat(target, WaterBoundaryOutlineStrengthId, Mathf.Clamp01(settings.waterBoundaryOutlineStrength));
        SetFloat(target, WaterBoundaryOutlineWidthId, Mathf.Max(0.25f, settings.waterBoundaryOutlineWidthPixels));
        SetFloat(target, WaterBoundaryOutlineThicknessInfluenceId, Mathf.Clamp01(settings.waterBoundaryOutlineThicknessInfluence));
        SetFloat(target, WaterBoundaryOutlineSideId, Mathf.Clamp01(settings.waterBoundaryOutlineSide));
        SetFloat(target, OilPapyrusDependenceId, Mathf.Clamp01(settings.oilPapyrusDependence));
        SetFloat(target, MiniatureFocusEnabledId, settings.miniatureFocusEnabled || settings.debugMode == 29 ? 1f : 0f);
        SetFloat(target, MiniatureFocusStrengthId, Mathf.Clamp01(settings.miniatureFocusStrength));
        SetFloat(target, MiniatureFocusRadiusId, Mathf.Max(0.25f, settings.miniatureFocusRadiusPixels));
        SetFloat(target, MiniatureFocusSoftnessId, Mathf.Clamp(settings.miniatureFocusSoftness, 0.001f, 1f));
        SetFloat(target, MiniatureFocusZoomInfluenceId, Mathf.Clamp01(settings.miniatureFocusZoomInfluence));
        SetFloat(target, MiniatureFocusPapyrusProtectionId, Mathf.Clamp01(settings.miniatureFocusPapyrusProtection));
        SetFloat(target, MiniatureFocusEdgeBiasId, Mathf.Clamp01(settings.miniatureFocusEdgeBias));
        target.SetInt(DebugModeId, Mathf.Clamp(settings.debugMode, 0, 29));
        Shader.SetGlobalInt(DebugModeId, Mathf.Clamp(settings.debugMode, 0, 29));
    }

    float UpdateCameraMotion(Camera renderCamera)
    {
        if (renderCamera == null)
        {
            ResetCameraMotion();
            return 0f;
        }

        float now = Time.realtimeSinceStartup;
        bool reset = lastMotionCamera != renderCamera || lastMotionRealtime < 0f;
        float deltaTime = reset ? 1f / 60f : Mathf.Clamp(now - lastMotionRealtime, 0.001f, 0.1f);

        Vector3 cameraPosition = renderCamera.transform.position;
        Quaternion cameraRotation = renderCamera.transform.rotation;
        float cameraFov = renderCamera.fieldOfView;

        if (reset)
        {
            lastMotionCamera = renderCamera;
            lastCameraPosition = cameraPosition;
            lastCameraRotation = cameraRotation;
            lastCameraFov = cameraFov;
            lastMotionRealtime = now;
            smoothedMotionTarget = 0f;
            laggedMotionTarget = 0f;
            reactiveMotionAmount = 0f;
            papyrusReactiveMotionAmount = 0f;
            outlineReactiveMotionAmount = 0f;
            oilReactiveMotionAmount = 0f;
            motionDrift = Vector2.zero;
            motionDriftVelocity = Vector2.zero;
            return 0f;
        }

        Vector3 positionDelta = cameraPosition - lastCameraPosition;
        float positionSpeed = positionDelta.magnitude / deltaTime;
        float rotationSpeed = Quaternion.Angle(lastCameraRotation, cameraRotation) / deltaTime;
        float fovSpeed = Mathf.Abs(cameraFov - lastCameraFov) / deltaTime;

        float rawMotion =
            positionSpeed * Mathf.Max(0f, settings.motionPositionSensitivity) +
            rotationSpeed * Mathf.Max(0f, settings.motionRotationSensitivity) +
            fovSpeed * Mathf.Max(0f, settings.motionFovSensitivity);
        rawMotion = 1f - Mathf.Exp(-rawMotion * Mathf.Max(0f, settings.motionSensitivity));
        rawMotion = Mathf.Clamp01(rawMotion);

        float smoothing = Mathf.Max(0.001f, settings.motionSmoothing);
        smoothedMotionTarget = Mathf.SmoothDamp(
            smoothedMotionTarget,
            rawMotion,
            ref smoothedMotionTargetVelocity,
            smoothing,
            Mathf.Infinity,
            deltaTime);

        float lag = Mathf.Max(0.001f, settings.motionLagSeconds);
        laggedMotionTarget = Mathf.SmoothDamp(
            laggedMotionTarget,
            smoothedMotionTarget,
            ref laggedMotionTargetVelocity,
            lag,
            Mathf.Infinity,
            deltaTime);

        float motionSpeed = laggedMotionTarget > reactiveMotionAmount
            ? Mathf.Max(0f, settings.motionRiseSpeed)
            : Mathf.Max(0f, settings.motionFallSpeed);
        reactiveMotionAmount = motionSpeed <= 0f
            ? laggedMotionTarget
            : Mathf.MoveTowards(reactiveMotionAmount, laggedMotionTarget, motionSpeed * deltaTime);
        float clampedReactiveMotion = Mathf.Clamp01(reactiveMotionAmount * Mathf.Clamp01(settings.maxMotionAmount));
        papyrusReactiveMotionAmount = MoveReactiveAmount(
            papyrusReactiveMotionAmount,
            clampedReactiveMotion,
            settings.papyrusMotionRiseSpeed,
            settings.papyrusMotionDecaySpeed,
            deltaTime);
        outlineReactiveMotionAmount = MoveReactiveAmount(
            outlineReactiveMotionAmount,
            clampedReactiveMotion,
            settings.outlineMotionRiseSpeed,
            settings.outlineMotionDecaySpeed,
            deltaTime);
        oilReactiveMotionAmount = MoveReactiveAmount(
            oilReactiveMotionAmount,
            clampedReactiveMotion,
            settings.motionRiseSpeed,
            settings.oilMotionDecaySpeed,
            deltaTime);

        Vector3 deltaEuler = (cameraRotation * Quaternion.Inverse(lastCameraRotation)).eulerAngles;
        deltaEuler.x = Mathf.DeltaAngle(0f, deltaEuler.x);
        deltaEuler.y = Mathf.DeltaAngle(0f, deltaEuler.y);
        deltaEuler.z = Mathf.DeltaAngle(0f, deltaEuler.z);

        Transform cameraTransform = renderCamera.transform;
        float sidewaysSpeed = Vector3.Dot(positionDelta, cameraTransform.right) / deltaTime;
        float verticalSpeed = Vector3.Dot(positionDelta, cameraTransform.up) / deltaTime;
        float yawSpeed = deltaEuler.y / deltaTime;
        float pitchSpeed = deltaEuler.x / deltaTime;
        float fovDeltaSpeed = (cameraFov - lastCameraFov) / deltaTime;
        Vector2 driftTarget = new Vector2(
            sidewaysSpeed * 0.01f - yawSpeed * 0.0025f,
            verticalSpeed * 0.01f + pitchSpeed * 0.0025f + fovDeltaSpeed * 0.002f);
        driftTarget *= Mathf.Max(0f, settings.papyrusMotionDriftSensitivity);
        driftTarget = Vector2.ClampMagnitude(driftTarget, 1f);
        float driftSpeed = driftTarget.sqrMagnitude > motionDrift.sqrMagnitude
            ? Mathf.Max(0f, settings.papyrusMotionRiseSpeed)
            : Mathf.Max(0f, settings.papyrusMotionDriftDecaySpeed);
        motionDrift = Vector2.SmoothDamp(
            motionDrift,
            driftTarget,
            ref motionDriftVelocity,
            Mathf.Max(0.001f, settings.motionLagSeconds + settings.motionSmoothing),
            driftSpeed <= 0f ? 0f : driftSpeed,
            deltaTime);

        lastCameraPosition = cameraPosition;
        lastCameraRotation = cameraRotation;
        lastCameraFov = cameraFov;
        lastMotionRealtime = now;

        return clampedReactiveMotion;
    }

    static float MoveReactiveAmount(float current, float target, float riseSpeed, float decaySpeed, float deltaTime)
    {
        float speed = target > current ? Mathf.Max(0f, riseSpeed) : Mathf.Max(0f, decaySpeed);
        return speed <= 0f ? current : Mathf.MoveTowards(current, target, speed * deltaTime);
    }

    Vector4 ResolvePapyrusScrollFlow(Camera renderCamera, float motionAmount)
    {
        float strength = Mathf.Clamp01(settings.papyrusScrollFlowStrength);
        if (renderCamera == null || strength <= 0f)
        {
            return Vector4.zero;
        }

        Vector3 cameraPosition = renderCamera.transform.position;
        float worldScale = Mathf.Max(0f, settings.papyrusScrollWorldScale);
        Vector2 flowOffset = new Vector2(cameraPosition.x, cameraPosition.z) * worldScale * strength;
        float materialBoost = Mathf.Clamp01(motionAmount) * Mathf.Clamp01(settings.papyrusScrollMaterialBoost);
        return new Vector4(flowOffset.x, flowOffset.y, materialBoost, strength);
    }

    Vector4 UpdateMapReveal(Camera renderCamera, float motionAmount)
    {
        float rawReveal = ResolveMapReveal(renderCamera);
        rawReveal = Mathf.Pow(rawReveal, Mathf.Max(0.001f, settings.mapRevealResponse));
        rawReveal = Mathf.Clamp01(rawReveal + (1f - rawReveal) * Mathf.Clamp01(motionAmount) * Mathf.Clamp01(settings.mapRevealMotionBoost));

        float now = Time.realtimeSinceStartup;
        bool reset = lastMapRevealRealtime < 0f;
        float deltaTime = reset
            ? 1f / 60f
            : Mathf.Clamp(now - lastMapRevealRealtime, 0.001f, 0.1f);
        lastMapRevealRealtime = now;

        float previousReveal = smoothedMapReveal;
        if (settings.mapRevealSmoothing <= 0f || reset)
        {
            smoothedMapReveal = rawReveal;
            smoothedMapRevealVelocity = 0f;
        }
        else
        {
            smoothedMapReveal = Mathf.SmoothDamp(
                smoothedMapReveal,
                rawReveal,
                ref smoothedMapRevealVelocity,
                Mathf.Max(0.001f, settings.mapRevealSmoothing),
                Mathf.Infinity,
                deltaTime);
        }

        smoothedMapReveal = Mathf.Clamp01(smoothedMapReveal);
        float revealVelocity = deltaTime > 0f ? Mathf.Abs(smoothedMapReveal - previousReveal) / deltaTime : 0f;
        float edgeMotion = Mathf.Clamp01(revealVelocity * 0.35f + Mathf.Clamp01(motionAmount) * 0.5f);
        mapRevealAnimationPhase = Mathf.Repeat(mapRevealAnimationPhase + deltaTime * edgeMotion, 10000f);
        return new Vector4(smoothedMapReveal, rawReveal, edgeMotion, mapRevealAnimationPhase);
    }

    float ResolveMapReveal(Camera renderCamera)
    {
        CameraOrbitController orbitController = ResolveOrbitController(renderCamera);
        if (orbitController == null)
        {
            return 0f;
        }

        float reveal = Mathf.InverseLerp(
            Mathf.Clamp01(settings.mapRevealZoomStart),
            Mathf.Clamp01(settings.mapRevealZoomFull),
            Mathf.Clamp01(orbitController.NormalizedZoom));
        reveal = Mathf.Clamp01(reveal);
        return reveal * reveal * (3f - 2f * reveal);
    }

    void ResetCameraMotion()
    {
        lastMotionCamera = null;
        lastMotionRealtime = -1f;
        smoothedMotionTarget = 0f;
        smoothedMotionTargetVelocity = 0f;
        laggedMotionTarget = 0f;
        laggedMotionTargetVelocity = 0f;
        reactiveMotionAmount = 0f;
        papyrusReactiveMotionAmount = 0f;
        outlineReactiveMotionAmount = 0f;
        oilReactiveMotionAmount = 0f;
        motionDrift = Vector2.zero;
        motionDriftVelocity = Vector2.zero;
        smoothedMapReveal = 0f;
        smoothedMapRevealVelocity = 0f;
        mapRevealAnimationPhase = 0f;
        lastMapRevealRealtime = -1f;
    }

    Vector2 ResolveAngleAdjustedRadii(Camera renderCamera, Vector2 manualRadii)
    {
        float strength = Mathf.Clamp01(settings.cameraAngleEllipseStrength);
        if (strength <= 0f)
        {
            return manualRadii;
        }

        float cameraElevation = ResolveCurrentElevation(renderCamera);
        float angleT = RangeMask01(
            cameraElevation,
            settings.ellipseAngleStart,
            settings.ellipseAngleEnd,
            settings.ellipseAngleSoftness);
        Vector2 perspectiveRadii = new Vector2(
            Mathf.Max(0.001f, settings.perspectiveRadiusX),
            Mathf.Max(0.001f, settings.perspectiveRadiusY));
        Vector2 topDownRadii = new Vector2(
            Mathf.Max(0.001f, settings.topDownRadiusX),
            Mathf.Max(0.001f, settings.topDownRadiusY));
        Vector2 angleRadii = Vector2.Lerp(perspectiveRadii, topDownRadii, angleT);
        return Vector2.Lerp(manualRadii, angleRadii, strength);
    }

    float UpdatePapyrusRevealProgress(float cameraPapyrusMask)
    {
        float target = Mathf.Clamp01(cameraPapyrusMask);
        if (settings.papyrusRevealSpeed <= 0f)
        {
            papyrusRevealProgress = target;
            lastRevealRealtime = Time.realtimeSinceStartup;
            return papyrusRevealProgress;
        }

        float now = Time.realtimeSinceStartup;
        float deltaTime = lastRevealRealtime < 0f
            ? 1f / 60f
            : Mathf.Clamp(now - lastRevealRealtime, 0f, 0.1f);
        lastRevealRealtime = now;
        papyrusRevealProgress = Mathf.MoveTowards(
            papyrusRevealProgress,
            target,
            deltaTime * Mathf.Max(0f, settings.papyrusRevealSpeed));
        return papyrusRevealProgress;
    }

    static CameraOrbitController ResolveOrbitController(Camera renderCamera)
    {
        if (renderCamera != null)
        {
            CameraOrbitController orbitController = renderCamera.GetComponent<CameraOrbitController>();
            if (orbitController != null)
            {
                return orbitController;
            }

            orbitController = renderCamera.GetComponentInParent<CameraOrbitController>();
            if (orbitController != null)
            {
                return orbitController;
            }
        }

        return Object.FindAnyObjectByType<CameraOrbitController>();
    }

    static float ResolveCameraElevation(Camera renderCamera)
    {
        if (renderCamera == null)
        {
            return 40f;
        }

        Vector3 forward = renderCamera.transform.forward.normalized;
        float downward = Mathf.Clamp01(Vector3.Dot(forward, Vector3.down));
        return Mathf.Asin(downward) * Mathf.Rad2Deg;
    }

    float ResolveCurrentElevation(Camera renderCamera)
    {
        CameraOrbitController orbitController = ResolveOrbitController(renderCamera);
        return orbitController != null ? orbitController.CurrentElevation : ResolveCameraElevation(renderCamera);
    }

    static float RangeMask01(float value, float start, float end, float softness)
    {
        if (Mathf.Abs(start - end) < 0.0001f)
        {
            return value >= end ? 1f : 0f;
        }

        float linear = Mathf.InverseLerp(start, end, value);
        float smoothed = Mathf.SmoothStep(0f, 1f, linear);
        return Mathf.Lerp(linear, smoothed, Mathf.Clamp01(softness));
    }

    static void SetFloat(Material target, int id, float value)
    {
        target.SetFloat(id, value);
        Shader.SetGlobalFloat(id, value);
    }

    static void SetVector(Material target, int id, Vector4 value)
    {
        target.SetVector(id, value);
        Shader.SetGlobalVector(id, value);
    }

    static void SetColor(Material target, int id, Color value)
    {
        target.SetColor(id, value);
        Shader.SetGlobalColor(id, value);
    }

    static void SetTexture(Material target, int id, Texture texture)
    {
        Texture resolvedTexture = texture != null ? texture : Texture2D.blackTexture;
        target.SetTexture(id, resolvedTexture);
        Shader.SetGlobalTexture(id, resolvedTexture);
    }

    static void DisableLensGlobals()
    {
        Shader.SetGlobalFloat(GlobalStrengthId, 0f);
        Shader.SetGlobalFloat(FocusDriverActiveId, 0f);
        Shader.SetGlobalFloat(FocusProtectionStrengthId, 0f);
        Shader.SetGlobalFloat(FocusFalloffPowerId, 1.25f);
        Shader.SetGlobalVector(MapRevealId, Vector4.zero);
        Shader.SetGlobalFloat(WaterLensStrengthId, 0f);
        Shader.SetGlobalTexture(WaterTextureId, Texture2D.blackTexture);
        Shader.SetGlobalFloat(WaterTextureEnabledId, 0f);
        Shader.SetGlobalVector(WaterTextureParamsId, Vector4.zero);
        Shader.SetGlobalVector(WaterTextureStyleId, Vector4.zero);
        Shader.SetGlobalTexture(JournalTextureId, Texture2D.blackTexture);
        Shader.SetGlobalFloat(JournalTextureEnabledId, 0f);
        Shader.SetGlobalVector(JournalTextureParamsId, Vector4.zero);
        Shader.SetGlobalVector(JournalTextureStyleId, Vector4.zero);
        Shader.SetGlobalFloat(OutlineOrganicStrengthId, 0f);
        Shader.SetGlobalFloat(OutlineOpacityId, 0f);
        Shader.SetGlobalFloat(OutlineBleedStrengthId, 0f);
        Shader.SetGlobalFloat(OutlineEdgePoolingStrengthId, 0f);
        Shader.SetGlobalFloat(WaterBoundaryOutlineThicknessInfluenceId, 0f);
        Shader.SetGlobalFloat(WaterMotionSuppressionId, 0f);
        Shader.SetGlobalFloat(WaterPapyrusOverrideStrengthId, 0f);
        Shader.SetGlobalFloat(WaterPapyrusBlueStrengthId, 0f);
        Shader.SetGlobalFloat(WaterPapyrusMatteSmoothnessId, 0.04f);
        Shader.SetGlobalFloat(MiniatureFocusEnabledId, 0f);
        Shader.SetGlobalFloat(MiniatureFocusStrengthId, 0f);
        Shader.SetGlobalFloat(MiniatureFocusRadiusId, 0f);
        Shader.SetGlobalFloat(MiniatureFocusSoftnessId, 0.45f);
        Shader.SetGlobalFloat(MiniatureFocusZoomInfluenceId, 0f);
        Shader.SetGlobalFloat(MiniatureFocusPapyrusProtectionId, 0f);
        Shader.SetGlobalFloat(MiniatureFocusEdgeBiasId, 0f);
        Shader.SetGlobalFloat(CameraPapyrusMaskId, 0f);
        Shader.SetGlobalVector(PapyrusRevealId, new Vector4(0f, 1f, 5f, 0.25f));
        Shader.SetGlobalFloat(PapyrusMaterialStrengthId, 0f);
        Shader.SetGlobalFloat(PapyrusOverlayOpacityId, 0f);
        Shader.SetGlobalFloat(AmbientPaperVignetteStrengthId, 0f);
        Shader.SetGlobalFloat(CartoonStrengthId, 0f);
        Shader.SetGlobalVector(MotionId, Vector4.zero);
        Shader.SetGlobalVector(MotionDriftId, Vector4.zero);
        Shader.SetGlobalVector(PapyrusScrollFlowId, Vector4.zero);
    }

    sealed class GuildLensFrameData : ContextItem
    {
        public TextureHandle exclusionMask = TextureHandle.nullHandle;

        public override void Reset()
        {
            exclusionMask = TextureHandle.nullHandle;
        }
    }

    sealed class GuildLensExclusionMaskPass : ScriptableRenderPass
    {
        const string PassName = "Guild Lens Exclusion Mask";
        static readonly ShaderTagId MaskShaderTag = new ShaderTagId("GuildLensExclusionMask");

        readonly ProfilingSampler maskProfilingSampler = new ProfilingSampler(PassName);
        readonly List<ShaderTagId> shaderTagIds = new List<ShaderTagId>(1) { MaskShaderTag };
        LayerMask layerMask;
        Material stencilMaskMaterial;

        public GuildLensExclusionMaskPass()
        {
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        public void Setup(LayerMask maskLayers, Material maskMaterial, RenderPassEvent passEvent)
        {
            layerMask = maskLayers;
            stencilMaskMaterial = maskMaterial;
            renderPassEvent = passEvent;
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            if (resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            TextureDesc maskDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            maskDesc.name = "_GuildLensExclusionMask";
            maskDesc.clearBuffer = true;
            maskDesc.clearColor = Color.black;
            maskDesc.msaaSamples = MSAASamples.None;

            TextureHandle maskTexture = renderGraph.CreateTexture(maskDesc);
            GuildLensFrameData lensFrameData = frameData.GetOrCreate<GuildLensFrameData>();
            lensFrameData.exclusionMask = maskTexture;

            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();

            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);
            DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(
                shaderTagIds,
                renderingData,
                cameraData,
                lightData,
                SortingCriteria.None);
            drawingSettings.perObjectData = PerObjectData.None;

            RendererListParams rendererListParams = new RendererListParams(
                renderingData.cullResults,
                drawingSettings,
                filteringSettings);
            rendererListParams.filteringSettings.batchLayerMask = uint.MaxValue;
            RendererListHandle rendererList = renderGraph.CreateRendererList(rendererListParams);

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<MaskPassData>(
                       PassName, out MaskPassData passData, maskProfilingSampler))
            {
                passData.rendererList = rendererList;
                passData.stencilMaskMaterial = stencilMaskMaterial;

                builder.UseRendererList(rendererList);
                builder.SetRenderAttachment(maskTexture, 0, AccessFlags.Write);
                if (resourceData.activeDepthTexture.IsValid())
                {
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Read);
                }

                builder.AllowPassCulling(false);
                builder.SetRenderFunc((MaskPassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(RTClearFlags.Color, Color.black, 1f, 0);
                    context.cmd.DrawRendererList(data.rendererList);
                    if (data.stencilMaskMaterial != null && data.stencilMaskMaterial.passCount > 1)
                    {
                        context.cmd.DrawProcedural(
                            Matrix4x4.identity,
                            data.stencilMaskMaterial,
                            1,
                            MeshTopology.Triangles,
                            3,
                            1);
                    }
                });
            }
        }

        sealed class MaskPassData
        {
            public RendererListHandle rendererList;
            public Material stencilMaskMaterial;
        }
    }

    sealed class GuildLensPass : ScriptableRenderPass
    {
        const string PassName = "Guild Lens";

        static readonly int BlitTextureId = Shader.PropertyToID("_BlitTexture");
        static readonly int BlitScaleBiasId = Shader.PropertyToID("_BlitScaleBias");
        static readonly MaterialPropertyBlock SharedPropertyBlock = new MaterialPropertyBlock();

        Material material;

        public GuildLensPass()
        {
            profilingSampler = new ProfilingSampler(PassName);
            ConfigureInput(ScriptableRenderPassInput.Depth);
            requiresIntermediateTexture = true;
        }

        public void Setup(Material passMaterial, RenderPassEvent passEvent)
        {
            material = passMaterial;
            renderPassEvent = passEvent;
            ConfigureInput(ScriptableRenderPassInput.Depth);
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null)
            {
                return;
            }

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            if (resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            TextureHandle source = resourceData.activeColorTexture;
            TextureDesc copyDesc = renderGraph.GetTextureDesc(source);
            copyDesc.name = "_GuildLensSourceColor";
            copyDesc.clearBuffer = false;
            copyDesc.msaaSamples = MSAASamples.None;

            TextureHandle sourceCopy = renderGraph.CreateTexture(copyDesc);
            renderGraph.AddBlitPass(source, sourceCopy, Vector2.one, Vector2.zero, passName: "Guild Lens Copy Color");

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<PassData>(
                       PassName, out PassData passData, profilingSampler))
            {
                passData.material = material;
                passData.source = sourceCopy;
                passData.exclusionMask = TextureHandle.nullHandle;
                if (frameData.Contains<GuildLensFrameData>())
                {
                    passData.exclusionMask = frameData.Get<GuildLensFrameData>().exclusionMask;
                }

                builder.UseTexture(sourceCopy, AccessFlags.Read);
                if (passData.exclusionMask.IsValid())
                {
                    builder.UseTexture(passData.exclusionMask, AccessFlags.Read);
                }
                if (resourceData.cameraDepthTexture.IsValid())
                {
                    builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
                }

                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Execute(context.cmd, data.source, data.exclusionMask, data.material);
                });
            }
        }

        static void Execute(RasterCommandBuffer cmd, RTHandle source, RTHandle exclusionMask, Material passMaterial)
        {
            SharedPropertyBlock.Clear();
            SharedPropertyBlock.SetTexture(BlitTextureId, source);
            SharedPropertyBlock.SetVector(BlitScaleBiasId, new Vector4(1f, 1f, 0f, 0f));
            if (exclusionMask != null)
            {
                SharedPropertyBlock.SetTexture(ExclusionMaskId, exclusionMask);
            }
            else
            {
                SharedPropertyBlock.SetTexture(ExclusionMaskId, Texture2D.blackTexture);
            }
            cmd.DrawProcedural(Matrix4x4.identity, passMaterial, 0, MeshTopology.Triangles, 3, 1, SharedPropertyBlock);
        }

        sealed class PassData
        {
            public Material material;
            public TextureHandle source;
            public TextureHandle exclusionMask;
        }
    }

    sealed class MiniatureFocusPass : ScriptableRenderPass
    {
        const int ShaderPassIndex = 2;
        const string PassName = "Guild Lens Miniature Focus";

        static readonly int BlitTextureId = Shader.PropertyToID("_BlitTexture");
        static readonly int BlitScaleBiasId = Shader.PropertyToID("_BlitScaleBias");
        static readonly MaterialPropertyBlock SharedPropertyBlock = new MaterialPropertyBlock();

        Material material;

        public MiniatureFocusPass()
        {
            profilingSampler = new ProfilingSampler(PassName);
            requiresIntermediateTexture = true;
        }

        public void Setup(Material passMaterial, RenderPassEvent passEvent)
        {
            material = passMaterial;
            renderPassEvent = passEvent;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (material == null || material.passCount <= ShaderPassIndex)
            {
                return;
            }

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            if (resourceData.isActiveTargetBackBuffer)
            {
                return;
            }

            TextureHandle source = resourceData.activeColorTexture;
            TextureDesc copyDesc = renderGraph.GetTextureDesc(source);
            copyDesc.name = "_GuildLensMiniatureFocusSource";
            copyDesc.clearBuffer = false;
            copyDesc.msaaSamples = MSAASamples.None;

            TextureHandle sourceCopy = renderGraph.CreateTexture(copyDesc);
            renderGraph.AddBlitPass(source, sourceCopy, Vector2.one, Vector2.zero, passName: "Guild Lens Miniature Focus Copy");

            using (IRasterRenderGraphBuilder builder = renderGraph.AddRasterRenderPass<PassData>(
                       PassName, out PassData passData, profilingSampler))
            {
                passData.material = material;
                passData.source = sourceCopy;

                builder.UseTexture(sourceCopy, AccessFlags.Read);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Execute(context.cmd, data.source, data.material);
                });
            }
        }

        static void Execute(RasterCommandBuffer cmd, RTHandle source, Material passMaterial)
        {
            SharedPropertyBlock.Clear();
            SharedPropertyBlock.SetTexture(BlitTextureId, source);
            SharedPropertyBlock.SetVector(BlitScaleBiasId, new Vector4(1f, 1f, 0f, 0f));
            cmd.DrawProcedural(Matrix4x4.identity, passMaterial, ShaderPassIndex, MeshTopology.Triangles, 3, 1, SharedPropertyBlock);
        }

        sealed class PassData
        {
            public Material material;
            public TextureHandle source;
        }
    }
}
