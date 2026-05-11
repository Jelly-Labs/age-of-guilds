Shader "Custom/URPWater"
{
    Properties
    {
        _BaseColor("Deep Water Color", Color) = (0.02, 0.18, 0.42, 1)

        [Header(Water Depth)]
        // Main depth zones plus an independent super-shallow shoreline overlay.
        _SuperShallowColor("Zone 0  Super Shallow Shore Ring", Color) = (0.62, 0.88, 0.82, 1)
        _SuperShallowStrength("Zone 0 Ring Strength", Range(0.0, 1.0)) = 0.35
        _SuperShallowWidth("Zone 0 Ring Width", Range(0.0, 0.25)) = 0.035
        _SuperShallowSoftness("Zone 0 Ring Softness", Range(0.001, 0.2)) = 0.025
        _SuperShallowShorePull("Zone 0 Shore Pull Toward Land", Range(-0.05, 0.05)) = 0.006
        _ShallowColor("Zone 1  Shoreline", Color) = (0.18, 0.76, 0.80, 1)
        _MidShallowColor("Zone 2  Shallow", Color) = (0.05, 0.52, 0.66, 1)
        _MidDeepColor("Zone 3  Mid Ocean", Color) = (0.03, 0.32, 0.55, 1)
        _DepthZone1("Zone 1 to 2 Threshold", Range(0.0, 1.0)) = 0.07
        _DepthZone2("Zone 2 to 3 Threshold", Range(0.0, 1.0)) = 0.22
        _DepthZone3("Zone 3 to 4 Threshold", Range(0.0, 1.0)) = 0.55
        _DepthSoftness("Zone Blend Softness", Range(0.001, 0.15)) = 0.04
        _DepthStrength("Depth Blend Strength", Range(0.0, 1.0)) = 0.9
        _DepthNoiseScale("Depth Noise Scale", Float) = 0.14
        _DepthNoiseStrength("Depth Noise Strength", Range(0.0, 0.2)) = 0.05
        _ShallowCalmRef("Shallow Calm Distance (Flow Map)", Range(0.001, 0.5)) = 0.05
        _ShallowCalmSoftness("Calm Transition Smoothness", Range(0.001, 0.5)) = 0.05
        _ShallowCalmNoise("Calm Boundary Randomizer", Range(0.0, 0.1)) = 0.02
        
        [Header(Wave Layers)]
        _NormalMap1("Normal Map 1", 2D) = "bump" {}
        _RippleStrength("Ripple Normal Strength", Range(0, 2)) = 1.0
        _ScrollSpeed1("Scroll Speed 1", Vector) = (0.02, 0.02, 0, 0)
        
        [NoScaleOffset] _NormalMap2("Normal Map 2 (Swell)", 2D) = "bump" {}
        _ScrollSpeed2("Scroll Speed 2", Vector) = (-0.015, 0.015, 0, 0)
        
        [Header(Anti Tiling Blend)]
        _BlendNoiseScale("Spatial Blend Scale", Range(0.001, 0.2)) = 0.05
        
        [Header(Macro Swell)]
        _SwellScale("Swell World Scale", Range(0.001, 0.12)) = 0.01
        _SwellSpeed("Swell Speed", Vector) = (0.005, 0.003, 0, 0)
        _SwellStrength("Swell Normal Strength", Range(0, 1.5)) = 0.4
        
        [Header(Visuals)]
        _NormalStrength("Normal Strength", Range(0, 2)) = 1.0
        _Smoothness("Smoothness", Range(0, 1)) = 0.95
        
        [Header(Distortion)]
        _WarpScale("Distortion Strength", Range(0, 0.2)) = 0.03
        _WarpSpeed("Distortion Speed", Range(0, 3.0)) = 1.0
        _WarpFrequency("Distortion Size", Range(0.001, 0.1)) = 0.02
        
        [Header(Coastal Waves)]
        _CoastalFlowMap("Coastal Flow Map (RGB)", 2D) = "black" {}
        _WaveSpeed("Wave Speed", Range(0, 10)) = 2.0
        _WaveCount("Wave Count", Range(1, 150)) = 50.0
        _FoamColor("Foam Color", Color) = (1, 1, 1, 1)
        _FoamThickness("Foam Band Thickness", Range(0, 1)) = 0.05
        
        [Header(Wave Profile (ADSR))]
        _WaveCrestStart("1. Crest Attack Start", Range(0, 1)) = 0.6
        _WaveCrestPeak("2. Crest Peak (Attack End)", Range(0, 1)) = 0.75
        _WaveCrestDrop("3. Crest Sustain End", Range(0, 1)) = 0.85
        _WaveCrestEnd("4. Wave Dissipate (Release)", Range(0, 1)) = 1.0
        
        [Header(Wave Irregularity)]
        _WaveWobble("Phase Wobble (Snake Effect)", Range(0, 1)) = 0.1
        _WaveDistortion("Domain Warp (Currents)", Range(0, 0.05)) = 0.01

        [Header(Open Water Whitecaps)]
        _WhitecapStrength("Whitecap Strength", Range(0, 1)) = 0.28
        _WhitecapDensity("Whitecap Density", Range(0, 1)) = 0.32
        _WhitecapScale("Whitecap Patch Scale", Range(0.001, 0.2)) = 0.045
        _WhitecapSpeed("Whitecap Drift Speed", Range(0, 2)) = 0.06
        _WhitecapNormalThreshold("Whitecap Slope Threshold", Range(0, 1)) = 0.24
        _WhitecapSoftness("Whitecap Softness", Range(0.001, 0.5)) = 0.22
        _WhitecapColor("Whitecap Color", Color) = (1, 1, 1, 1)

        [Header(Atmosphere Height Fog)]
        _AtmosFogColor("Fog Haze Color", Color) = (0.62, 0.74, 0.85, 1)
        _AtmosFogStart("Fog Start Distance", Float) = 30.0
        _AtmosFogEnd("Fog Full Distance", Float) = 120.0
        _AtmosFogStrength("Fog Strength", Range(0.0, 1.0)) = 0.6
        
        [Header(Dynamic Cloud Shadows)]
        _WaterShadowColor ("Shadow Darkening Color", Color) = (0.65, 0.65, 0.75, 1.0)
        _WaterShadowIntensity ("Shadow Overall Opacity Multiplier", Range(0, 2)) = 1.0
        _ShadowSmoothness ("Shadow Smoothness (Glossiness under Clouds)", Range(0, 1)) = 0.85
        _SunQuenchPower ("Sun Specular Quench Power", Range(0.1, 1000)) = 500.0

        [Header(Water Sun Glint Shape)]
        _WaterSunGlintSize ("Sun Glint Size", Range(0, 1)) = 0.38
        _WaterSunGlintStrength ("Sun Glint Strength", Range(0, 2)) = 0.65
        _WaterSunGlintCap ("Sun Glint Brightness Cap", Range(0.25, 5)) = 0.92
        _WaterSunGlintThreshold ("Sun Glint Threshold", Range(0, 2)) = 0.68
        _WaterSunGlintSoftness ("Sun Glint Softness", Range(0.001, 2)) = 0.45
        _WaterSunGlintCompression ("Sun Glint Compression", Range(0, 1)) = 0.72

        [Header(Realtime Cast Shadows On Water)]
        _WaterRealtimeShadowColor ("Cast Shadow Tint", Color) = (0.32, 0.38, 0.46, 1)
        _WaterRealtimeShadowStrength ("Cast Shadow Strength", Range(0, 2)) = 0.45
        _WaterRealtimeShadowContrast ("Cast Shadow Contrast", Range(0.25, 4)) = 1.1
        _WaterRealtimeShadowSoftness ("Cast Shadow Edge Softness", Range(0.001, 1)) = 0.18
        _WaterRealtimeShadowSpecularSuppression ("Cast Shadow Glint Suppression", Range(0, 1)) = 0.55
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+10" "RenderPipeline"="UniversalPipeline" }

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Shaders/Include/GlobalCloudShadows.hlsl"
            #include "Assets/Shaders/Include/GuildLensMask.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 tangentWS   : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float2 uv          : TEXCOORD5;
                float4 screenPos   : TEXCOORD6;
            };

            TEXTURE2D(_NormalMap1); SAMPLER(sampler_NormalMap1);
            TEXTURE2D(_NormalMap2); SAMPLER(sampler_NormalMap2);
            TEXTURE2D(_CoastalFlowMap); SAMPLER(sampler_CoastalFlowMap);
            TEXTURE2D(_GuildLensWaterTexture); SAMPLER(sampler_GuildLensWaterTexture);
            TEXTURE2D(_GuildLensJournalTexture); SAMPLER(sampler_GuildLensJournalTexture);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                // Depth zones
                half4 _SuperShallowColor;
                half4 _ShallowColor;
                half4 _MidShallowColor;
                half4 _MidDeepColor;
                float _SuperShallowStrength;
                float _SuperShallowWidth;
                float _SuperShallowSoftness;
                float _SuperShallowShorePull;
                float _DepthZone1;
                float _DepthZone2;
                float _DepthZone3;
                float _DepthSoftness;
                float _DepthStrength;
                float _ShallowCalmRef;
                float _ShallowCalmSoftness;
                float _ShallowCalmNoise;
                float _DepthNoiseScale;
                float _DepthNoiseStrength;
                float4 _NormalMap1_ST;
                float4 _NormalMap2_ST;
                float4 _ScrollSpeed1;
                float4 _ScrollSpeed2;
                
                float _BlendNoiseScale;
                float _SwellScale;
                float4 _SwellSpeed;
                float _SwellStrength;
                
                float _NormalStrength;
                float _RippleStrength;
                float _Smoothness;
                float _WarpScale;
                float _WarpSpeed;
                float _WarpFrequency;
                float4 _CoastalFlowMap_ST;
                float _WaveSpeed;
                float _WaveCount;
                half4 _FoamColor;
                float _FoamThickness;
                float _WaveCrestStart;
                float _WaveCrestPeak;
                float _WaveCrestDrop;
                float _WaveCrestEnd;
                float _WaveWobble;
                float _WaveDistortion;
                float _WhitecapStrength;
                float _WhitecapDensity;
                float _WhitecapScale;
                float _WhitecapSpeed;
                float _WhitecapNormalThreshold;
                float _WhitecapSoftness;
                half4 _WhitecapColor;

                // Atmosphere fog (matches IslandShader for consistent horizon)
                half4  _AtmosFogColor;
                float  _AtmosFogStart;
                float  _AtmosFogEnd;
                float  _AtmosFogStrength;
                
                // Cloud Shadow custom overrides
                half4 _WaterShadowColor;
                float _WaterShadowIntensity;
                float _ShadowSmoothness;
                float _SunQuenchPower;
                float _WaterSunGlintSize;
                float _WaterSunGlintStrength;
                float _WaterSunGlintCap;
                float _WaterSunGlintThreshold;
                float _WaterSunGlintSoftness;
                float _WaterSunGlintCompression;
                half4 _WaterRealtimeShadowColor;
                float _WaterRealtimeShadowStrength;
                float _WaterRealtimeShadowContrast;
                float _WaterRealtimeShadowSoftness;
                float _WaterRealtimeShadowSpecularSuppression;
            CBUFFER_END

            float _GuildLensFocusFallbackDistance;
            float _GuildLensFocusRangeWidth;
            float _GuildLensTransitionSoftness;
            float _GuildLensGlobalStrength;
            float _GuildLensNearStrength;
            float _GuildLensFarStrength;
            float _GuildLensFarHazeStrength;
            float _GuildLensFarHazeRange;
            float _GuildLensPapyrusStrength;
            float _GuildLensPaperVignetteStrength;
            float _GuildLensPaperVignetteRadius;
            float _GuildLensHorizonPaperStrength;
            float _GuildLensFocusDistance;
            float _GuildLensFocusDriverActive;
            float4 _GuildLensFocusScreenPoint;
            float4 _GuildLensFocusEllipse;
            float _GuildLensFocusScreenSoftness;
            float _GuildLensFocusProtectionStrength;
            float _GuildLensFocusFalloffPower;
            float4 _GuildLensMapReveal;
            float4 _GuildLensOutlineColor;
            float _GuildLensWaterLensStrength;
            float _GuildLensWaterTextureEnabled;
            float4 _GuildLensWaterTextureParams;
            float4 _GuildLensWaterTextureStyle;
            float _GuildLensJournalTextureEnabled;
            float4 _GuildLensJournalTextureParams;
            float4 _GuildLensJournalTextureStyle;
            float _GuildLensWaterMotionSuppression;
            float _GuildLensWaterPapyrusOverrideStrength;
            float _GuildLensWaterPapyrusBlueStrength;
            float _GuildLensWaterPapyrusMatteSmoothness;
            float4 _GuildLensPapyrusColor;
            float _GuildLensPapyrusBlueWashStrength;
            float _GuildLensCameraPapyrusMask;
            float4 _GuildLensPapyrusReveal;
            float4 _GuildLensMotionDrift;
            float _GuildLensPapyrusEdgeSoftness;
            float _GuildLensPapyrusEdgeNoiseStrength;
            float _GuildLensPapyrusEdgeAnimationSpeed;
            float _GuildLensDebugMode;

            // ── Lightweight value noise for depth zone randomizer ────────────────────
            float hashW(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float vnoiseW(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hashW(i), hashW(i + float2(1,0)), u.x),
                            lerp(hashW(i + float2(0,1)), hashW(i + float2(1,1)), u.x), u.y);
            }

            float GuildLensWaterMask(float2 screenUV, float eyeDepth)
            {
                float edgeBand;
                float textureMask;
                float revealEdgeMask;
                float paperMask = AOG_GuildLensPaperCoverageAnimated(
                    screenUV,
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
                float screenFocusMask = AOG_GuildLensScreenFocusMask(
                    screenUV,
                    _GuildLensFocusScreenPoint,
                    _GuildLensFocusEllipse,
                    _GuildLensFocusScreenSoftness,
                    _GuildLensFocusDriverActive);
                float focusGate = AOG_GuildLensFocusEffectGate(
                    screenFocusMask,
                    _GuildLensFocusProtectionStrength,
                    _GuildLensFocusFalloffPower);
                return saturate(AOG_GuildLensVisualMask(paperMask) * _GuildLensWaterLensStrength * focusGate);
            }

            float2 RotateWaterUV(float2 uv, float angle)
            {
                float s;
                float c;
                sincos(angle, s, c);
                return float2(uv.x * c - uv.y * s, uv.x * s + uv.y * c);
            }

            float TextureInkMask(float4 texel, float contrast, float intensity)
            {
                float luma = dot(texel.rgb, float3(0.2126, 0.7152, 0.0722));
                float alpha = texel.a;
                float contrastResponse = lerp(2.5, 9.0, saturate(contrast * 0.5));
                float darkInk = saturate((0.62 - luma) * contrastResponse) * alpha;
                float softPaperInk = saturate(1.0 - luma) * alpha * 0.16;
                float ink = saturate(max(darkInk, softPaperInk));
                return saturate(ink * max(0.0, intensity));
            }

            float SampleWaterTextureLayer(float2 posXZ)
            {
                float opacity = saturate(_GuildLensWaterTextureParams.x) * step(0.5, _GuildLensWaterTextureEnabled);
                float scale = max(0.0001, _GuildLensWaterTextureParams.y);
                float density = saturate(_GuildLensWaterTextureParams.z);
                float seed = _GuildLensWaterTextureParams.w;
                float rotation = _GuildLensWaterTextureStyle.x;
                float randomRotation = saturate(_GuildLensWaterTextureStyle.y);
                float contrast = _GuildLensWaterTextureStyle.z;
                float intensity = _GuildLensWaterTextureStyle.w;

                float2 cell = floor(posXZ * scale * 0.18 + seed);
                float rnd = hashW(cell + seed);
                float angle = rotation + (rnd - 0.5) * 6.28318 * randomRotation;
                float2 uv = RotateWaterUV(posXZ * scale + seed * 0.137, angle);
                float4 texel = SAMPLE_TEXTURE2D(_GuildLensWaterTexture, sampler_GuildLensWaterTexture, uv);
                float coverage = lerp(0.35, 1.0, density);
                return saturate(TextureInkMask(texel, contrast, intensity) * opacity * coverage);
            }

            float SampleJournalTextureLayer(float2 posXZ)
            {
                float opacity = saturate(_GuildLensJournalTextureParams.x) * step(0.5, _GuildLensJournalTextureEnabled);
                float scale = max(0.0001, _GuildLensJournalTextureParams.y);
                float density = saturate(_GuildLensJournalTextureParams.z);
                float seed = _GuildLensJournalTextureParams.w;
                float rotation = _GuildLensJournalTextureStyle.x;
                float randomRotation = saturate(_GuildLensJournalTextureStyle.y);
                float contrast = _GuildLensJournalTextureStyle.z;
                float intensity = _GuildLensJournalTextureStyle.w;

                float2 cellUV = posXZ * scale * 0.55 + seed;
                float2 cell = floor(cellUV);
                float2 local = frac(cellUV) - 0.5;
                float rnd = hashW(cell + seed * 1.71);
                float active = step(1.0 - density, rnd);
                float angle = rotation + (hashW(cell + seed * 4.3) - 0.5) * 6.28318 * randomRotation;
                float2 uv = RotateWaterUV(local, angle) + 0.5;
                float inside = step(0.0, uv.x) * step(uv.x, 1.0) * step(0.0, uv.y) * step(uv.y, 1.0);
                float4 texel = SAMPLE_TEXTURE2D(_GuildLensJournalTexture, sampler_GuildLensJournalTexture, uv);
                float edgeFade = smoothstep(0.0, 0.18, min(min(uv.x, uv.y), min(1.0 - uv.x, 1.0 - uv.y)));
                return saturate(TextureInkMask(texel, contrast, intensity) * opacity * active * inside * edgeFade);
            }

            float WaterLuma(float3 color)
            {
                return dot(color, float3(0.2126, 0.7152, 0.0722));
            }

            half3 GuildLensStaticWaterColor(float2 posXZ, half3 naturalColor, float edgeMask)
            {
                float blueStrength = saturate(_GuildLensWaterPapyrusBlueStrength) * saturate(_GuildLensPapyrusBlueWashStrength);
                half3 paperColor = saturate(_GuildLensPapyrusColor.rgb);
                half3 mapBlue = lerp(_BaseColor.rgb, half3(0.42, 0.50, 0.60), saturate(_GuildLensWaterPapyrusBlueStrength));
                half3 paperWater = lerp(paperColor * 0.72, mapBlue, lerp(0.58, 0.86, blueStrength));

                float broadStain = vnoiseW(posXZ * 0.014 + float2(43.4, 13.1));
                float shallowStain = vnoiseW(posXZ * 0.043 + float2(7.7, 81.2));
                float fineGrain = vnoiseW(posXZ * 0.19 + float2(5.3, 23.1));
                float paperShade = lerp(0.88, 1.08, broadStain);
                paperShade += (shallowStain - 0.5) * 0.045 + (fineGrain - 0.5) * 0.035;

                half3 mapColor = saturate(paperWater * paperShade);
                mapColor = lerp(mapColor, naturalColor * 0.35 + paperColor * 0.65, 0.08);
                mapColor *= 1.0 - saturate(edgeMask) * 0.12;
                return saturate(mapColor);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                
                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS    = normalInput.normalWS;
                OUT.tangentWS   = normalInput.tangentWS;
                OUT.bitangentWS = normalInput.bitangentWS;
                
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 screenUV = IN.screenPos.xy / max(0.0001, IN.screenPos.w);
                float eyeDepth = max(0.01, IN.screenPos.w);
                float waterLensVisualMask = GuildLensWaterMask(screenUV, eyeDepth);
                float waterMotionMask = saturate(waterLensVisualMask * _GuildLensWaterMotionSuppression);
                float waterFlattenMask = saturate(waterLensVisualMask * _GuildLensWaterPapyrusOverrideStrength);
                float waterLensInterior = waterFlattenMask;
                float waterLensEdge = saturate(waterFlattenMask * (1.0 - waterFlattenMask));
                float stillness = saturate(max(waterMotionMask, waterLensInterior));
                float waterTime = _Time.y * (1.0 - stillness);
                float normalLensScale = lerp(1.0, 0.0, stillness);
                float warpLensScale = lerp(_WarpScale, 0.0, stillness);
                float waveDistortionLens = lerp(_WaveDistortion, 0.0, stillness);
                float freq = _WarpFrequency;
                float spd = _WarpSpeed;
                float2 posXZ = IN.positionWS.xz;
                float waterTextureMask = SampleWaterTextureLayer(posXZ) * waterFlattenMask;
                float journalTextureMask = SampleJournalTextureLayer(posXZ) * waterFlattenMask;

                if (abs(_GuildLensDebugMode - 26.0) < 0.5)
                {
                    return half4(waterTextureMask, waterTextureMask, waterTextureMask, 1.0);
                }

                if (abs(_GuildLensDebugMode - 27.0) < 0.5)
                {
                    return half4(journalTextureMask, journalTextureMask, journalTextureMask, 1.0);
                }
                
                // Macroscopic procedural UV distortion to break tiling
                float2 warp = float2(
                    sin(IN.positionWS.x * freq + waterTime * (0.1 * spd)) + sin(IN.positionWS.z * (freq * 1.65) + waterTime * (0.15 * spd)),
                    cos(IN.positionWS.x * (freq * 1.35) - waterTime * (0.12 * spd)) + cos(IN.positionWS.z * (freq * 0.95) + waterTime * (0.08 * spd))
                );
                
                // Use World Space position for noise sampling to break up tiling independently of geometry scale

                // 1. Calculate Panning UVs with the warp offset applied
                float2 baseUV = IN.uv * _NormalMap1_ST.xy + _NormalMap1_ST.zw + warp * warpLensScale;
                
                // Create 3 rotated variations to eliminate visible tiling
                float2 uv1 = baseUV + (waterTime * _ScrollSpeed1.xy);
                // Rotated 90 deg: (x,y) -> (-y, x), slightly different scale and speed
                float2 uv2 = float2(-baseUV.y, baseUV.x) * 0.8 + (waterTime * _ScrollSpeed2.xy);
                // Rotated 45 deg: (x-y, x+y)*0.707
                float2 uv3 = float2(baseUV.x - baseUV.y, baseUV.x + baseUV.y) * 0.707 * 1.2 - (waterTime * _ScrollSpeed1.xy * 0.5);

                // 2. Spatial blend weights based on low-frequency noise
                float noise1 = vnoiseW(posXZ * _BlendNoiseScale);
                float noise2 = vnoiseW(posXZ * _BlendNoiseScale + float2(123.4, 567.8));
                
                float w1 = smoothstep(0.3, 0.7, noise1);
                float w2 = smoothstep(0.3, 0.7, noise2) * (1.0 - w1);
                float w3 = 1.0 - (w1 + w2);

                // === Fake Water Depth (via Coastal Flow Map) ===
                // We use the CoastalFlowMap's distance-to-coast to fake shallow zones
                float2 baseFlowUV = IN.uv * _CoastalFlowMap_ST.xy + _CoastalFlowMap_ST.zw;
                float fakeDepth = SAMPLE_TEXTURE2D(_CoastalFlowMap, sampler_CoastalFlowMap, baseFlowUV).b;
                
                // Add natural noise variation to the shallow depth boundary
                float boundaryNoise = vnoiseW(IN.positionWS.xz * _DepthNoiseScale) - 0.5;
                float randomizedDepth = saturate(fakeDepth + boundaryNoise * _ShallowCalmNoise);
                
                // SAFEGUARD & SOFT TRANSITION
                float safeCalmRef = max(0.001, _ShallowCalmRef);
                float depthWaveMask = smoothstep(max(0.0, safeCalmRef - _ShallowCalmSoftness), safeCalmRef + _ShallowCalmSoftness, randomizedDepth);

                // 3. Sample main normal map at 3 orientations
                half4 nMapA = SAMPLE_TEXTURE2D(_NormalMap1, sampler_NormalMap1, uv1);
                half4 nMapB = SAMPLE_TEXTURE2D(_NormalMap1, sampler_NormalMap1, uv2);
                half4 nMapC = SAMPLE_TEXTURE2D(_NormalMap1, sampler_NormalMap1, uv3);

                float rippleNormalStrength = _NormalStrength * _RippleStrength * depthWaveMask * normalLensScale;
                half3 nA = UnpackNormalScale(nMapA, rippleNormalStrength);
                half3 nB = UnpackNormalScale(nMapB, rippleNormalStrength);
                half3 nC = UnpackNormalScale(nMapC, rippleNormalStrength);
                
                // Average the base local normal direction
                half3 blendedLocalNormal = normalize(nA * w1 + nB * w2 + nC * w3);

                // 4. Large-scale Swell (using NormalMap2) mapped via world-space to avoid tiling entirely across the vast ocean
                float2 swellUV = posXZ * _SwellScale + (waterTime * _SwellSpeed.xy);
                half4 nMapSwell = SAMPLE_TEXTURE2D(_NormalMap2, sampler_NormalMap2, swellUV);
                float swellNormalStrength = _NormalStrength * _SwellStrength * normalLensScale;
                half3 nSwell = UnpackNormalScale(nMapSwell, swellNormalStrength);

                // Final Normal Blend
                half3 tangentNormal = normalize(half3(blendedLocalNormal.xy + nSwell.xy, max(0.001, blendedLocalNormal.z * nSwell.z)));
                tangentNormal = normalize(lerp(tangentNormal, half3(0.0, 0.0, 1.0), waterLensInterior));

                // Transform Tangent space normal to World space
                half3 normalWS = TransformTangentToWorld(tangentNormal, half3x3(IN.tangentWS, IN.bitangentWS, IN.normalWS));
                normalWS = normalize(lerp(normalWS, normalize(IN.normalWS), waterLensInterior));

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                inputData.shadowMask = half4(1.0, 1.0, 1.0, 1.0);
                inputData.fogCoord = 0.0;
                inputData.vertexLighting = half3(0.0, 0.0, 0.0);
                Light mainLight = GetMainLight(inputData.shadowCoord);
                float realtimeShadowRaw = saturate(1.0 - mainLight.shadowAttenuation);
                
                // --- Coastal Directional Waves ---
                float2 flowUV = IN.uv * _CoastalFlowMap_ST.xy + _CoastalFlowMap_ST.zw;
                
                // IDEA 2: Domain Warping (Distort the coordinates using the surface normal noise before finding distance)
                flowUV += tangentNormal.xy * waveDistortionLens;
                
                float4 flowData = SAMPLE_TEXTURE2D(_CoastalFlowMap, sampler_CoastalFlowMap, flowUV);
                float rawDistToCoast = flowData.b;

                // Inject the boundary noise into the coastal waves so their rings break and warp organically
                float distToCoast = saturate(rawDistToCoast + boundaryNoise * _ShallowCalmNoise);

                // IDEA 1: Phase Warping (Add macroscopic sine wave wobble to the phase timing)
                float wavePhase = frac(distToCoast * _WaveCount + waterTime * _WaveSpeed + (warp.x + warp.y) * _WaveWobble);
                
                // ADSR Envelope for wave shape: 
                // Attack (Start to Peak), Sustain (Peak to Drop), Release (Drop to End)
                float crest = smoothstep(_WaveCrestStart, _WaveCrestPeak, wavePhase) * 
                              smoothstep(_WaveCrestEnd, _WaveCrestDrop, wavePhase);
                crest *= 1.0 - waterLensInterior;

                // Fade out into deep ocean so waves are only visible near coast
                float coastAlpha = 1.0 - smoothstep(0.001, _FoamThickness, distToCoast);
                
                // Compute final color
                half3 finalColor = lerp(_BaseColor.rgb, _FoamColor.rgb, crest * coastAlpha * _FoamColor.a);

                // ── DEPTH COLOR + INDEPENDENT SUPER-SHALLOW OVERLAY ─────────────
                // distToCoast is our free depth proxy (0=shore, 1=ocean).
                // Noise warp breaks up the perfectly geometric zone rings.
                float depthNoise  = vnoiseW(IN.positionWS.xz * _DepthNoiseScale) - 0.5;
                float warpedDist  = saturate(distToCoast + depthNoise * _DepthNoiseStrength);
                float zone1 = max(0.0001, _DepthZone1);
                float zone2 = max(zone1 + 0.0001, _DepthZone2);
                float zone3 = max(zone2 + 0.0001, _DepthZone3);
                float zoneSoftness = max(0.001, _DepthSoftness);

                // Shoreline → Shallow → Mid → Deep. Zone 0 is applied separately below.
                half3 depthColor  = _ShallowColor.rgb;
                depthColor = lerp(depthColor, _MidShallowColor.rgb,
                             smoothstep(zone1, zone1 + zoneSoftness, warpedDist));
                depthColor = lerp(depthColor, _MidDeepColor.rgb,
                             smoothstep(zone2, zone2 + zoneSoftness, warpedDist));
                depthColor = lerp(depthColor, _BaseColor.rgb,
                             smoothstep(zone3, zone3 + zoneSoftness, warpedDist));

                // Preserve foam on top; depth only affects base water
                float foamMask = crest * coastAlpha * _FoamColor.a * lerp(1.0, 0.0, waterLensInterior);
                finalColor = lerp(depthColor, _FoamColor.rgb, foamMask) * _DepthStrength
                           + finalColor * (1.0 - _DepthStrength);

                // Sparse open-water breaks: gated by depth, normal slope, and procedural patch noise.
                // This adds occasional sea-life without becoming another shoreline foam system.
                float whitecapDeepMask = smoothstep(0.18, 0.72, distToCoast);
                float whitecapSlope = saturate(length(tangentNormal.xy) * 2.75);
                float whitecapSlopeMask = smoothstep(
                    _WhitecapNormalThreshold,
                    _WhitecapNormalThreshold + max(0.001, _WhitecapSoftness),
                    whitecapSlope);
                float2 whitecapDrift = normalize(float2(0.82, 0.57)) * waterTime * _WhitecapSpeed;
                float whitecapPatch = vnoiseW(posXZ * _WhitecapScale + whitecapDrift);
                float whitecapShard = vnoiseW(posXZ * (_WhitecapScale * 4.7) - whitecapDrift * 1.9);
                float whitecapDensityThreshold = lerp(0.98, 0.58, saturate(_WhitecapDensity));
                float whitecapNoiseMask = smoothstep(
                    whitecapDensityThreshold,
                    whitecapDensityThreshold + max(0.02, _WhitecapSoftness * 0.35),
                    whitecapPatch + whitecapShard * 0.22);
                float whitecapMask = whitecapDeepMask
                    * whitecapSlopeMask
                    * whitecapNoiseMask
                    * saturate(_WhitecapStrength)
                    * _WhitecapColor.a
                    * (1.0 - waterLensInterior);
                finalColor = lerp(finalColor, _WhitecapColor.rgb, whitecapMask);

                float superShallowWidth = max(0.0001, _SuperShallowWidth);
                float superShallowSoftness = max(0.0001, _SuperShallowSoftness);
                float superShallowDist = max(0.0, rawDistToCoast + _SuperShallowShorePull);
                float superShallowMask = 1.0 - smoothstep(
                    superShallowWidth,
                    superShallowWidth + superShallowSoftness,
                    superShallowDist);
                finalColor = lerp(finalColor, _SuperShallowColor.rgb, superShallowMask * saturate(_SuperShallowStrength));

                // ── ATMOSPHERE HEIGHT FOG ────────────────────────────────────
                // Matches the island shader fog exactly — set same values on both
                // materials and the horizon will read as one continuous atmosphere.
                float camDist  = length(IN.positionWS - _WorldSpaceCameraPos.xyz);
                float fogRange = max(_AtmosFogEnd - _AtmosFogStart, 0.001);
                float fogT     = saturate((camDist - _AtmosFogStart) / fogRange);
                finalColor = lerp(finalColor, _AtmosFogColor.rgb, fogT * _AtmosFogStrength);
                
                // --- DYNAMIC CLOUD SHADOW PRE-EVALUATION ---
                float cloudShadowRaw = GetGlobalCloudShadowRawMask(IN.positionWS);
                
                // Combine global animated shadow opacity with our local Water Material intensity dial
                float shadowPower = saturate(cloudShadowRaw * _GlobalCloudShadowTint.a * _WaterShadowIntensity);
                shadowPower *= 1.0 - waterLensInterior;

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = finalColor;
                surfaceData.metallic = 0.0;
                float lensSmoothness = lerp(_Smoothness, 0.18, stillness);
                float surfaceFoamMask = saturate(foamMask + whitecapMask);
                float foamSmoothness = lerp(lensSmoothness, 0.24, surfaceFoamMask);
                surfaceData.smoothness = lerp(lerp(foamSmoothness, _ShadowSmoothness, shadowPower), _GuildLensWaterPapyrusMatteSmoothness, waterLensInterior);
                surfaceData.normalTS = half3(0.0, 0.0, 1.0);
                surfaceData.emission = half3(0.0, 0.0, 0.0);
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = 1.0;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                
                // --- CUSTOM WATER SHADOW TWEAKING ---
                
                // 1. Base organic color darkening (Using local WaterShadowColor instead of the Global Tint)
                half3 shadowedColor = color.rgb * _WaterShadowColor.rgb;
                color.rgb = lerp(color.rgb, shadowedColor, shadowPower);

                // 1b. Real-time cast shadows from islands/ships: separate art controls from cloud shadows.
                float realtimeShadowMask = smoothstep(0.0, max(0.001, _WaterRealtimeShadowSoftness), realtimeShadowRaw);
                realtimeShadowMask = pow(saturate(realtimeShadowMask), max(0.25, _WaterRealtimeShadowContrast));
                realtimeShadowMask *= saturate(_WaterRealtimeShadowStrength) * (1.0 - waterLensInterior);
                half3 castShadowedColor = color.rgb * _WaterRealtimeShadowColor.rgb;
                color.rgb = lerp(color.rgb, castShadowedColor, realtimeShadowMask);
                
                // 2. HDR SUN REFLECTION QUENCHER (Using local SunQuenchPower)
                float shadowGlintQuench = saturate(max(shadowPower, realtimeShadowMask * _WaterRealtimeShadowSpecularSuppression));
                float maxBrightness = 100.0 / (1.0 + shadowGlintQuench * _SunQuenchPower);
                float currentBrightness = max(color.r, max(color.g, color.b));
                
                if (currentBrightness > maxBrightness)
                {
                    color.rgb *= (maxBrightness / currentBrightness);
                }

                // 3. Open-water sun glint art control.
                // Smoothness changes the BRDF, but HDR glints can still read too large at map scale.
                // This compresses only bright water highlights and avoids crushing foam/whitecaps.
                float glintBrightness = max(color.r, max(color.g, color.b));
                float glintThreshold = max(0.0, _WaterSunGlintThreshold);
                float glintSoftness = max(0.001, _WaterSunGlintSoftness);
                float glintMask = smoothstep(glintThreshold, glintThreshold + glintSoftness, glintBrightness);
                glintMask *= (1.0 - saturate(surfaceFoamMask * 0.85)) * (1.0 - waterLensInterior);

                // Size is controlled by a custom sun half-vector lobe. Smaller size preserves only
                // a tight reflection core and suppresses the broad PBR footprint around it.
                float3 sunHalfDir = normalize(mainLight.direction + inputData.viewDirectionWS);
                float sunHalfAlignment = saturate(dot(normalWS, sunHalfDir));
                float glintLobePower = lerp(420.0, 32.0, saturate(_WaterSunGlintSize));
                float glintCore = smoothstep(0.01, 0.22, pow(sunHalfAlignment, glintLobePower));

                float glintHighlight = max(0.0, glintBrightness - glintThreshold);
                float targetGlintBrightness = glintThreshold + glintHighlight * max(0.0, _WaterSunGlintStrength);
                targetGlintBrightness = min(targetGlintBrightness, max(glintThreshold, _WaterSunGlintCap));
                float glintRatio = targetGlintBrightness / max(glintBrightness, 0.001);
                color.rgb *= lerp(1.0, min(1.0, glintRatio), glintMask * saturate(_WaterSunGlintCompression));

                float postGlintBrightness = max(color.r, max(color.g, color.b));
                float outsideCoreMask = glintMask * (1.0 - glintCore) * saturate(_WaterSunGlintCompression);
                float outsideCoreCap = lerp(glintThreshold, max(glintThreshold, _WaterSunGlintCap), saturate(_WaterSunGlintSize));
                float outsideCoreRatio = min(postGlintBrightness, outsideCoreCap) / max(postGlintBrightness, 0.001);
                color.rgb *= lerp(1.0, outsideCoreRatio, outsideCoreMask);

                half3 staticWaterColor = GuildLensStaticWaterColor(posXZ, finalColor, waterLensEdge);
                half3 waterPrintedColor = lerp(staticWaterColor * 0.78, _GuildLensOutlineColor.rgb, _GuildLensOutlineColor.a * 0.28);
                half3 journalPrintedColor = lerp(staticWaterColor * 0.62, _GuildLensOutlineColor.rgb, _GuildLensOutlineColor.a * 0.42);
                staticWaterColor = lerp(staticWaterColor, waterPrintedColor, waterTextureMask);
                staticWaterColor = lerp(staticWaterColor, journalPrintedColor, journalTextureMask);
                color.rgb = lerp(color.rgb, staticWaterColor, waterFlattenMask);
                
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "GuildLensExclusionMask"
            Tags { "LightMode"="GuildLensExclusionMask" }

            ZWrite Off
            ZTest LEqual
            Cull Back

            Stencil
            {
                Ref 1
                Comp Always
                Pass Keep
            }

            HLSLPROGRAM
            #pragma vertex GuildLensMaskVert
            #pragma fragment GuildLensMaskFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct MaskAttributes
            {
                float4 positionOS : POSITION;
            };

            struct MaskVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            MaskVaryings GuildLensMaskVert(MaskAttributes IN)
            {
                MaskVaryings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 GuildLensMaskFrag(MaskVaryings IN) : SV_Target
            {
                return half4(1.0, 1.0, 1.0, 1.0);
            }
            ENDHLSL
        }
    }
}
