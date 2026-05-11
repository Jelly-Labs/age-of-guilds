#ifndef GLOBAL_CLOUD_SHADOWS_INCLUDED
#define GLOBAL_CLOUD_SHADOWS_INCLUDED

#include "Assets/Shaders/Include/CloudNoise.hlsl"

// Global Variables injected dynamically by the script monitoring the main Puff Clouds layer
float _GlobalCloudScale;
float _GlobalCloudCoverage;
float _GlobalCloudSoftness;
float _GlobalCloudDeform;
float4 _GlobalCloudWind;
half4 _GlobalCloudShadowTint;

float GetGlobalCloudShadowRawMask(float3 positionWS)
{
    // Evaluates purely the opaque mask of the cloud hovering over this world position
    return GetCloudMask(positionWS.xz, _GlobalCloudScale, _GlobalCloudWind.xy, _Time.y, _GlobalCloudCoverage, _GlobalCloudSoftness, _GlobalCloudDeform);
}

half3 ApplyGlobalCloudShadow(half3 baseColor, float3 positionWS)
{
    float shadowMask = GetGlobalCloudShadowRawMask(positionWS);
    
    // Darken the terrain/water pixel directly based on the mask
    half3 shadowedColor = baseColor * _GlobalCloudShadowTint.rgb;
    return lerp(baseColor, shadowedColor, shadowMask * _GlobalCloudShadowTint.a);
}

#endif
