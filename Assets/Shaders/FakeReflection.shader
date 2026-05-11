Shader "Custom/FakeReflection"
{
    Properties
    {
        _MainTex ("Base Color Map", 2D) = "white" {}
        _BaseColor ("Color Tint", Color) = (1, 1, 1, 0.5)
        
        [Header(Fade and Blending)]
        _MaxDepth ("Submerged Depth Fade Out", Float) = 3.0
        _ShoreFade ("Shoreline Fade (Ignore Foam)", Float) = 0.2
        
        [Header(Fresnel and Zoom Fading)]
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 5.0
        _DistanceFadeStart ("Zoom Out Fade Start (Camera Distance)", Float) = 50.0
        _DistanceFadeEnd ("Zoom Out Fade End (Camera Distance)", Float) = 150.0
        
        [Header(Visuals and Blur)]
        _BlurAmount ("Reflection Blur (Requires Texture MipMaps)", Range(0, 8)) = 2.0
        _WarpScale ("Distortion Strength", Range(0, 0.1)) = 0.02
        _WarpSpeed ("Distortion Speed", Range(0, 5)) = 1.0
        _WarpFreq ("Distortion Frequency", Range(0, 10)) = 2.0
    }
    
    SubShader
    {
        // Draw after water (Queue 2010), but act as fully opaque!
        Tags { "RenderType"="Opaque" "Queue"="Transparent-10" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        
        // Unity handles scale inversion correctly, so use standard Cull Back
        Cull Back 
        ZWrite On
        ZTest LEqual
        Blend One Zero // NO hardware alpha transparency!

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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float depthBelowWater : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 screenPos  : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _BaseColor;
                float _MaxDepth;
                float _ShoreFade;
                float _FresnelPower;
                float _DistanceFadeStart;
                float _DistanceFadeEnd;
                float _BlurAmount;
                float _WarpScale;
                float _WarpSpeed;
                float _WarpFreq;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                // Get the true upside-down submerged world position
                float3 origWS = TransformObjectToWorld(IN.positionOS.xyz);
                
                // Calculate the fade out depth BEFORE we squash the vertex
                OUT.depthBelowWater = max(0, -origWS.y);
                
                // Add a gentle ripple distortion based on world position and time
                float timeOffset = _Time.y * _WarpSpeed;
                float rippleX = sin(origWS.z * _WarpFreq + timeOffset);
                float rippleZ = cos(origWS.x * _WarpFreq + timeOffset);
                
                origWS.x += rippleX * _WarpScale;
                origWS.z += rippleZ * _WarpScale;
                
                // Re-enable Planar Squashing! This places the reflection perfectly on the opaque water plane.
                float3 camPos = _WorldSpaceCameraPos.xyz;
                float3 rayDir = normalize(origWS - camPos);
                float t = -camPos.y / rayDir.y;
                float3 planeHit = camPos + rayDir * t;
                
                OUT.positionWS = planeHit;
                OUT.positionCS = TransformWorldToHClip(planeHit);
                
                // Micro-depth: Sorts front faces above squashed back faces perfectly!
                float microDepth = OUT.depthBelowWater * 0.00005;
                #if UNITY_REVERSED_Z
                OUT.positionCS.z += 0.0002; 
                OUT.positionCS.z -= microDepth; // Smaller Z is further away
                #else
                OUT.positionCS.z -= 0.0002;
                OUT.positionCS.z += microDepth; // Larger Z is further away
                #endif
                
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Fetch the opaque background pixels of the raw sea underneath the reflection
                float2 uvScreen = IN.screenPos.xy / IN.screenPos.w;
                half3 backgroundSea = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uvScreen).rgb;

                // Sample the blurry reflection texture
                half3 reflectionRGB = SAMPLE_TEXTURE2D_LOD(_MainTex, sampler_MainTex, IN.uv, _BlurAmount).rgb * _BaseColor.rgb;
                reflectionRGB *= 0.6; // Dark tint
                
                // Alpha Fading Math
                float shoreAlpha = smoothstep(0.0, _ShoreFade, IN.depthBelowWater);
                float depthAlpha = 1.0 - smoothstep(0.0, _MaxDepth, IN.depthBelowWater);
                
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - IN.positionWS);
                float NdotV = saturate(viewDir.y); 
                float fresnel = pow(1.0 - NdotV, _FresnelPower);
                
                float camDist = length(_WorldSpaceCameraPos.xyz - IN.positionWS);
                float distAlpha = 1.0 - smoothstep(_DistanceFadeStart, _DistanceFadeEnd, camDist);
                
                float totalAlpha = shoreAlpha * depthAlpha * fresnel * distAlpha;
                
                // MANUAL RGB LERPING! We linearly interpolate to the sea color instead of using hardware alpha blending!
                half3 finalColor = lerp(backgroundSea, reflectionRGB, totalAlpha);
                
                // Output fully opaque pixel. Hologram overlap artifacts are physically impossible now!
                return half4(finalColor, 1.0);
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
            Blend One Zero

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

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _BaseColor;
                float _MaxDepth;
                float _ShoreFade;
                float _FresnelPower;
                float _DistanceFadeStart;
                float _DistanceFadeEnd;
                float _BlurAmount;
                float _WarpScale;
                float _WarpSpeed;
                float _WarpFreq;
            CBUFFER_END

            MaskVaryings GuildLensMaskVert(MaskAttributes IN)
            {
                MaskVaryings OUT;

                float3 origWS = TransformObjectToWorld(IN.positionOS.xyz);
                float depthBelowWater = max(0, -origWS.y);

                float timeOffset = _Time.y * _WarpSpeed;
                float rippleX = sin(origWS.z * _WarpFreq + timeOffset);
                float rippleZ = cos(origWS.x * _WarpFreq + timeOffset);
                origWS.x += rippleX * _WarpScale;
                origWS.z += rippleZ * _WarpScale;

                float3 camPos = _WorldSpaceCameraPos.xyz;
                float3 rayDir = normalize(origWS - camPos);
                float t = -camPos.y / rayDir.y;
                float3 planeHit = camPos + rayDir * t;

                OUT.positionCS = TransformWorldToHClip(planeHit);

                float microDepth = depthBelowWater * 0.00005;
                #if UNITY_REVERSED_Z
                OUT.positionCS.z += 0.0002;
                OUT.positionCS.z -= microDepth;
                #else
                OUT.positionCS.z -= 0.0002;
                OUT.positionCS.z += microDepth;
                #endif

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
