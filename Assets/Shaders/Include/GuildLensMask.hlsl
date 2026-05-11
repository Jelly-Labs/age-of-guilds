#ifndef AGE_OF_GUILDS_GUILD_LENS_MASK_INCLUDED
#define AGE_OF_GUILDS_GUILD_LENS_MASK_INCLUDED

float AOG_GuildLensHash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float AOG_GuildLensValueNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    float a = AOG_GuildLensHash21(i);
    float b = AOG_GuildLensHash21(i + float2(1.0, 0.0));
    float c = AOG_GuildLensHash21(i + float2(0.0, 1.0));
    float d = AOG_GuildLensHash21(i + float2(1.0, 1.0));

    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float AOG_GuildLensFractalNoise(float2 uv)
{
    float n = 0.0;
    n += AOG_GuildLensValueNoise(uv) * 0.5;
    n += AOG_GuildLensValueNoise(uv * 2.07 + 13.1) * 0.3;
    n += AOG_GuildLensValueNoise(uv * 4.11 + 41.7) * 0.2;
    return n;
}

float2 AOG_GuildLensPapyrusDriftUV(float2 uv, float4 motionDrift)
{
    return uv + motionDrift.xy * motionDrift.z;
}

float AOG_GuildLensPapyrusRevealMask(float2 uv, float4 papyrusReveal, float4 motionDrift)
{
    float strength = saturate(papyrusReveal.y);
    if (strength <= 0.0001)
    {
        return 1.0;
    }

    float progress = saturate(papyrusReveal.x);
    float scale = max(0.001, papyrusReveal.z);
    float softness = max(0.001, papyrusReveal.w);
    float2 driftedUV = AOG_GuildLensPapyrusDriftUV(uv, motionDrift);
    float broadNoise = AOG_GuildLensFractalNoise(driftedUV * scale + float2(17.3, 42.1));
    float fineNoise = AOG_GuildLensFractalNoise(driftedUV * scale * 2.17 + float2(73.4, 9.8));
    float revealNoise = saturate(broadNoise * 0.78 + fineNoise * 0.22);
    float threshold = lerp(1.0 + softness, -softness, progress);
    float reveal = smoothstep(threshold - softness, threshold + softness, revealNoise);
    return lerp(1.0, reveal, strength);
}

float AOG_GuildLensPaperCoverageAnimated(
    float2 uv,
    float globalStrength,
    float papyrusStrength,
    float cameraPapyrusMask,
    float4 papyrusReveal,
    float4 motionDrift,
    float4 mapReveal,
    float papyrusEdgeSoftness,
    float papyrusEdgeNoiseStrength,
    float papyrusEdgeAnimationSpeed,
    out float edgeBand,
    out float textureMask,
    out float revealEdgeMask)
{
    float revealStrength = saturate(papyrusReveal.y);
    float revealProgress = saturate(max(papyrusReveal.x, mapReveal.x));
    float zoomReveal = saturate(max(cameraPapyrusMask, mapReveal.x));
    float animatedCameraMask = lerp(zoomReveal, revealProgress, revealStrength * 0.35);
    float activeMask = saturate(saturate(globalStrength) * saturate(papyrusStrength) * animatedCameraMask);
    float revealMotion = saturate(mapReveal.z + motionDrift.w * 0.35);
    float edgeAnimationPhase = mapReveal.w * max(0.0, papyrusEdgeAnimationSpeed);

    float2 driftedUV = AOG_GuildLensPapyrusDriftUV(uv, motionDrift);
    float2 animatedUV = driftedUV + float2(edgeAnimationPhase * 0.017, -edgeAnimationPhase * 0.011);
    float2 warpUV = animatedUV * 2.35;
    float2 warp = float2(
        AOG_GuildLensFractalNoise(warpUV + float2(11.3, 47.8)),
        AOG_GuildLensFractalNoise(warpUV + float2(71.9, 6.4))) - 0.5;
    float edgeNoiseStrength = saturate(papyrusEdgeNoiseStrength);
    float2 organicUV = animatedUV + warp * lerp(0.055, 0.13, edgeNoiseStrength);

    float broadNoise = AOG_GuildLensFractalNoise(organicUV * float2(3.1, 1.45) + float2(21.7, 8.9));
    float stainNoise = AOG_GuildLensFractalNoise(organicUV * float2(7.2, 3.35) + float2(4.6, 36.1));
    float tornNoise = AOG_GuildLensFractalNoise(organicUV * float2(15.5, 6.2) + float2(59.0, 13.2));

    float reach = activeMask * 1.28;
    float boundary = 1.14 - reach;
    boundary += (broadNoise - 0.5) * lerp(0.11, 0.32, edgeNoiseStrength);
    boundary += (stainNoise - 0.5) * lerp(0.05, 0.16, edgeNoiseStrength);
    boundary += (tornNoise - 0.5) * lerp(0.015, 0.065, edgeNoiseStrength);
    boundary += (AOG_GuildLensFractalNoise(animatedUV * float2(1.1, 0.72) + float2(edgeAnimationPhase * 0.07, edgeAnimationPhase * 0.043)) - 0.5) * revealMotion * 0.08;

    float softnessControl = saturate(papyrusEdgeSoftness);
    float softness = lerp(0.025, 0.18, softnessControl);
    softness *= lerp(0.78, 1.34, stainNoise);
    softness *= lerp(0.9, 1.45, revealMotion);

    float coverage = smoothstep(boundary - softness, boundary + softness, uv.y);
    float revealNoise = AOG_GuildLensPapyrusRevealMask(uv + float2(edgeAnimationPhase * 0.004, -edgeAnimationPhase * 0.003), papyrusReveal, motionDrift);
    coverage *= lerp(1.0, lerp(0.62, 1.0, revealNoise), revealStrength);

    float stainVariation = lerp(0.82, 1.08, AOG_GuildLensFractalNoise(organicUV * float2(5.0, 2.2) + float2(93.7, 93.7)));
    coverage = saturate(coverage * stainVariation * activeMask);
    edgeBand = saturate(1.0 - abs(uv.y - boundary) / max(0.001, softness * 3.8)) * coverage;
    revealEdgeMask = saturate(1.0 - abs(uv.y - boundary) / max(0.001, softness * 1.65));
    revealEdgeMask *= saturate(activeMask * lerp(0.7, 1.35, revealNoise));
    revealEdgeMask = saturate(max(edgeBand * 0.75, revealEdgeMask));
    textureMask = saturate(coverage * lerp(0.14, 0.34, edgeBand) * lerp(0.92, 1.18, revealNoise));
    return coverage;
}

