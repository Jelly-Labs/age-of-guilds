Shader "Custom/ShipWake"
{
    Properties
    {
        _Color ("Wake Color", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 1)) = 1.0
        _ScrollSpeed("Scroll Speed", Float) = 3.0
        
        [Header(Shape Parameters)]
        _VWidthStart("Front Width Limit", Float) = 0.05
        _VWidthEnd("Back Width Spread", Float) = 1.2
        _EdgeFalloff("Edge Fade Falloff", Range(0.01, 1)) = 0.3
        _LengthFadeBack("Back Fade Cutoff", Range(0.01, 1.2)) = 0.8
        
        [Header(Foam Characteristics)]
        _FoamDensity("Foam Density", Float) = 10.0
        _OpacityBoost("Solid Core Opacity Boost", Range(0.5, 5)) = 2.0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float _Intensity;
            float _ScrollSpeed;
            float _VWidthStart;
            float _VWidthEnd;
            float _EdgeFalloff;
            float _LengthFadeBack;
            float _FoamDensity;
            float _OpacityBoost;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float hash(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453123); }
            float noise(float2 p) {
                float2 i = floor(p); float2 f = frac(p);
                float2 u = f*f*(3.0-2.0*f);
                return lerp(lerp(hash(i), hash(i + float2(1,0)), u.x),
                            lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), u.x), u.y);
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv; 
                float distFromCenter = abs(uv.x - 0.5) * 2.0;

                // V shape mask
                float allowedWidth = lerp(_VWidthEnd, _VWidthStart, uv.y);
                allowedWidth = clamp(allowedWidth, 0.0, 0.95); 
                
                float widthMask = smoothstep(allowedWidth, allowedWidth - _EdgeFalloff, distFromCenter);
                
                float lengthFade = smoothstep(0.0, _LengthFadeBack, uv.y);
                lengthFade *= smoothstep(1.0, 0.95, uv.y);

                float2 noiseUV = uv * float2(3.0, 8.0);
                noiseUV.y += _Time.y * _ScrollSpeed;
                float foamNoise = noise(noiseUV * _FoamDensity) * 0.5 + noise(noiseUV * (_FoamDensity * 2.0)) * 0.5;

                float foamThreshold = lerp(0.7, 0.2, uv.y);
                
                // Tightened upper threshold to force pure 1.0 opaque values in the core of the bubbles
                float foamPattern = smoothstep(foamThreshold - 0.05, foamThreshold + 0.05, foamNoise);

                float finalAlpha = widthMask * lengthFade * foamPattern * _Intensity * _OpacityBoost;
                finalAlpha = saturate(finalAlpha); // cap it safely at 1.0 max
                
                return half4(_Color.rgb, finalAlpha * _Color.a);
            }
            ENDHLSL
        }
    }
}
