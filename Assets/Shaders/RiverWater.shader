Shader "Custom/URPRiver"
{
    Properties
    {
        _RiverColor("River Deep Color", Color) = (0.05, 0.45, 0.65, 0.95)
        _ShoreColor("River Shallow Color (Edge)", Color) = (0.3, 0.7, 0.8, 0.4)
        
        [Header(Shoreline Depth Blend)]
        _DepthSoftness ("Shore Interface Softness (Meters)", Range(0.01, 5.0)) = 1.0
        
        [Header(Flow Settings)]
        _NormalMap("Flowing Normal Map", 2D) = "bump" {}
        _NormalStrength("Normal Strength", Range(0, 2)) = 1.0
        _FlowSpeed("Downstream Flow Speed", Float) = 2.0
        _CrossFlowSpeed("Cross Flow Variance", Float) = 0.2
        _Tiling("Texture Tiling (U=Across, V=Length)", Vector) = (2, 2, 0, 0)
        
        [Header(Visuals)]
        _Smoothness("Smoothness", Range(0, 1)) = 0.95
        
        [Header(Atmosphere Height Fog)]
        _AtmosFogColor("Fog Haze Color", Color) = (0.62, 0.74, 0.85, 1)
        _AtmosFogStart("Fog Start Distance", Float) = 30.0
        _AtmosFogEnd("Fog Full Distance", Float) = 120.0
        _AtmosFogStrength("Fog Strength", Range(0.0, 1.0)) = 0.6
        
        [Header(Dynamic Cloud Shadows)]
        _WaterShadowColor ("Shadow Darkening Color", Color) = (0.65, 0.65, 0.75, 1.0)
        _WaterShadowIntensity ("Shadow Overall Opacity Multiplier", Range(0, 2)) = 1.0
        _ShadowSmoothness ("Shadow Smoothness", Range(0, 1)) = 0.85
        _SunQuenchPower ("Sun Specular Quench Power", Range(0.1, 1000)) = 500.0
    }
    
    SubShader
    {
        // Transparent so we can read the Depth Buffer for soft shores!
        Tags { "RenderType"="Transparent" "Queue"="Transparent-10" "RenderPipeline"="UniversalPipeline" }
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Assets/Shaders/Include/GlobalCloudShadows.hlsl"

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

            TEXTURE2D(_NormalMap); SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _RiverColor;
                half4 _ShoreColor;
                float _DepthSoftness;
                
                float _NormalStrength;
                float _FlowSpeed;
                float _CrossFlowSpeed;
                float4 _Tiling;
                float _Smoothness;

                // Atmosphere fog
                half4  _AtmosFogColor;
                float  _AtmosFogStart;
                float  _AtmosFogEnd;
                float  _AtmosFogStrength;
                
                // Cloud Shadows
                half4 _WaterShadowColor;
                float _WaterShadowIntensity;
                float _ShadowSmoothness;
                float _SunQuenchPower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                
                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS    = normalInput.normalWS;
                OUT.tangentWS   = normalInput.tangentWS;
                OUT.bitangentWS = normalInput.bitangentWS;
                
                OUT.uv = IN.uv;
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // === RIVER FLOW MAPPING ===
                // U = Width across river (0 to 1)
                // V = Absolute physical distance downstream
                float2 localUV = IN.uv * _Tiling.xy;
                
                // Main Flow (Rapid, Downstream)
                float2 uv1 = localUV + float2(0.0, -_Time.y * _FlowSpeed);
                // Secondary Flow (Slightly slower, crossing diagonally for organic wave interference)
                float2 uv2 = localUV * 1.35 + float2(_Time.y * _CrossFlowSpeed, -_Time.y * _FlowSpeed * 0.82);

                half3 n1 = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv1), _NormalStrength);
                half3 n2 = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv2), _NormalStrength * 0.8);
                
                // Final Normal Blend
                half3 tangentNormal = normalize(half3(n1.xy + n2.xy, n1.z * n2.z));
                half3 normalWS = TransformTangentToWorld(tangentNormal, half3x3(IN.tangentWS, IN.bitangentWS, IN.normalWS));
                normalWS = normalize(normalWS);

                // === EDGE BLEND / SOFT SHORES ===
                // Eliminates sharp polygon intersection lines where river meets the riverbed!
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneDepthMeters = LinearEyeDepth(rawDepth, _ZBufferParams);
                float riverDepthMeters = IN.screenPos.w;
                
                // physical thickness of the water here in meters!
                float waterDepth = max(0.0, sceneDepthMeters - riverDepthMeters);
                
                // Mix color based on water depth
                float depthRatio = saturate(waterDepth / max(0.01, _DepthSoftness));
                half4 finalColor = lerp(_ShoreColor, _RiverColor, depthRatio);
                
                // If the water is absolutely physically 0 depth (exactly at the shore), gracefully fade to transparent!
                finalColor.a *= depthRatio;

                // === ATMOSPHERE FOG ===
                float camDist  = length(IN.positionWS - _WorldSpaceCameraPos.xyz);
                float fogRange = max(_AtmosFogEnd - _AtmosFogStart, 0.001);
                float fogT     = saturate((camDist - _AtmosFogStart) / fogRange);
                finalColor.rgb = lerp(finalColor.rgb, _AtmosFogColor.rgb, fogT * _AtmosFogStrength);
                
                // === CLOUD SHADOWS ===
                float cloudShadowRaw = GetGlobalCloudShadowRawMask(IN.positionWS);
                float shadowPower = saturate(cloudShadowRaw * _GlobalCloudShadowTint.a * _WaterShadowIntensity);

                // === PBR LIGHTING ===
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = finalColor.rgb;
                surfaceData.metallic = 0.0;
                surfaceData.smoothness = lerp(_Smoothness, _ShadowSmoothness, shadowPower);
                surfaceData.alpha = finalColor.a;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);
                
                // Shadow / HDR Quenching
                half3 shadowedColor = color.rgb * _WaterShadowColor.rgb;
                color.rgb = lerp(color.rgb, shadowedColor, shadowPower);
                
                float maxBrightness = 100.0 / (1.0 + shadowPower * _SunQuenchPower);
                float currentBrightness = max(color.r, max(color.g, color.b));
                
                if (currentBrightness > maxBrightness)
                {
                    color.rgb *= (maxBrightness / currentBrightness);
                }
                
                return half4(color.rgb, surfaceData.alpha);
            }
            ENDHLSL
        }
    }
}