float AOG_GuildLensPaperCoverage(
    float2 uv,
    float globalStrength,
    float papyrusStrength,
    float cameraPapyrusMask,
    float4 papyrusReveal,
    float4 motionDrift,
    out float edgeBand,
    out float textureMask)
{
    float revealEdgeMask;
    return AOG_GuildLensPaperCoverageAnimated(
        uv,
        globalStrength,
        papyrusStrength,
        cameraPapyrusMask,
        papyrusReveal,
        motionDrift,
        float4(cameraPapyrusMask, cameraPapyrusMask, 0.0, 0.0),
        papyrusReveal.w,
        0.55,
        0.0,
        edgeBand,
        textureMask,
        revealEdgeMask);
}

float AOG_GuildLensVisualMask(float paperMask)
{
    return saturate(pow(saturate(paperMask), 0.55) * 1.35);
}

float AOG_GuildLensScreenFocusMask(
    float2 uv,
    float4 focusScreenPoint,
    float4 focusEllipse,
    float focusScreenSoftness,
    float focusDriverActive)
{
    float screenFocusStrength = saturate(focusScreenPoint.w * focusScreenPoint.z * step(0.5, focusDriverActive));
    if (screenFocusStrength <= 0.0001)
    {
        return 0.0;
    }

    float2 screenFocusDelta = uv - focusScreenPoint.xy;
    float rotationSin;
    float rotationCos;
    sincos(focusEllipse.z, rotationSin, rotationCos);
    float2 rotatedFocusDelta = float2(
        screenFocusDelta.x * rotationCos + screenFocusDelta.y * rotationSin,
        -screenFocusDelta.x * rotationSin + screenFocusDelta.y * rotationCos);
    float2 screenFocusRadii = max(focusEllipse.xy, float2(0.001, 0.001));
    float screenFocusDistance = length(rotatedFocusDelta / screenFocusRadii);
    float ellipseSoftness = max(0.0001, focusScreenSoftness) / max(0.001, min(screenFocusRadii.x, screenFocusRadii.y));
    float screenFocusMask = 1.0 - smoothstep(1.0, 1.0 + ellipseSoftness, screenFocusDistance);
    return saturate(screenFocusMask * screenFocusStrength);
}

float AOG_GuildLensFocusEffectGate(
    float screenFocusMask,
    float focusProtectionStrength,
    float focusFalloffPower)
{
    float protectedFocus = saturate(screenFocusMask * saturate(focusProtectionStrength));
    float gate = saturate(1.0 - protectedFocus);
    return pow(gate, max(0.001, focusFalloffPower));
}

float AOG_GuildLensWaterFreezeMaskAnimated(
    float2 uv,
    float globalStrength,
    float papyrusStrength,
    float cameraPapyrusMask,
    float4 papyrusReveal,
    float4 motionDrift,
    float4 mapReveal,
    float papyrusEdgeSoftness,
    float papyrusEdgeNoiseStrength,
    float papyrusEdgeAnimationSpeed,
    float waterLensStrength,
    float waterMotionSuppression)
{
    float edgeBand;
    float textureMask;
    float revealEdgeMask;
    float paperMask = AOG_GuildLensPaperCoverageAnimated(
        uv,
        globalStrength,
        papyrusStrength,
        cameraPapyrusMask,
        papyrusReveal,
        motionDrift,
        mapReveal,
        papyrusEdgeSoftness,
        papyrusEdgeNoiseStrength,
        papyrusEdgeAnimationSpeed,
        edgeBand,
        textureMask,
        revealEdgeMask);
    return saturate(AOG_GuildLensVisualMask(paperMask) * saturate(waterLensStrength) * saturate(waterMotionSuppression));
}

float AOG_GuildLensWaterFreezeMask(
    float2 uv,
    float globalStrength,
    float papyrusStrength,
    float cameraPapyrusMask,
    float4 papyrusReveal,
    float4 motionDrift,
    float waterLensStrength,
    float waterMotionSuppression)
{
    return AOG_GuildLensWaterFreezeMaskAnimated(
        uv,
        globalStrength,
        papyrusStrength,
        cameraPapyrusMask,
        papyrusReveal,
        motionDrift,
        float4(cameraPapyrusMask, cameraPapyrusMask, 0.0, 0.0),
        papyrusReveal.w,
        0.55,
        0.0,
        waterLensStrength,
        waterMotionSuppression);
}

#endif
