Shader "Skybox/AgeOfGuilds/PanoramicEXR"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Panoramic Sky (HDR)", 2D) = "gray" {}
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Exposure ("Exposure", Range(0, 8)) = 1
        _Rotation ("Rotation", Range(0, 360)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Background"
            "RenderType" = "Background"
            "PreviewType" = "Skybox"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                half _Exposure;
                float _Rotation;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 directionWS : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS);
                output.directionWS = TransformObjectToWorldDir(input.positionOS);
                return output;
            }

            float2 DirectionToPanoramicUV(float3 directionWS)
            {
                directionWS = normalize(directionWS);
                float rotationRadians = _Rotation * 0.01745329252;
                float longitude = atan2(directionWS.x, directionWS.z) + rotationRadians;
                float latitude = asin(clamp(directionWS.y, -1.0, 1.0));
                return float2(frac(0.5 + longitude * 0.15915494309), 0.5 - latitude * 0.31830988618);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = DirectionToPanoramicUV(input.directionWS);
                half3 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).rgb;
                color *= _Tint.rgb * _Exposure;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
