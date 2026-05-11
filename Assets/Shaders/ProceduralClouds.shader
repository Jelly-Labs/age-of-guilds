Shader "Custom/ProceduralClouds"
{
    Properties
    {
        [Header(Cloud Base)]
        _BaseColor ("Sunlit Cloud Color", Color) = (1, 1, 1, 0.9)
        _ShadowColor ("Cloud Self-Shadow Tint", Color) = (0.85, 0.88, 0.95, 0.9)
        _TerrainShadowTint ("Terrain Shadow Tint (Multiplier)", Color) = (0.5, 0.55, 0.65, 1)
        
        [Header(Shape)]
        _Scale ("Cloud Noise Scale", Range(0.0001, 1.0)) = 0.02
        _Coverage ("Cloud Coverage Density", Range(0, 1)) = 0.55
        _Softness ("Cloud Edge Softness", Range(0.001, 0.5)) = 0.05
        _Deform ("Organic Morphing/Boiling", Range(0, 2)) = 0.3
        
        [Header(Island Proximity Masking)]
        _CoastalMap ("Coastal Extent Map (R/B=Distance)", 2D) = "black" {}
        _HighlandFade ("Enable Coastal Mask (1=On)", Range(0, 1)) = 0.0
        _HighlandElevationStart ("Coast Spread", Float) = 0.2
        _HighlandElevationFull ("Inland Solid", Float) = 0.8
        _IntersectionFade ("Mountain Intersection Softness", Range(0.01, 10)) = 1.0
        
        [Header(Wind Animation)]
        _WindSpeed ("Wind Velocity (X, Z)", Vector) = (0.3, 0.1, 0, 0)
        
        [Header(Vertical Undulation)]
        _MistBobSpeed ("Undulation Speed", Float) = 0.5
        _MistBobFreq ("Undulation Frequency", Float) = 0.05
        _MistBobAmount ("Undulation Height (Meters)", Float) = 0.0
        
        [Header(Volumetric Lighting)]
        _SunOffset ("Sun Bevel Distance", Range(0, 0.5)) = 0.05
        _SunDir ("Sun Direction (X, Z)", Vector) = (-1.0, 1.0, 0, 0)
        
        [Header(Camera Proximity)]
        _CameraFadeStart ("Camera Dissolve Distance", Float) = 10.0
        _CameraFadeLength ("Camera Dissolve Softness", Float) = 15.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off // Draw from above or below!

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            // Required for reading the distance to the mountains!
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            // Bring in our optimized math library!
            #include "Assets/Shaders/Include/CloudNoise.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float fogFactor   : TEXCOORD1;
                float4 screenPos  : TEXCOORD2;
                float2 uv         : TEXCOORD3;
            };

            TEXTURE2D(_CoastalMap); SAMPLER(sampler_CoastalMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                float _Scale;
                float _Coverage;
                float _Softness;
                float _Deform;
                float _HighlandFade;
                float _HighlandElevationStart;
                float _HighlandElevationFull;
                float4 _CoastalMap_ST;
                float _IntersectionFade;
                float4 _WindSpeed;
                float _MistBobSpeed;
                float _MistBobFreq;
                float _MistBobAmount;
                float _SunOffset;
                float4 _SunDir;
                half4 _TerrainShadowTint;
                float _CameraFadeStart;
                float _CameraFadeLength;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                
                // Add procedural rolling vertical displacement
                // Dual sine waves on X and Z axes create an organic, rolling diagonal wave pattern
                float bob = sin(_Time.y * _MistBobSpeed + OUT.positionWS.x * _MistBobFreq + OUT.positionWS.z * _MistBobFreq) 
                          + cos(_Time.y * (_MistBobSpeed * 1.3) + OUT.positionWS.z * _MistBobFreq - OUT.positionWS.x * _MistBobFreq);
                OUT.positionWS.y += (bob * 0.5) * _MistBobAmount;

                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                
                // URP Fog handling to fade clouds out against the horizon!
                OUT.fogFactor = ComputeFogFactor(OUT.positionCS.z);
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Core noise mask
                float mask = GetCloudMask(IN.positionWS.xz, _Scale, _WindSpeed.xy, _Time.y, _Coverage, _Softness, _Deform);
                
                // Performance safeguard: drop the fragment instantly if it's completely invisible sky
                clip(mask - 0.001);
                
                // --- COASTAL OCEAN MASKING ---
                if (_HighlandFade > 0.5)
                {
                    // Map using the Mesh's actual UVs so it perfectly automatically aligns with the waterplane mesh!
                    float2 mapUV = IN.uv * _CoastalMap_ST.xy + _CoastalMap_ST.zw;
                    float distToIsland = SAMPLE_TEXTURE2D(_CoastalMap, sampler_CoastalMap, mapUV).b;
                    
                    // Inverting the math to fix the mist appearing on the Sea!
                    float islandProximity = saturate(distToIsland);
                    float elevationMask = smoothstep(_HighlandElevationStart, _HighlandElevationFull, islandProximity);
                    mask *= elevationMask;
                }
                
                // --- VOLUMETRIC SOFT INTERSECTION ---
                // Eliminates the sharp geometric pane-of-glass cut through the mountains!
                {
                    float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                    float rawDepth = SampleSceneDepth(screenUV);
                    float sceneDepthMeters = LinearEyeDepth(rawDepth, _ZBufferParams);
                    float cloudDepthMeters = IN.screenPos.w;
                    float intersectionMask = saturate((sceneDepthMeters - cloudDepthMeters) / _IntersectionFade);
                    mask *= intersectionMask;
                }
                
                // --- CAMERA PROXIMITY FADE ---
                // Guarantees the camera NEVER clips through a 2D plane by softly dissolving it!
                float camDist = length(IN.positionWS - _WorldSpaceCameraPos.xyz);
                float camFade = saturate((camDist - _CameraFadeStart) / max(0.001, _CameraFadeLength));
                mask *= camFade;
                
                clip(mask - 0.001);
                
                // Bevel/Volumetric shading logic:
                // We fake thickness by sampling the cloud mask slightly offset towards the light!
                float2 bevelOffset = normalize(_SunDir.xy) * _SunOffset;
                float sunMask = GetCloudMask(IN.positionWS.xz + bevelOffset, _Scale, _WindSpeed.xy, _Time.y, _Coverage, _Softness, _Deform);
                
                // If the pixel towards the sun is ALSO a cloud, it blocks the light (shadow).
                // If the pixel towards the sun is EMPTY SKY, the light hits our current pixel (bright edge)!
                // We use smoothstep to create a soft volumetric transition.
                float lightThickness = saturate(mask - sunMask);
                
                // Lerp between base color (white) and shadow color (grey/blue)
                half3 cloudColor = lerp(_ShadowColor.rgb, _BaseColor.rgb, lightThickness * 2.0 + 0.5);
                
                half4 finalColor = half4(cloudColor, mask * _BaseColor.a);
                
                // Mix in horizon fog
                finalColor.rgb = MixFog(finalColor.rgb, IN.fogFactor);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}
