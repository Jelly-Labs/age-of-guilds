Shader "AgeOfGuilds/MapEdgeMask"
{
    Properties
    {
        _EdgeColor ("Edge Color", Color) = (0.64, 0.46, 0.27, 1)
        _Opacity ("Opacity", Range(0, 1)) = 0.78
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.16
        _NoiseScale ("Noise Scale", Float) = 0.025
        [HideInInspector] _WorldCenter ("World Center XZ", Vector) = (0, 0, 0, 0)
        [HideInInspector] _WorldRadii ("World Radii XZ Feather", Vector) = (138, 138, 42, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent+100"
            "IgnoreProjector" = "True"
        }

        LOD 100
        ZWrite Off
        ZTest Always
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Map Edge Mask"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _EdgeColor;
                float _Opacity;
                float _NoiseStrength;
                float _NoiseScale;
                float4 _WorldCenter;
                float4 _WorldRadii;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
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

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 radii = max(_WorldRadii.xy, float2(0.001, 0.001));
                float smallestRadius = max(0.001, min(radii.x, radii.y));
                float feather = clamp(_WorldRadii.z / smallestRadius, 0.0001, 1.0);
                float innerEdge = saturate(1.0 - feather);

                float noise = FractalNoise(input.positionWS.xz * max(0.0001, _NoiseScale) + float2(17.1, 43.9));
                float noiseAmount = saturate(_NoiseStrength) * feather * 1.4;
                float2 ellipseDelta = (input.positionWS.xz - _WorldCenter.xy) / radii;
                float ellipseDistance = length(ellipseDelta) + (noise - 0.5) * noiseAmount;
                float edgeMask = smoothstep(innerEdge, 1.0, ellipseDistance);

                half4 color = _EdgeColor;
                color.rgb *= lerp(1.0, lerp(0.84, 1.12, noise), saturate(_NoiseStrength) * 0.35);
                color.a *= saturate(_Opacity) * edgeMask;
                return color;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
