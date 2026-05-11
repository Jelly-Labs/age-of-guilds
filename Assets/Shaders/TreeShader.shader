Shader "Custom/TreeShader"
{
    Properties
    {
        [Header(Base Color)]
        _BaseColor      ("Base Canopy Color",         Color)      = (0.13, 0.35, 0.12, 1)

        [Header(Procedural Color Variety)]
        _ColorVariantA  ("Color Variant A (Lush)",    Color)      = (0.10, 0.40, 0.10, 1)
        _ColorVariantB  ("Color Variant B (Mid)",     Color)      = (0.25, 0.42, 0.08, 1)
        _ColorVariantC  ("Color Variant C (Dry)",     Color)      = (0.38, 0.34, 0.05, 1)
        _ColorNoiseScale("Color Noise Scale",         Float)      = 0.04
        _ColorContrast  ("Color Contrast",            Range(0,1)) = 0.7

        [Header(Normal Map)]
        [Normal] _NormalMap  ("Normal Map",           2D)         = "bump" {}
        _NormalScale         ("Normal Scale",         Range(0,2)) = 1.0

        [Header(Surface)]
        _Smoothness     ("Smoothness",                Range(0,1)) = 0.45
        _SpecularPower  ("Specular Power (Gloss)",    Range(1,256))= 48.0
        _AmbientBoost   ("Ambient Boost",             Range(0,1)) = 0.3
        _CanopyLightWrap("Canopy Light Wrap",          Range(0,0.75)) = 0.18

        [Header(World Blend)]
        _TerrainBlendColor   ("Terrain Blend Color",   Color)      = (0.24, 0.32, 0.18, 1)
        _TerrainBlendStrength("Terrain Blend Strength",Range(0,1)) = 0.12

        [Header(Atmosphere Fog)]
        _AtmosFogColor   ("Fog Color",                Color)      = (0.62, 0.74, 0.85, 1)
        _AtmosFogStart   ("Fog Start Distance",       Float)      = 30.0
        _AtmosFogEnd     ("Fog Full Distance",        Float)      = 120.0
        _AtmosFogStrength("Fog Strength",             Range(0,1)) = 0.6

        [Header(Dynamic Cloud Shadows)]
        _TreeShadowColor    ("Cloud Shadow Tint",      Color)      = (0.35, 0.43, 0.36, 1)
        _TreeShadowIntensity("Cloud Shadow Intensity", Range(0,2)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+5" "RenderPipeline"="UniversalPipeline" }

        // ── FORWARD LIT PASS ─────────────────────────────────────────────────
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   4.5

            #pragma multi_compile_instancing
            #pragma instancing_options procedural:SetupInstancing

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Shaders/Include/GlobalCloudShadows.hlsl"

            // ── Per-instance data ─────────────────────────────────────────────
            struct DrawInstance { float4x4 objectToWorld; };

            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            StructuredBuffer<DrawInstance> _VisibleInstances;
            #endif

            void SetupInstancing()
            {
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                unity_ObjectToWorld = _VisibleInstances[unity_InstanceID].objectToWorld;
                unity_WorldToObject = transpose(unity_ObjectToWorld);
                unity_WorldToObject[3]    = float4(0, 0, 0, 1);
                unity_WorldToObject[0][3] = 0;
                unity_WorldToObject[1][3] = 0;
                unity_WorldToObject[2][3] = 0;
            #endif
            }

            // ── Material uniforms ─────────────────────────────────────────────
            float4 _BaseColor;
            float4 _ColorVariantA;
            float4 _ColorVariantB;
            float4 _ColorVariantC;
            float  _ColorNoiseScale;
            float  _ColorContrast;

            TEXTURE2D(_NormalMap);  SAMPLER(sampler_NormalMap);
            float4 _NormalMap_ST;
            float  _NormalScale;

            float  _Smoothness;
            float  _SpecularPower;
            float  _AmbientBoost;
            float  _CanopyLightWrap;
            float4 _TerrainBlendColor;
            float  _TerrainBlendStrength;

            float4 _AtmosFogColor;
            float  _AtmosFogStart;
            float  _AtmosFogEnd;
            float  _AtmosFogStrength;
            float4 _TreeShadowColor;
            float  _TreeShadowIntensity;

            // ── Noise helpers ─────────────────────────────────────────────────
            float hashT(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float vnoiseT(float2 p)
            {
                float2 i = floor(p); float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hashT(i), hashT(i + float2(1,0)), u.x),
                            lerp(hashT(i + float2(0,1)), hashT(i + float2(1,1)), u.x), u.y);
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float2 uv          : TEXCOORD1;
                // TBN rows packed as separate texcoords
                float3 tangentWS   : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float3 normalWS    : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varyings OUT;
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float3x3 objectToWorld3 = (float3x3)unity_ObjectToWorld;

                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS  = TransformWorldToHClip(OUT.positionWS);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _NormalMap);

                // Build world-space TBN for normal mapping
                float3 normalWS   = normalize(mul(objectToWorld3, IN.normalOS));
                float3 tangentWS  = normalize(mul(objectToWorld3, IN.tangentOS.xyz));
                // Reconstruct bitangent respecting handedness sign stored in tangentOS.w
                float3 bitangentWS = cross(normalWS, tangentWS) * IN.tangentOS.w
                                   * unity_WorldTransformParams.w;

                OUT.normalWS    = normalWS;
                OUT.tangentWS   = tangentWS;
                OUT.bitangentWS = bitangentWS;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);

                // ── Normal mapping ─────────────────────────────────────────────
                // Sample the normal map, unpack and scale, then rotate into world space
                half4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv);
                half3 tangentNormal = UnpackNormalScale(normalSample, _NormalScale);

                float3x3 tbn      = float3x3(
                    normalize(IN.tangentWS),
                    normalize(IN.bitangentWS),
                    normalize(IN.normalWS));
                float3 normalWS   = normalize(mul(tangentNormal, tbn));

                float3 viewDir    = normalize(_WorldSpaceCameraPos.xyz - IN.positionWS);

                // ── Procedural colour ──────────────────────────────────────────
                float2 xz = IN.positionWS.xz;
                float n1  = vnoiseT(xz * _ColorNoiseScale);
                float n2  = vnoiseT(xz * _ColorNoiseScale * 2.3 + float2(4.1, 9.7));
                float t1  = saturate((n1 - 0.5) * _ColorContrast * 2.0 + 0.5);
                float t2  = saturate((n2 - 0.5) * _ColorContrast * 2.0 + 0.5);
                half3 cAB = lerp(_ColorVariantA.rgb, _ColorVariantB.rgb, t1);
                half3 albedo = lerp(cAB, _ColorVariantC.rgb, t2 * 0.5);
                float terrainBlendNoise = saturate(0.72 + (n1 - 0.5) * 0.35);
                albedo = lerp(albedo, _TerrainBlendColor.rgb, saturate(_TerrainBlendStrength) * terrainBlendNoise);

                // ── Lighting ───────────────────────────────────────────────────
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light  mainLight   = GetMainLight(shadowCoord);

                float lightWrap = saturate(_CanopyLightWrap);
                float NdotL = saturate((dot(normalWS, mainLight.direction) + lightWrap) / (1.0 + lightWrap));
                half3 diffuse = albedo * mainLight.color * NdotL
                              * mainLight.distanceAttenuation * mainLight.shadowAttenuation;

                // Blinn-Phong specular using the normal-mapped normal
                float3 halfDir = normalize(mainLight.direction + viewDir);
                float  NdotH   = saturate(dot(normalWS, halfDir));
                float  specGloss = exp2(_SpecularPower * _Smoothness + 1.0);
                float  spec    = pow(NdotH, specGloss) * _Smoothness;
                half3  specular = mainLight.color * spec * mainLight.shadowAttenuation;

                // Ambient
                half3 ambient = SampleSH(normalWS) * albedo * (1.0 + _AmbientBoost);

                half3 finalColor = diffuse + specular + ambient;

                // ── Atmosphere fog ─────────────────────────────────────────────
                float camDist  = length(IN.positionWS - _WorldSpaceCameraPos.xyz);
                float fogT     = saturate((camDist - _AtmosFogStart) / max(_AtmosFogEnd - _AtmosFogStart, 0.001));
                finalColor     = lerp(finalColor, _AtmosFogColor.rgb, fogT * _AtmosFogStrength);

                // Match the moving world cloud shadows used by terrain and water.
                float cloudShadowRaw = GetGlobalCloudShadowRawMask(IN.positionWS);
                float cloudShadow = saturate(cloudShadowRaw * _GlobalCloudShadowTint.a * _TreeShadowIntensity);
                half3 cloudShadowedColor = finalColor * _TreeShadowColor.rgb;
                finalColor = lerp(finalColor, cloudShadowedColor, cloudShadow);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // ── SHADOW CASTER PASS ───────────────────────────────────────────────
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex   ShadowVert
            #pragma fragment ShadowFrag
            #pragma target   4.5

            #pragma multi_compile_instancing
            #pragma instancing_options procedural:SetupInstancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // ── Per-instance data (self-contained — no HLSLINCLUDE) ───────────
            struct DrawInstance { float4x4 objectToWorld; };

            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            StructuredBuffer<DrawInstance> _VisibleInstances;
            #endif

            void SetupInstancing()
            {
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
                unity_ObjectToWorld = _VisibleInstances[unity_InstanceID].objectToWorld;
                unity_WorldToObject = transpose(unity_ObjectToWorld);
                unity_WorldToObject[3]    = float4(0, 0, 0, 1);
                unity_WorldToObject[0][3] = 0;
                unity_WorldToObject[1][3] = 0;
                unity_WorldToObject[2][3] = 0;
            #endif
            }

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            ShadowVaryings ShadowVert(ShadowAttributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                ShadowVaryings OUT;
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

                float3 posWS   = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(posWS);

                #if UNITY_REVERSED_Z
                    OUT.positionCS.z = min(OUT.positionCS.z, OUT.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    OUT.positionCS.z = max(OUT.positionCS.z, OUT.positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                return OUT;
            }

            half4 ShadowFrag(ShadowVaryings IN) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
