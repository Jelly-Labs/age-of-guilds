Shader "Hidden/AgeOfGuilds/GuildLens"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "Guild Lens"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Assets/Shaders/Include/GuildLensMask.hlsl"

            float _GuildLensFocusFallbackDistance;
            float _GuildLensFocusRangeWidth;
            float _GuildLensTransitionSoftness;
            float _GuildLensGlobalStrength;
            float _GuildLensWorldClarity;
            float _GuildLensNearStrength;
            float _GuildLensFarStrength;
            float _GuildLensFarHazeStrength;
            float _GuildLensFarHazeRange;
            float _GuildLensOilSmear;
            float _GuildLensPapyrusStrength;
            float _GuildLensPaperVignetteStrength;
            float _GuildLensPaperVignetteRadius;
            float _GuildLensAmbientPaperVignetteStrength;
            float _GuildLensHorizonPaperStrength;
            float _GuildLensInkStrength;
            float _GuildLensDistanceFade;
            float _GuildLensSaturationPreserve;
            float _GuildLensContrastPreserve;
            float _GuildLensOutlineWidthPixels;
            float _GuildLensOutlineStrength;
            float4 _GuildLensOutlineColor;
            float _GuildLensOutlineOpacity;
            float _GuildLensOutlineContrast;
            float _GuildLensOutlineEdgeSoftness;
            float _GuildLensOutlineAnimationSpeed;
            float _GuildLensOutlineRevealIntensity;
            float _GuildLensOutlineOrganicStrength;
            float _GuildLensOutlineRevealStrength;
            float _GuildLensOutlineNoiseScale;
            float _GuildLensOutlineRevealSoftness;
            float _GuildLensOutlineSettleStrength;
            float _GuildLensOutlineBleedStrength;
            float _GuildLensOutlineBleedWidthPixels;
            float _GuildLensOutlineBleedSoftness;
            float _GuildLensOutlineSegmentRandomness;
            float _GuildLensOutlineStrokeCrawlStrength;
            float _GuildLensOutlineLeadingEdgeStrength;
            float _GuildLensOutlineEdgePoolingStrength;
            float4 _GuildLensPapyrusColor;
            float _GuildLensPapyrusTintStrength;
            float _GuildLensPapyrusOverlayOpacity;
            float _GuildLensPapyrusSaturation;
            float _GuildLensPapyrusAgingDarkening;
            float _GuildLensPapyrusBlendStrength;
            float _GuildLensPapyrusEdgeSoftness;
            float _GuildLensPapyrusEdgeNoiseStrength;
            float _GuildLensPapyrusEdgeDarkening;
            float _GuildLensPapyrusEdgeAnimationSpeed;
            float _GuildLensPapyrusContrast;
            float _GuildLensPapyrusMaterialStrength;
            float _GuildLensPapyrusUnifyStrength;
            float _GuildLensPapyrusFiberStrength;
            float _GuildLensPapyrusStainStrength;
            float _GuildLensPapyrusBlueWashStrength;
            float4 _GuildLensPapyrusScrollFlow;
            float _GuildLensInkWashStrength;
            float4 _GuildLensInkWashColor;
            float _GuildLensInkWashScale;
            float _GuildLensInkWashSoftness;
            float _GuildLensCameraPapyrusMask;
            float4 _GuildLensPapyrusReveal;
            float _GuildLensCartoonStrength;
            float _GuildLensCartoonSampleRadiusPixels;
            float _GuildLensCartoonValueSteps;
            float _GuildLensCartoonColorSteps;
            float _GuildLensCartoonEdgePreserve;
            float4 _GuildLensMotion;
            float4 _GuildLensMotionDrift;
            float _GuildLensOutlinePapyrusDependence;
            float _GuildLensOutlineFocusInfluence;
            float _GuildLensWaterBoundaryOutlineStrength;
            float _GuildLensWaterBoundaryOutlineWidthPixels;
            float _GuildLensWaterBoundaryOutlineThicknessInfluence;
            float _GuildLensWaterBoundaryOutlineSide;
            float _GuildLensWaterLensStrength;
            float _GuildLensWaterMotionSuppression;
            float _GuildLensWaterPapyrusOverrideStrength;
            float _GuildLensOilPapyrusDependence;
            float _GuildLensFocusDistance;
            float _GuildLensFocusDriverActive;
            float4 _GuildLensFocusWorldPoint;
            float4 _GuildLensFocusScreenPoint;
            float4 _GuildLensFocusEllipse;
            float _GuildLensFocusScreenSoftness;
            float _GuildLensFocusProtectionStrength;
            float _GuildLensFocusFalloffPower;
            float4 _GuildLensMapReveal;
            int _GuildLensDebugMode;

            TEXTURE2D_X(_GuildLensExclusionMask);

            struct CartoonStats
            {
                float3 mean;
                float variance;
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float FractalNoise(float2 uv)
            {
                float n = 0.0;
                n += ValueNoise(uv) * 0.5;
                n += ValueNoise(uv * 2.07 + 13.1) * 0.3;
                n += ValueNoise(uv * 4.11 + 41.7) * 0.2;
                return n;
            }

            float2 PapyrusDriftUV(float2 uv)
            {
                return AOG_GuildLensPapyrusDriftUV(uv, _GuildLensMotionDrift);
            }

            float PapyrusRevealMask(float2 uv)
            {
                return AOG_GuildLensPapyrusRevealMask(uv, _GuildLensPapyrusReveal, _GuildLensMotionDrift);
            }

            float SampleExclusionMask(float2 uv)
            {
                return saturate(SAMPLE_TEXTURE2D_X_LOD(_GuildLensExclusionMask, sampler_LinearClamp, uv, 0).r);
            }

            float SampleExpandedExclusionMask(float2 uv)
            {
                float radiusPixels = max(1.0, _GuildLensOutlineWidthPixels);
                float2 texel = _BlitTexture_TexelSize.xy * radiusPixels;
                float2 diag = texel * 0.70710678;

                float mask = SampleExclusionMask(uv);
                mask = max(mask, SampleExclusionMask(uv + float2(texel.x, 0.0)));
                mask = max(mask, SampleExclusionMask(uv - float2(texel.x, 0.0)));
                mask = max(mask, SampleExclusionMask(uv + float2(0.0, texel.y)));
                mask = max(mask, SampleExclusionMask(uv - float2(0.0, texel.y)));
                mask = max(mask, SampleExclusionMask(uv + diag));
                mask = max(mask, SampleExclusionMask(uv - diag));
                mask = max(mask, SampleExclusionMask(uv + float2(diag.x, -diag.y)));
                mask = max(mask, SampleExclusionMask(uv + float2(-diag.x, diag.y)));
                return saturate(mask);
            }

            float3 SampleSourceColor(float2 uv)
            {
                return SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb;
            }

            float Luma(float3 color)
            {
                return dot(color, float3(0.2126, 0.7152, 0.0722));
            }

            float OrganicPapyrusCoverage(float2 uv, out float edgeBand, out float textureMask, out float revealEdgeMask)
            {
                return AOG_GuildLensPaperCoverageAnimated(
                    uv,
                    _GuildLensGlobalStrength,
                    _GuildLensPapyrusStrength,
                    _GuildLensCameraPapyrusMask,
                    _GuildLensPapyrusReveal,
                    _GuildLensMotionDrift,
                    _GuildLensMapReveal,
                    _GuildLensPapyrusEdgeSoftness,
                    _GuildLensPapyrusEdgeNoiseStrength,
                    _GuildLensPapyrusEdgeAnimationSpeed,
                    edgeBand,
                    textureMask,
                    revealEdgeMask);
            }

            float PapyrusVisualMask(float paperMask)
            {
                return AOG_GuildLensVisualMask(paperMask);
            }

            float ScreenFocusProtectionMask(float2 uv)
            {
                return AOG_GuildLensScreenFocusMask(
                    uv,
                    _GuildLensFocusScreenPoint,
                    _GuildLensFocusEllipse,
                    _GuildLensFocusScreenSoftness,
                    _GuildLensFocusDriverActive);
            }

            float FocusEffectGate(float screenFocusMask)
            {
                return AOG_GuildLensFocusEffectGate(
                    screenFocusMask,
                    _GuildLensFocusProtectionStrength,
                    _GuildLensFocusFalloffPower);
            }

            float WaterFreezeDebugMask(float2 uv, float focusGate)
            {
                float waterFreezeMask = AOG_GuildLensWaterFreezeMaskAnimated(
                    uv,
                    _GuildLensGlobalStrength,
                    _GuildLensPapyrusStrength,
                    _GuildLensCameraPapyrusMask,
                    _GuildLensPapyrusReveal,
                    _GuildLensMotionDrift,
                    _GuildLensMapReveal,
                    _GuildLensPapyrusEdgeSoftness,
                    _GuildLensPapyrusEdgeNoiseStrength,
                    _GuildLensPapyrusEdgeAnimationSpeed,
                    _GuildLensWaterLensStrength,
                    _GuildLensWaterMotionSuppression);
                return saturate(waterFreezeMask * _GuildLensWaterPapyrusOverrideStrength * focusGate);
            }

            void AccumulateCartoonSample(float2 uv, float2 offset, inout float3 sum, inout float3 sumSq, inout float count)
            {
                float3 sampleColor = SampleSourceColor(uv + offset);
                sum += sampleColor;
                sumSq += sampleColor * sampleColor;
                count += 1.0;
            }

            CartoonStats BuildCartoonSector(float2 uv, float2 quadrant, float2 texel)
            {
                float2 sx = float2(quadrant.x * texel.x, 0.0);
                float2 sy = float2(0.0, quadrant.y * texel.y);
                float3 sum = 0.0;
                float3 sumSq = 0.0;
                float count = 0.0;

                AccumulateCartoonSample(uv, float2(0.0, 0.0), sum, sumSq, count);
                AccumulateCartoonSample(uv, sx, sum, sumSq, count);
                AccumulateCartoonSample(uv, sy, sum, sumSq, count);
                AccumulateCartoonSample(uv, sx + sy, sum, sumSq, count);
                AccumulateCartoonSample(uv, sx * 2.0 + sy, sum, sumSq, count);
                AccumulateCartoonSample(uv, sx + sy * 2.0, sum, sumSq, count);

                CartoonStats stats;
                stats.mean = sum / count;
                float3 variance = max(sumSq / count - stats.mean * stats.mean, 0.0);
                stats.variance = dot(variance, float3(0.333333, 0.333333, 0.333333));
                return stats;
            }

            float3 PainterlyNeighborhoodColor(float2 uv, float2 texel)
            {
                CartoonStats sectorA = BuildCartoonSector(uv, float2(1.0, 1.0), texel);
                CartoonStats sectorB = BuildCartoonSector(uv, float2(-1.0, 1.0), texel);
                CartoonStats sectorC = BuildCartoonSector(uv, float2(1.0, -1.0), texel);
                CartoonStats sectorD = BuildCartoonSector(uv, float2(-1.0, -1.0), texel);

                float3 selectedColor = sectorA.mean;
                float selectedVariance = sectorA.variance;
                if (sectorB.variance < selectedVariance)
                {
                    selectedColor = sectorB.mean;
                    selectedVariance = sectorB.variance;
                }
                if (sectorC.variance < selectedVariance)
                {
                    selectedColor = sectorC.mean;
                    selectedVariance = sectorC.variance;
                }
                if (sectorD.variance < selectedVariance)
                {
                    selectedColor = sectorD.mean;
                }
                return selectedColor;
            }

            float PosterizeValue(float value, float steps)
            {
                float levels = max(1.0, steps - 1.0);
                return floor(saturate(value) * levels + 0.5) / levels;
            }

            float3 PosterizePainterlyColor(float3 color)
            {
                float luminance = max(0.0001, Luma(color));
                float posterizedLuma = PosterizeValue(luminance, _GuildLensCartoonValueSteps);
                float3 valueColor = saturate(color * (posterizedLuma / luminance));
                float3 bandedColor = float3(
                    PosterizeValue(valueColor.r, _GuildLensCartoonColorSteps),
                    PosterizeValue(valueColor.g, _GuildLensCartoonColorSteps),
                    PosterizeValue(valueColor.b, _GuildLensCartoonColorSteps));
                return saturate(lerp(valueColor, bandedColor, 0.62));
            }

            float ComputeCartoonEdgeSignal(float2 uv, float3 centerColor, float2 texel)
            {
                float centerLuma = Luma(centerColor);
                float3 rightColor = SampleSourceColor(uv + float2(texel.x, 0.0));
                float3 leftColor = SampleSourceColor(uv - float2(texel.x, 0.0));
                float3 upColor = SampleSourceColor(uv + float2(0.0, texel.y));
                float3 downColor = SampleSourceColor(uv - float2(0.0, texel.y));

                float edge = abs(centerLuma - Luma(rightColor));
                edge = max(edge, abs(centerLuma - Luma(leftColor)));
                edge = max(edge, abs(centerLuma - Luma(upColor)));
                edge = max(edge, abs(centerLuma - Luma(downColor)));
                edge = max(edge, distance(centerColor, rightColor) * 0.35);
                edge = max(edge, distance(centerColor, leftColor) * 0.35);
                edge = max(edge, distance(centerColor, upColor) * 0.35);
                edge = max(edge, distance(centerColor, downColor) * 0.35);

                return smoothstep(0.035, 0.22, edge);
            }

            float3 ApplyCartoonSimplification(float3 sourceColor, float2 uv, float visualMask, out float cartoonBlendMask, out float edgePreserveMask)
            {
                float maskedStrength = saturate(visualMask * saturate(_GuildLensCartoonStrength));
                cartoonBlendMask = 0.0;
                edgePreserveMask = 0.0;
                if (maskedStrength <= 0.0001)
                {
                    return sourceColor;
                }

                float2 texel = _BlitTexture_TexelSize.xy * max(0.25, _GuildLensCartoonSampleRadiusPixels);
                float3 painterlyColor = PainterlyNeighborhoodColor(uv, texel);
                float3 posterizedColor = PosterizePainterlyColor(painterlyColor);
                edgePreserveMask = saturate(ComputeCartoonEdgeSignal(uv, sourceColor, texel) * saturate(_GuildLensCartoonEdgePreserve));

                float3 simplifiedColor = lerp(posterizedColor, sourceColor, edgePreserveMask);
                cartoonBlendMask = saturate(maskedStrength * (1.0 - edgePreserveMask * 0.72));
                return lerp(sourceColor, simplifiedColor, cartoonBlendMask);
            }

            float PapyrusMaterialField(float2 uv, out float fiberField, out float stainField, out float grainField)
            {
                float2 paperUV = PapyrusDriftUV(uv) + _GuildLensPapyrusScrollFlow.xy;
                float2 broadWarp = float2(
                    FractalNoise(paperUV * float2(2.4, 1.2) + float2(4.7, 18.2)),
                    FractalNoise(paperUV * float2(1.7, 2.1) + float2(61.4, 7.3))) - 0.5;
                float2 warpedUV = paperUV + broadWarp * 0.105;

                float materialContrast = lerp(1.0, 1.85, saturate(_GuildLensPapyrusContrast * 0.5));
                float stainScale = max(0.001, _GuildLensInkWashScale);
                float cloudA = FractalNoise(warpedUV * float2(stainScale * 0.52, stainScale * 0.24) + float2(19.2, 8.1));
                float cloudB = FractalNoise(warpedUV * float2(stainScale * 1.12, stainScale * 0.42) + float2(73.6, 31.7));
                stainField = saturate((cloudA * 0.72 + cloudB * 0.28 - 0.5) * materialContrast + 0.5);

                float fiberWarp = FractalNoise(warpedUV * float2(14.0, 4.0) + float2(2.1, 91.5));
                float fiberCoord = warpedUV.y * 250.0 + fiberWarp * 10.0;
                float longFiber = ValueNoise(float2(warpedUV.x * 20.0, fiberCoord));
                float crossFiber = ValueNoise(float2(warpedUV.x * 58.0 + warpedUV.y * 8.0, warpedUV.y * 72.0 + 43.0));
                fiberField = saturate((longFiber * 0.76 + crossFiber * 0.24 - 0.5) * materialContrast + 0.5);

                grainField = FractalNoise(warpedUV * float2(430.0, 360.0) + float2(11.9, 67.4));
                grainField = saturate((grainField - 0.5) * materialContrast + 0.5);
                return saturate((stainField * 0.48 + fiberField * 0.34 + grainField * 0.18 - 0.5) * lerp(1.0, 1.24, saturate(_GuildLensPapyrusContrast * 0.5)) + 0.5);
            }

            float PapyrusMaterialMask(float visualMask)
            {
                return saturate(visualMask * saturate(_GuildLensPapyrusMaterialStrength) * (1.0 + saturate(_GuildLensPapyrusScrollFlow.z)));
            }

            float PapyrusTextureMaterialMask(float textureMask, float visualMask)
            {
                return saturate(lerp(textureMask, visualMask, 0.52) * saturate(_GuildLensPapyrusMaterialStrength) * (1.0 + saturate(_GuildLensPapyrusScrollFlow.z)));
            }

            float AmbientPaperVignetteMask(float2 uv, float mainStylizationMask)
            {
                float2 centered = uv * 2.0 - 1.0;
                float vignetteRadius = lerp(0.25, 1.1, saturate(_GuildLensPaperVignetteRadius));
                float radial = smoothstep(vignetteRadius, 1.38, length(centered));
                float corner = pow(saturate(abs(centered.x) * abs(centered.y)), 0.72);
                float horizon = smoothstep(0.70, 1.0, uv.y) * saturate(_GuildLensHorizonPaperStrength);
                float edgeNoise = FractalNoise(PapyrusDriftUV(uv) * float2(6.5, 3.7) + float2(12.8, 44.1));
                float paperEdge = saturate(max(radial, corner * 0.82) + horizon * 0.34);
                paperEdge *= lerp(0.78, 1.18, edgeNoise);
                float ambientStrength = saturate(_GuildLensAmbientPaperVignetteStrength) * saturate(_GuildLensPaperVignetteStrength);
                return saturate(paperEdge * ambientStrength * (1.0 - saturate(mainStylizationMask) * 0.65));
            }

            float3 ApplyAmbientPaperVignette(float3 baseColor, float2 uv, float ambientMask)
            {
                if (ambientMask <= 0.0001)
                {
                    return baseColor;
                }

                float fiberField;
                float stainField;
                float grainField;
                float textureField = PapyrusMaterialField(uv, fiberField, stainField, grainField);
                float3 authoredPaper = saturate(_GuildLensPapyrusColor.rgb);
                float aging = saturate(_GuildLensPapyrusAgingDarkening);
                float3 agedPaper = authoredPaper * lerp(0.92, 0.72, aging);
                float sourceLuma = Luma(baseColor);
                float3 paperTarget = agedPaper * lerp(0.82, 1.08, sourceLuma);
                float paperTexture = (textureField - 0.5) * 0.055 + (stainField - 0.5) * 0.065 + (grainField - 0.5) * 0.025;
                float2 centered = uv * 2.0 - 1.0;
                float cornerStain = pow(saturate(abs(centered.x) * abs(centered.y)), 0.62) * lerp(0.65, 1.15, stainField);

                float3 color = lerp(baseColor, paperTarget, ambientMask * 0.34);
                color += paperTexture * ambientMask;
                color *= 1.0 - ambientMask * (0.08 + cornerStain * 0.16);
                return saturate(color);
            }

            float3 ApplyBasePapyrus(
                float3 baseColor,
                float2 uv,
                float visualMask,
                float edgeBand,
                float textureMask,
                out float papyrusTextureDebug,
                out float papyrusMaterialMask)
            {
                float3 color = baseColor;
                papyrusMaterialMask = PapyrusMaterialMask(visualMask);

                float fiberField;
                float stainField;
                float grainField;
                papyrusTextureDebug = PapyrusMaterialField(uv, fiberField, stainField, grainField);
                float textureMaterialMask = PapyrusTextureMaterialMask(textureMask, visualMask);

                float luminance = Luma(color);
                float overlayOpacity = saturate(_GuildLensPapyrusOverlayOpacity);
                float blendStrength = saturate(_GuildLensPapyrusBlendStrength);
                float3 authoredPaper = saturate(_GuildLensPapyrusColor.rgb);
                float aging = saturate(_GuildLensPapyrusAgingDarkening);
                float3 agedPaper = authoredPaper * lerp(1.0, 0.68, aging);
                agedPaper = lerp(agedPaper, agedPaper * float3(1.04, 0.98, 0.88), aging * 0.35);

                float readableLuma = lerp(0.72, 1.18, smoothstep(0.0, 1.0, luminance));
                float3 paperTarget = agedPaper * readableLuma;
                float tintAmount = papyrusMaterialMask * overlayOpacity * blendStrength * saturate(_GuildLensPapyrusTintStrength);
                color = lerp(color, paperTarget, tintAmount);

                float3 unifiedPaper = lerp(color, agedPaper * readableLuma, saturate(_GuildLensPapyrusUnifyStrength));
                float unifyAmount = papyrusMaterialMask * overlayOpacity * blendStrength * saturate(_GuildLensPapyrusUnifyStrength);
                color = lerp(color, unifiedPaper, unifyAmount);

                float currentLuma = Luma(color);
                float3 greyColor = float3(currentLuma, currentLuma, currentLuma);
                float saturationTarget = max(saturate(_GuildLensPapyrusSaturation), saturate(_GuildLensSaturationPreserve) * 0.35);
                color = lerp(color, lerp(greyColor, color, saturationTarget), papyrusMaterialMask * overlayOpacity);

                float fiberAmount = textureMaterialMask * saturate(_GuildLensPapyrusFiberStrength);
                float stainAmount = lerp(textureMaterialMask, papyrusMaterialMask, 0.62) * saturate(_GuildLensPapyrusStainStrength);
                float textureResponse = lerp(0.85, 1.75, saturate(_GuildLensPapyrusContrast * 0.5));
                float paperTexture = (fiberField - 0.5) * 0.30 * textureResponse * fiberAmount;
                paperTexture += (grainField - 0.5) * 0.14 * textureResponse * fiberAmount;
                paperTexture += (stainField - 0.5) * 0.32 * textureResponse * stainAmount;
                paperTexture *= overlayOpacity;
                color += float3(paperTexture, paperTexture, paperTexture);

                float blueWashShape = smoothstep(0.12, 0.92, 1.0 - uv.y);
                blueWashShape *= lerp(0.72, 1.16, stainField);
                float lowerCoolWash = blueWashShape * papyrusMaterialMask * overlayOpacity * saturate(_GuildLensPapyrusBlueWashStrength) * 0.55;
                color = lerp(color, lerp(authoredPaper, float3(0.44, 0.51, 0.60), 0.72), lowerCoolWash);

                float washSoftness = max(0.001, _GuildLensInkWashSoftness);
                float washCloud = smoothstep(0.54 - washSoftness * 0.24, 0.54 + washSoftness * 0.24, stainField);
                float washAmount = washCloud * papyrusMaterialMask * overlayOpacity * saturate(_GuildLensInkWashStrength) * 0.20;
                float3 washTarget = lerp(color * float3(0.90, 0.88, 0.82), saturate(_GuildLensInkWashColor.rgb), 0.20);
                color = lerp(color, washTarget, washAmount);

                float contrastAmount = papyrusMaterialMask * overlayOpacity * (1.0 - saturate(_GuildLensContrastPreserve)) * 0.16;
                float3 flatColor = (color - 0.5) * 0.82 + 0.5;
                color = lerp(color, flatColor, contrastAmount);

                float edgeDarken = edgeBand * papyrusMaterialMask * overlayOpacity * saturate(_GuildLensPapyrusEdgeDarkening);
                color *= 1.0 - edgeDarken;

                return saturate(color);
            }

            void ComputeFocusDebug(float2 uv, float rawDepth, out float focusMask, out float screenFocusMask, out float depthFocusMask)
            {
                float eyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float focusDistance = lerp(
                    max(0.01, _GuildLensFocusFallbackDistance),
                    max(0.01, _GuildLensFocusDistance),
                    step(0.5, _GuildLensFocusDriverActive));

                float halfRange = max(0.01, _GuildLensFocusRangeWidth * 0.5);
                float transition = max(0.0001, _GuildLensTransitionSoftness);
                depthFocusMask = 1.0 - smoothstep(halfRange, halfRange + transition, abs(eyeDepth - focusDistance));

                screenFocusMask = ScreenFocusProtectionMask(uv);
                focusMask = screenFocusMask;
            }

            float EffectiveWaterBoundaryOutlineWidth()
            {
                return lerp(
                    max(0.25, _GuildLensOutlineWidthPixels),
                    max(0.25, _GuildLensWaterBoundaryOutlineWidthPixels),
                    saturate(_GuildLensWaterBoundaryOutlineThicknessInfluence));
            }

            float ExclusionBoundaryDebug(float2 uv, float centerMask)
            {
                float2 texel = _BlitTexture_TexelSize.xy * EffectiveWaterBoundaryOutlineWidth();
                float boundary = abs(centerMask - SampleExclusionMask(uv + float2(texel.x, 0.0)));
                boundary = max(boundary, abs(centerMask - SampleExclusionMask(uv - float2(texel.x, 0.0))));
                boundary = max(boundary, abs(centerMask - SampleExclusionMask(uv + float2(0.0, texel.y))));
                boundary = max(boundary, abs(centerMask - SampleExclusionMask(uv - float2(0.0, texel.y))));
                return saturate(boundary);
            }

            float RawSceneOutlineMaskAt(float2 uv, float radiusPixels, float thresholdScale, float featherScale)
            {
                float2 texel = _BlitTexture_TexelSize.xy * max(0.25, radiusPixels);
                float3 centerColor = SampleSourceColor(uv);
                float centerLuma = Luma(centerColor);

                float edge = 0.0;
                float3 sampleColor = SampleSourceColor(uv + float2(1.0, 0.0) * texel);
                edge = max(edge, abs(centerLuma - Luma(sampleColor)) * 1.8 + distance(centerColor, sampleColor) * 0.42);
                sampleColor = SampleSourceColor(uv + float2(-1.0, 0.0) * texel);
                edge = max(edge, abs(centerLuma - Luma(sampleColor)) * 1.8 + distance(centerColor, sampleColor) * 0.42);
                sampleColor = SampleSourceColor(uv + float2(0.0, 1.0) * texel);
                edge = max(edge, abs(centerLuma - Luma(sampleColor)) * 1.8 + distance(centerColor, sampleColor) * 0.42);
                sampleColor = SampleSourceColor(uv + float2(0.0, -1.0) * texel);
                edge = max(edge, abs(centerLuma - Luma(sampleColor)) * 1.8 + distance(centerColor, sampleColor) * 0.42);
                sampleColor = SampleSourceColor(uv + float2(0.7071, 0.7071) * texel);
                edge = max(edge, abs(centerLuma - Luma(sampleColor)) * 1.8 + distance(centerColor, sampleColor) * 0.42);
                sampleColor = SampleSourceColor(uv + float2(-0.7071, 0.7071) * texel);
                edge = max(edge, abs(centerLuma - Luma(sampleColor)) * 1.8 + distance(centerColor, sampleColor) * 0.42);
                sampleColor = SampleSourceColor(uv + float2(0.7071, -0.7071) * texel);
                edge = max(edge, abs(centerLuma - Luma(sampleColor)) * 1.8 + distance(centerColor, sampleColor) * 0.42);
                sampleColor = SampleSourceColor(uv + float2(-0.7071, -0.7071) * texel);
                edge = max(edge, abs(centerLuma - Luma(sampleColor)) * 1.8 + distance(centerColor, sampleColor) * 0.42);

                float contrast = lerp(0.55, 2.65, saturate(_GuildLensOutlineContrast * 0.5));
                edge *= contrast;
                float softness = saturate(_GuildLensOutlineEdgeSoftness);
                float threshold = lerp(0.075, 0.028, saturate(_GuildLensOutlineContrast * 0.5)) * max(0.05, thresholdScale);
                float feather = lerp(0.018, 0.16, softness) * max(0.05, featherScale);
                return smoothstep(max(0.001, threshold - feather * 0.45), threshold + feather, edge);
            }

            float RawSceneOutlineMask(float2 uv)
            {
                return RawSceneOutlineMaskAt(uv, _GuildLensOutlineWidthPixels, 1.0, 1.0);
            }

            float WaterBoundaryOutlineMask(float2 uv)
            {
                float centerMask = SampleExclusionMask(uv);
                float boundary = ExclusionBoundaryDebug(uv, centerMask);
                float sidePreference = saturate(_GuildLensWaterBoundaryOutlineSide);
                float bothSides = 1.0 - abs(sidePreference - 0.5) * 2.0;
                float sideMask = max(saturate(1.0 - abs(centerMask - sidePreference) * 2.0), saturate(bothSides));
                return saturate(boundary * sideMask * saturate(_GuildLensWaterBoundaryOutlineStrength));
            }

            float2 EstimateSourceEdgeTangent(float2 uv)
            {
                float2 texel = _BlitTexture_TexelSize.xy * max(1.0, _GuildLensOutlineWidthPixels);
                float leftLuma = Luma(SampleSourceColor(uv - float2(texel.x, 0.0)));
                float rightLuma = Luma(SampleSourceColor(uv + float2(texel.x, 0.0)));
                float downLuma = Luma(SampleSourceColor(uv - float2(0.0, texel.y)));
                float upLuma = Luma(SampleSourceColor(uv + float2(0.0, texel.y)));
                float2 gradient = float2(rightLuma - leftLuma, upLuma - downLuma);
                float gradientLength = max(0.0001, length(gradient));
                return gradientLength > 0.001 ? normalize(float2(-gradient.y, gradient.x)) : float2(0.866, 0.5);
            }

            void BuildInkStrokeMasks(float2 uv, out float coreStroke, out float bodyStroke, out float bleedStroke, out float coastlineBoost)
            {
                float baseWidth = max(0.25, _GuildLensOutlineWidthPixels);
                float bodyWidth = baseWidth * lerp(1.45, 2.25, saturate(_GuildLensOutlineOrganicStrength));
                float bleedWidth = max(_GuildLensOutlineBleedWidthPixels, bodyWidth * 1.25);

                float coreEdge = RawSceneOutlineMaskAt(uv, baseWidth, 0.92, 0.72);
                float bodyEdge = RawSceneOutlineMaskAt(uv, bodyWidth, 0.68, 1.18);
                float bleedEdge = RawSceneOutlineMaskAt(uv, bleedWidth, 0.48, lerp(1.25, 2.45, saturate(_GuildLensOutlineBleedSoftness)));
                coastlineBoost = WaterBoundaryOutlineMask(uv);

                coreStroke = saturate(coreEdge);
                bodyStroke = saturate(bodyEdge);
                bleedStroke = saturate(bleedEdge);
            }

            float OrganicOutlineField(float2 uv)
            {
                float scale = max(0.001, _GuildLensOutlineNoiseScale);
                float phase = _GuildLensMapReveal.w * max(0.0, _GuildLensOutlineAnimationSpeed);
                float2 inkUV = PapyrusDriftUV(uv) + _GuildLensPapyrusScrollFlow.xy * 0.73 + float2(phase * 0.013, -phase * 0.021);
                float2 warp = float2(
                    FractalNoise(inkUV * float2(scale * 0.28, scale * 0.16) + float2(11.1, 37.7)),
                    FractalNoise(inkUV * float2(scale * 0.18, scale * 0.31) + float2(71.4, 5.3))) - 0.5;
                float2 warpedUV = inkUV + warp * lerp(0.045, 0.11, saturate(_GuildLensOutlineOrganicStrength));
                float broad = FractalNoise(warpedUV * float2(scale * 0.72, scale * 0.38) + float2(8.6, 92.1));
                float flecks = FractalNoise(warpedUV * float2(scale * 1.75, scale * 1.05) + float2(41.8, 16.5));
                return saturate(broad * 0.72 + flecks * 0.28);
            }

            float OrganicOutlineReveal(float2 uv, float rawOutlineMask, out float leadingEdgeMask, out float timingDebug)
            {
                float inkField = OrganicOutlineField(uv);
                float revealProgress = saturate(_GuildLensMapReveal.x);
                float outlineMotion = saturate(_GuildLensMotion.z + _GuildLensMapReveal.z);
                float revealAmount = saturate(revealProgress + outlineMotion * 0.18);
                float softness = max(0.001, _GuildLensOutlineRevealSoftness);
                float segmentRandomness = saturate(_GuildLensOutlineSegmentRandomness);
                float crawlStrength = saturate(_GuildLensOutlineStrokeCrawlStrength);
                float phase = _GuildLensMapReveal.w * max(0.0, _GuildLensOutlineAnimationSpeed);
                float2 tangent = EstimateSourceEdgeTangent(uv);
                float crawlCoord = dot(uv + _GuildLensPapyrusScrollFlow.xy * 0.37, tangent) * lerp(7.0, 26.0, crawlStrength);
                float crawlNoise = FractalNoise(float2(crawlCoord + phase * 0.52, inkField * 4.7 + phase * 0.11));
                float localTiming = FractalNoise(uv * max(0.001, _GuildLensOutlineNoiseScale) * 1.9 + tangent * 3.1 + float2(19.4, 61.8));
                float timing = lerp(0.5, saturate(inkField * 0.62 + localTiming * 0.28 + crawlNoise * 0.24), segmentRandomness);
                timing = saturate(timing + (crawlNoise - 0.5) * crawlStrength * 0.18);
                timingDebug = timing;

                float crawlReveal = saturate(revealAmount + (crawlNoise - 0.5) * crawlStrength * 0.24);
                float growth = smoothstep(timing - softness, timing + softness, crawlReveal);
                float leadingEdge = 1.0 - smoothstep(0.0, softness * lerp(1.2, 2.75, crawlStrength), abs(timing - crawlReveal));
                leadingEdgeMask = saturate(leadingEdge * rawOutlineMask * _GuildLensOutlineLeadingEdgeStrength * lerp(0.55, 1.25, crawlNoise));
                float bloom = saturate(growth + leadingEdgeMask * saturate(_GuildLensOutlineRevealIntensity) * 0.85);

                float organicStrength = saturate(_GuildLensOutlineOrganicStrength);
                float revealGate = lerp(1.0, bloom, saturate(_GuildLensOutlineRevealStrength));
                float unsettledStroke = lerp(1.0, bloom, organicStrength) * revealGate;
                float stableNoise = lerp(1.0, lerp(0.76, 1.16, inkField), organicStrength * 0.55);
                float settle = saturate(_GuildLensOutlineSettleStrength * revealProgress * (1.0 - outlineMotion * 0.55));
                return saturate(lerp(unsettledStroke, stableNoise, settle));
            }

            float OutlineCoverageGate(float visualMask, float stylizationMask, float focusGate)
            {
                float focusAllowed = lerp(focusGate, 1.0, saturate(_GuildLensOutlineFocusInfluence));
                float papyrusDependence = saturate(_GuildLensOutlinePapyrusDependence) * (1.0 - saturate(_GuildLensOutlineFocusInfluence));
                float paperGate = lerp(visualMask, stylizationMask, papyrusDependence);
                return saturate(paperGate * focusAllowed);
            }

            float OrganicOutlineMask(
                float2 uv,
                float visualMask,
                float stylizationMask,
                float focusGate,
                float edgeBand,
                out float rawOutlineMask,
                out float inkBodyMask,
                out float inkBleedMask,
                out float leadingRevealMask,
                out float timingDebug)
            {
                float coreStroke;
                float bodyStroke;
                float bleedStroke;
                float coastlineBoost;
                BuildInkStrokeMasks(uv, coreStroke, bodyStroke, bleedStroke, coastlineBoost);

                float waterInteriorOutlineGate = 1.0 - SampleExclusionMask(uv);
                coreStroke *= waterInteriorOutlineGate;
                bodyStroke *= waterInteriorOutlineGate;
                bleedStroke *= waterInteriorOutlineGate;

                rawOutlineMask = saturate(max(max(coreStroke, bodyStroke), coastlineBoost * 0.72));

                float coverageGate = OutlineCoverageGate(visualMask, stylizationMask, focusGate);
                float organicReveal = OrganicOutlineReveal(uv, rawOutlineMask, leadingRevealMask, timingDebug);
                float directOpacity = saturate(_GuildLensOutlineColor.a) * saturate(_GuildLensOutlineOpacity) * max(0.0, _GuildLensOutlineStrength);
                float coastlineOpacity = coastlineBoost * coverageGate * organicReveal;
                float edgePooling = saturate(edgeBand * _GuildLensOutlineEdgePoolingStrength * bodyStroke * (1.0 - coastlineBoost * 0.65));

                inkBodyMask = saturate((bodyStroke * 0.62 + coreStroke * 0.72) * coverageGate * organicReveal + coastlineOpacity * 0.24);
                inkBleedMask = saturate((bleedStroke * organicReveal + leadingRevealMask * 0.32 + edgePooling) * coverageGate * saturate(_GuildLensOutlineBleedStrength) * (1.0 - coastlineBoost * 0.55));
                leadingRevealMask = saturate(leadingRevealMask * coverageGate);

                float alpha = saturate(((coreStroke * 1.08) * coverageGate + inkBodyMask * 0.54 + leadingRevealMask * 0.48 + coastlineOpacity * 0.28) * directOpacity);
                return saturate(alpha);
            }

            float3 ApplyOrganicOutline(float3 baseColor, float outlineMask, float inkBleedMask, float leadingRevealMask)
            {
                float3 inkTarget = saturate(_GuildLensOutlineColor.rgb);
                float bleedAmount = saturate(inkBleedMask * _GuildLensOutlineColor.a);
                float3 bleedTarget = lerp(baseColor * 0.82, inkTarget, 0.28);
                float3 color = lerp(baseColor, bleedTarget, bleedAmount);
                float leadingBoost = saturate(leadingRevealMask * _GuildLensOutlineLeadingEdgeStrength * _GuildLensOutlineColor.a);
                color = lerp(color, inkTarget, saturate(outlineMask + leadingBoost * 0.28));
                return saturate(color);
            }

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                float4 source = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0);

                float edgeBand;
                float textureMask;
                float revealEdgeMask;
                float paperMask = OrganicPapyrusCoverage(uv, edgeBand, textureMask, revealEdgeMask);
                float visualMask = PapyrusVisualMask(paperMask);
                float focusProtectionMask = ScreenFocusProtectionMask(uv);
                float focusGate = FocusEffectGate(focusProtectionMask);
                float stylizationMask = saturate(visualMask * focusGate);
                float ambientVignetteMask = AmbientPaperVignetteMask(uv, stylizationMask);
                float focusGatedEdgeBand = edgeBand * focusGate;
                float focusGatedTextureMask = textureMask * focusGate;
                float rawOutlineMask;
                float inkBodyMask;
                float inkBleedMask;
                float leadingRevealMask;
                float outlineTimingDebug;
                float organicOutlineMask = OrganicOutlineMask(
                    uv,
                    visualMask,
                    stylizationMask,
                    focusGate,
                    focusGatedEdgeBand,
                    rawOutlineMask,
                    inkBodyMask,
                    inkBleedMask,
                    leadingRevealMask,
                    outlineTimingDebug);

                if (_GuildLensDebugMode == 1)
                {
                    float rawDepth = SampleSceneDepth(uv);
                    float focusMask;
                    float screenFocusMask;
                    float depthFocusMask;
                    ComputeFocusDebug(uv, rawDepth, focusMask, screenFocusMask, depthFocusMask);
                    return float4(focusMask, screenFocusMask, depthFocusMask, source.a);
                }

                if (_GuildLensDebugMode == 2)
                {
                    return float4(edgeBand, edgeBand, edgeBand, source.a);
                }

                if (_GuildLensDebugMode == 3)
                {
                    return float4(paperMask, paperMask, paperMask, source.a);
                }

                if (_GuildLensDebugMode == 4)
                {
                    return float4(visualMask, visualMask, visualMask, source.a);
                }

                if (_GuildLensDebugMode == 5)
                {
                    return float4(stylizationMask, stylizationMask, stylizationMask, source.a);
                }

                if (_GuildLensDebugMode == 6)
                {
                    float cameraPapyrusMask = saturate(_GuildLensCameraPapyrusMask);
                    return float4(cameraPapyrusMask, cameraPapyrusMask, cameraPapyrusMask, source.a);
                }

                if (_GuildLensDebugMode == 7)
                {
                    float rawDepth = SampleSceneDepth(uv);
                    float focusMask;
                    float screenFocusMask;
                    float depthFocusMask;
                    ComputeFocusDebug(uv, rawDepth, focusMask, screenFocusMask, depthFocusMask);
                    return float4(screenFocusMask, screenFocusMask, screenFocusMask, source.a);
                }

                if (_GuildLensDebugMode == 8)
                {
                    float papyrusRevealMask = PapyrusRevealMask(uv);
                    return float4(papyrusRevealMask, papyrusRevealMask, papyrusRevealMask, source.a);
                }

                if (_GuildLensDebugMode == 9)
                {
                    float exclusionMask = SampleExclusionMask(uv);
                    float expandedExclusionMask = SampleExpandedExclusionMask(uv);
                    float boundary = ExclusionBoundaryDebug(uv, exclusionMask);
                    return float4(expandedExclusionMask, exclusionMask, boundary, source.a);
                }

                if (_GuildLensDebugMode == 10)
                {
                    float debugMotion = max(saturate(_GuildLensMotion.x), max(saturate(_GuildLensMotion.z), saturate(_GuildLensMotion.w)));
                    return float4(debugMotion, debugMotion, debugMotion, source.a);
                }

                float cartoonBlendMask;
                float edgePreserveMask;
                float3 cartoonColor = ApplyCartoonSimplification(source.rgb, uv, stylizationMask, cartoonBlendMask, edgePreserveMask);

                if (_GuildLensDebugMode == 11)
                {
                    return float4(cartoonBlendMask, cartoonBlendMask, cartoonBlendMask, source.a);
                }

                if (_GuildLensDebugMode == 12)
                {
                    return float4(edgePreserveMask, edgePreserveMask, edgePreserveMask, source.a);
                }

                if (_GuildLensDebugMode == 13)
                {
                    return float4(cartoonColor, source.a);
                }

                if (_GuildLensDebugMode == 14)
                {
                    float papyrusFiberDebug;
                    float papyrusStainDebug;
                    float papyrusGrainDebug;
                    float papyrusTextureDebug = PapyrusMaterialField(uv, papyrusFiberDebug, papyrusStainDebug, papyrusGrainDebug);
                    return float4(papyrusTextureDebug, papyrusTextureDebug, papyrusTextureDebug, source.a);
                }

                if (_GuildLensDebugMode == 15)
                {
                    float papyrusMaterialMask = PapyrusMaterialMask(stylizationMask);
                    return float4(papyrusMaterialMask, papyrusMaterialMask, papyrusMaterialMask, source.a);
                }

                if (_GuildLensDebugMode == 16)
                {
                    float waterFreezeMask = WaterFreezeDebugMask(uv, focusGate);
                    return float4(waterFreezeMask, waterFreezeMask, waterFreezeMask, source.a);
                }

                if (_GuildLensDebugMode == 17)
                {
                    return float4(rawOutlineMask, rawOutlineMask, rawOutlineMask, source.a);
                }

                if (_GuildLensDebugMode == 18)
                {
                    return float4(organicOutlineMask, organicOutlineMask, organicOutlineMask, source.a);
                }

                if (_GuildLensDebugMode == 19)
                {
                    return float4(saturate(_GuildLensMapReveal.x), saturate(_GuildLensMapReveal.y), saturate(_GuildLensMapReveal.z), source.a);
                }

                if (_GuildLensDebugMode == 20)
                {
                    float animatedEdge = saturate(revealEdgeMask * focusGate);
                    return float4(animatedEdge, animatedEdge, animatedEdge, source.a);
                }

                if (_GuildLensDebugMode == 21)
                {
                    return float4(inkBodyMask, inkBodyMask, inkBodyMask, source.a);
                }

                if (_GuildLensDebugMode == 22)
                {
                    return float4(leadingRevealMask, outlineTimingDebug, saturate(_GuildLensMapReveal.x), source.a);
                }

                if (_GuildLensDebugMode == 23)
                {
                    return float4(inkBleedMask, inkBleedMask, inkBleedMask, source.a);
                }

                if (_GuildLensDebugMode == 24)
                {
                    float coastlineUnifiedMask = saturate(max(RawSceneOutlineMask(uv), WaterBoundaryOutlineMask(uv) * 0.72));
                    return float4(coastlineUnifiedMask, coastlineUnifiedMask, coastlineUnifiedMask, source.a);
                }

                if (_GuildLensDebugMode == 25)
                {
                    return float4(ambientVignetteMask, ambientVignetteMask, ambientVignetteMask, source.a);
                }

                if (_GuildLensDebugMode == 26)
                {
                    return source;
                }

                if (_GuildLensDebugMode == 27)
                {
                    return source;
                }

                if (_GuildLensDebugMode == 28)
                {
                    float waterOutlineSuppression = saturate(SampleExclusionMask(uv) * stylizationMask);
                    return float4(waterOutlineSuppression, waterOutlineSuppression, waterOutlineSuppression, source.a);
                }

                float finalTextureDebug;
                float finalMaterialMask;
                float3 finalColor = ApplyBasePapyrus(cartoonColor, uv, stylizationMask, focusGatedEdgeBand, focusGatedTextureMask, finalTextureDebug, finalMaterialMask);
                finalColor = ApplyAmbientPaperVignette(finalColor, uv, ambientVignetteMask);
                finalColor = ApplyOrganicOutline(finalColor, organicOutlineMask, inkBleedMask, leadingRevealMask);
                return float4(finalColor, source.a);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Guild Lens Stencil Exclusion Mask"

            ZWrite Off
            ZTest Always
            Cull Off

            Stencil
            {
                Ref 1
                Comp Equal
                Pass Keep
            }

            HLSLPROGRAM
            #pragma vertex StencilMaskVert
            #pragma fragment StencilMaskFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct StencilMaskVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            StencilMaskVaryings StencilMaskVert(uint vertexID : SV_VertexID)
            {
                StencilMaskVaryings output;
                float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
                output.positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
                return output;
            }

            half4 StencilMaskFrag(StencilMaskVaryings input) : SV_Target
            {
                return half4(1.0, 1.0, 1.0, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Guild Lens Miniature Focus Blur"

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment MiniatureFocusFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Assets/Shaders/Include/GuildLensMask.hlsl"

            float _GuildLensGlobalStrength;
            float _GuildLensPapyrusStrength;
            float _GuildLensCameraPapyrusMask;
            float4 _GuildLensPapyrusReveal;
            float4 _GuildLensMotionDrift;
            float4 _GuildLensMapReveal;
            float _GuildLensPapyrusEdgeSoftness;
            float _GuildLensPapyrusEdgeNoiseStrength;
            float _GuildLensPapyrusEdgeAnimationSpeed;
            float _GuildLensFocusDriverActive;
            float4 _GuildLensFocusScreenPoint;
            float4 _GuildLensFocusEllipse;
            float _GuildLensFocusScreenSoftness;
            float _GuildLensMiniatureFocusEnabled;
            float _GuildLensMiniatureFocusStrength;
            float _GuildLensMiniatureFocusRadiusPixels;
            float _GuildLensMiniatureFocusSoftness;
            float _GuildLensMiniatureFocusZoomInfluence;
            float _GuildLensMiniatureFocusPapyrusProtection;
            float _GuildLensMiniatureFocusEdgeBias;
            int _GuildLensDebugMode;

            float MiniaturePapyrusProtection(float2 uv)
            {
                float edgeBand;
                float textureMask;
                float revealEdgeMask;
                float paperMask = AOG_GuildLensPaperCoverageAnimated(
                    uv,
                    _GuildLensGlobalStrength,
                    _GuildLensPapyrusStrength,
                    _GuildLensCameraPapyrusMask,
                    _GuildLensPapyrusReveal,
                    _GuildLensMotionDrift,
                    _GuildLensMapReveal,
                    _GuildLensPapyrusEdgeSoftness,
                    _GuildLensPapyrusEdgeNoiseStrength,
                    _GuildLensPapyrusEdgeAnimationSpeed,
                    edgeBand,
                    textureMask,
                    revealEdgeMask);
                float visualMask = AOG_GuildLensVisualMask(paperMask);
                return lerp(1.0, 1.0 - visualMask, saturate(_GuildLensMiniatureFocusPapyrusProtection));
            }

            float MiniatureFocusBlurMask(float2 uv)
            {
                float focusDriverUsable = step(0.5, _GuildLensFocusDriverActive) * step(0.5, _GuildLensFocusScreenPoint.z);
                float4 focusPoint = _GuildLensFocusScreenPoint;
                focusPoint.xy = lerp(float2(0.5, 0.5), focusPoint.xy, focusDriverUsable);
                focusPoint.z = 1.0;
                focusPoint.w = 1.0;

                float4 focusEllipse = _GuildLensFocusEllipse;
                focusEllipse.xy = max(focusEllipse.xy, float2(0.001, 0.001));
                float focusSoftness = max(0.0001, lerp(_GuildLensFocusScreenSoftness, _GuildLensMiniatureFocusSoftness, 0.65));
                float focusMask = AOG_GuildLensScreenFocusMask(uv, focusPoint, focusEllipse, focusSoftness, 1.0);
                float outOfFocusMask = 1.0 - focusMask;

                float2 centered = uv * 2.0 - 1.0;
                float edgeMask = smoothstep(0.45, 1.32, length(centered));
                outOfFocusMask = saturate(outOfFocusMask + edgeMask * saturate(_GuildLensMiniatureFocusEdgeBias));

                float zoomFactor = lerp(1.0, saturate(_GuildLensMapReveal.x), saturate(_GuildLensMiniatureFocusZoomInfluence));
                float papyrusProtection = MiniaturePapyrusProtection(uv);
                return saturate(
                    outOfFocusMask *
                    saturate(_GuildLensMiniatureFocusEnabled) *
                    saturate(_GuildLensMiniatureFocusStrength) *
                    saturate(_GuildLensGlobalStrength) *
                    zoomFactor *
                    papyrusProtection);
            }

            float3 SampleMiniatureBlur(float2 uv, float radiusPixels)
            {
                float2 texel = _BlitTexture_TexelSize.xy * radiusPixels;
                float2 diag = texel * 0.70710678;

                float3 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0).rgb * 0.20;
                color += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(texel.x, 0.0), 0).rgb * 0.12;
                color += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - float2(texel.x, 0.0), 0).rgb * 0.12;
                color += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(0.0, texel.y), 0).rgb * 0.12;
                color += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - float2(0.0, texel.y), 0).rgb * 0.12;
                color += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + diag, 0).rgb * 0.08;
                color += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv - diag, 0).rgb * 0.08;
                color += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(diag.x, -diag.y), 0).rgb * 0.08;
                color += SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv + float2(-diag.x, diag.y), 0).rgb * 0.08;
                return color;
            }

            float4 MiniatureFocusFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;
                float4 source = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, 0);

                if (_GuildLensDebugMode != 0 && _GuildLensDebugMode != 29)
                {
                    return source;
                }

                float blurMask = MiniatureFocusBlurMask(uv);
                if (_GuildLensDebugMode == 29)
                {
                    return float4(blurMask, blurMask, blurMask, source.a);
                }

                if (blurMask <= 0.0001)
                {
                    return source;
                }

                float radiusPixels = max(0.25, _GuildLensMiniatureFocusRadiusPixels);
                float3 blurred = SampleMiniatureBlur(uv, radiusPixels);
                return float4(lerp(source.rgb, blurred, blurMask), source.a);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
