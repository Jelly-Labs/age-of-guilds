// IslandShader v2 - coast sand fix applied
Shader "Custom/URPLitIsland"
{
    Properties
    {
        [Header(Base Texture)]
        _BaseMap("Base Color Texture", 2D) = "white" {}
        _TextureBlend("Texture x Zone Strength", Range(0.0, 1.0)) = 0.8
        [Space]
        // 0=Multiply  1=Overlay  2=Screen  3=SoftLight
        _BlendMode("Blend Mode  (0=Multiply  1=Overlay  2=Screen  3=SoftLight)", Range(0.0, 3.0)) = 1.0

        [Header(Zone Variation Randomizer)]
        _VariationScale("Variation Noise Scale", Float) = 0.8
        _VariationStrength("Variation Strength", Range(0.0, 1.0)) = 0.2
        _VariationColor("Variation Color", Color) = (0.55, 0.45, 0.25, 1)

        [Header(Zone Colors)]
        _SandColor("Sand Color", Color) = (0.78, 0.69, 0.48, 1)
        _GrassColor("Grass Color", Color) = (0.22, 0.48, 0.14, 1)
        _MidColor("Mid Color (Alpine Meadow)", Color) = (0.38, 0.34, 0.20, 1)
        _RockColor("Rock Color (Bare Stone)", Color) = (0.48, 0.45, 0.40, 1)
        _SnowColor("Snow Color", Color) = (0.92, 0.96, 1.0, 1)

        [Header(Beach Zone)]
        _BeachHeightMax("Beach Max Height", Float) = 0.8
        _BeachBlend("Beach Blend Softness", Float) = 0.3
        _BeachSlopeMax("Beach Slope Cutoff (cliffs)", Range(0.0, 1.0)) = 0.65
        _BeachSlopeMin("Beach Slope Fade Start", Range(0.0, 1.0)) = 0.35
        _BeachNoiseScale("Beach Edge Noise Scale", Float) = 0.4
        _BeachNoiseStrength("Beach Edge Noise Strength", Range(0.0, 1.0)) = 0.15
        _LegacyBeachStrength("Legacy Beach Strength", Range(0.0, 1.0)) = 0.0

        [Header(SDF Shoreline Beach Override)]
        _ShorelineDistanceMap("Shoreline Distance Map (A = Land Edge Distance)", 2D) = "white" {}
        _ShorelineMapOrigin("Shoreline Map Origin XZ", Vector) = (-150, -150, 0, 0)
        _ShorelineMapSize("Shoreline Map Size XZ", Vector) = (300, 300, 0, 0)
        _ShorelineMapFlipX("Shoreline Map Flip X", Range(0.0, 1.0)) = 0.0
        _ShorelineMapFlipY("Shoreline Map Flip Y", Range(0.0, 1.0)) = 1.0
        _ShorelineMapMaxDistance("SDF Bake Max Distance (Calibration)", Float) = 100.0
        _ShorelineBeachWidth("Forced Beach Width", Float) = 2.5
        _ShorelineBeachSoftness("Forced Beach Softness", Float) = 0.75
        _ShorelineBeachStrength("Forced Beach Strength", Range(0.0, 1.0)) = 1.0
        _ShorelineOverlayColor("Shoreline Overlay Color", Color) = (0.78, 0.69, 0.48, 1)
        _ShorelineOverlayStrength("Shoreline Overlay Strength", Range(0.0, 1.0)) = 1.0
        _ShorelineTextureOverride("Shoreline Texture Override", Range(0.0, 1.0)) = 1.0
        _ShorelineNormalOverride("Shoreline Normal Override", Range(0.0, 1.0)) = 1.0
        _ShorelineBreakupScale("Forced Beach Breakup Scale", Float) = 0.2
        _ShorelineBreakupStrength("Forced Beach Breakup Width", Float) = 0.15
        _ShorelineCliffOverride("Forced Beach Cliff Override (0=Preserve)", Range(0.0, 1.0)) = 0.0
        _ShorelineCliffExclusionSoftness("Forced Beach Cliff Exclusion Softness", Range(0.001, 0.5)) = 0.08
        _ShorelineSlopeFadeStart("Forced Beach Slope Fade Start", Range(0.0, 1.0)) = 0.20
        _ShorelineSlopeFadeEnd("Forced Beach Slope Fade End", Range(0.0, 1.0)) = 0.70
        _ShorelineSteepStrength("Forced Beach Strength On Steep Coasts", Range(0.0, 1.0)) = 1.0
        _ShorelineDebugMode("Shoreline Debug Mode (0 Off, 1 Raw SDF, 2 Distance, 3 Forced, 4 Overlay, 5 Legacy, 6 SDF/Land/Mask)", Range(0.0, 6.0)) = 0.0

        [Header(Snow Zone)]
        _SnowHeightMin("Snow Start Height", Float) = 4.0
        _SnowHeightMax("Snow Full Height", Float) = 6.0
        _SnowBlend("Snow Blend Softness", Float) = 0.5
        _SnowSlopeMax("Snow Slope Cutoff (cliffs)", Range(0.0, 1.0)) = 0.6
        _SnowSlopeMin("Snow Slope Fade Start", Range(0.0, 1.0)) = 0.3

        [Header(Grass Transition)]
        _GrassHeightMin("Grass Zone Start", Float) = 0.8
        _GrassHeightMax("Grass Zone Full", Float) = 2.0

        [Header(Mid Zone Alpine Meadow)]
        _MidHeightMin("Mid Zone Start", Float) = 2.5
        _MidHeightMax("Mid Zone Full", Float) = 3.5

        [Header(Rock Zone Bare Stone)]
        _RockHeightMin("Rock Zone Start", Float) = 4.0
        _RockHeightMax("Rock Zone Full", Float) = 5.0

        [Header(Lighting)]
        _Metallic("Metallic", Range(0, 1)) = 0.0

        [Header(Per Zone Smoothness)]
        _SmoothnessBeach("Beach  (wet sand)", Range(0, 1)) = 0.35
        _SmoothnessGrass("Grass", Range(0, 1)) = 0.08
        _SmoothnessMid("Mid  Alpine", Range(0, 1)) = 0.06
        _SmoothnessRock("Rock", Range(0, 1)) = 0.04
        _SmoothnessSnow("Snow", Range(0, 1)) = 0.20
        _SmoothnessCliff("Cliff Face", Range(0, 1)) = 0.03

        [Header(Cliff Texture Override)]
        // Fades the UV-mapped base texture on steep cliff faces,
        // letting the procedural cliff color show without stretch artifacts.
        _CliffTextureFade("Cliff Texture Fade", Range(0.0, 1.0)) = 0.9

        [Header(Atmosphere Height Fog)]
        _AtmosFogColor("Fog Haze Color", Color) = (0.62, 0.74, 0.85, 1)
        _AtmosFogStart("Fog Start Distance", Float) = 30.0
        _AtmosFogEnd("Fog Full Distance", Float) = 120.0
        _AtmosFogStrength("Fog Strength", Range(0.0, 1.0)) = 0.6

        [Header(Dynamic Cloud Shadows)]
        _IslandShadowColor ("Shadow Darkening Color", Color) = (0.35, 0.40, 0.45, 1.0)
        _IslandShadowIntensity ("Shadow Overall Opacity Multiplier", Range(0, 2)) = 1.0
        _SunQuenchPower ("Sun Specular Quench Power", Range(0.1, 1000)) = 200.0
        _ShadowLift ("Shadow Lift (prevents black shadows)", Range(0, 1)) = 0.3

        [Header(Cliff Darkening)]
        _CliffColor("Cliff Color", Color) = (0.28, 0.25, 0.22, 1)
        _CliffSlopeStart("Cliff Slope Start", Range(0.0, 1.0)) = 0.55
        _CliffSlopeEnd("Cliff Slope Full", Range(0.0, 1.0)) = 0.75
        _CliffStrength("Cliff Strength", Range(0.0, 1.0)) = 0.85

        [Header(Forest Canopy)]
        _ForestColor("Forest Color", Color) = (0.10, 0.28, 0.08, 1)
        _ForestScale("Forest Blob Scale", Float) = 0.18
        _ForestThreshold("Forest Coverage Threshold", Range(0.0, 1.0)) = 0.55
        _ForestSoftness("Forest Edge Softness", Range(0.01, 0.5)) = 0.15
        _ForestStrength("Forest Strength", Range(0.0, 1.0)) = 0.75
        _ForestHeightMin("Forest Min Height", Float) = 0.8
        _ForestHeightMax("Forest Max Height", Float) = 3.5

        [Header(Farm Fields)]
        _FarmColor1("Crop Color A (Rapeseed/Gold)", Color) = (0.78, 0.70, 0.15, 1)
        _FarmColor2("Crop Color B (Wheat/Light Green)", Color) = (0.50, 0.65, 0.20, 1)
        _FarmColor3("Crop Color C (Vegetables/Dark Green)", Color) = (0.18, 0.40, 0.12, 1)
        _FarmBorderColor("Hedgerow / Border Color", Color) = (0.15, 0.18, 0.10, 1)
        _FarmScale("Parcel Scale (bigger = larger fields)", Float) = 0.25
        _FarmBorderWidth("Border Width (hedgerows)", Range(0.01, 0.4)) = 0.12
        _FarmStripeFreq("Internal Row Frequency", Float) = 4.0
        _FarmStripeContrast("Internal Row Contrast", Range(0.5, 20.0)) = 5.0
        _FarmSlopeMax("Max Slope (flat only)", Range(0.0, 0.5)) = 0.18
        _FarmHeightMin("Farm Min Height", Float) = 0.5
        _FarmHeightMax("Farm Max Height", Float) = 2.0
        _FarmStrength("Farm Strength", Range(0.0, 1.0)) = 0.7

        [Header(Paths And Roads)]
        _RoadStrength("Road Strength", Range(0.0, 1.0)) = 0.0
        _RoadColor("Road Color", Color) = (0.55, 0.43, 0.28, 1)
        _RoadWidth("Road Width", Float) = 0.65
        _RoadSoftness("Road Edge Softness", Float) = 0.35
        _RoadSeed("Road Seed", Float) = 11.0
        _RoadDensity("Road Density", Range(0.0, 1.0)) = 0.7
        _RoadBranchCount("Road Branch Count", Range(0.0, 8.0)) = 5.0
        _RoadRadius("Road Branch Radius", Float) = 18.0
        _RoadMeanderScale("Road Meander Scale", Float) = 0.25
        _RoadMeanderStrength("Road Meander Strength", Float) = 1.4
        _RoadHeightMin("Road Min Height", Float) = -10.0
        _RoadHeightMax("Road Max Height", Float) = 200.0
        _RoadSlopeMax("Road Max Slope", Range(0.0, 1.0)) = 0.2
        _RoadSmoothness("Road Smoothness", Range(0.0, 1.0)) = 0.08
        _RoadNormalDamp("Road Normal Damp", Range(0.0, 1.0)) = 0.85
        _RoadTextureOverride("Road Texture Override", Range(0.0, 1.0)) = 0.4
        _RoadMaxDarken("Road Max Darken", Range(0.0, 1.0)) = 0.18
        _RoadMaskCutoff("Road Mask Cutoff", Range(0.0, 0.95)) = 0.25
        _RoadMaskContrast("Road Mask Contrast", Range(0.25, 8.0)) = 3.0
        _RoadDebugMode("Road Debug Mode", Range(0.0, 1.0)) = 0.0
        [HideInInspector] _RoadAnchorCount("Road Anchor Count", Float) = 0.0
        [HideInInspector] _RoadAnchor0("Road Anchor 0", Vector) = (0, 0, 0, 0)
        [HideInInspector] _RoadAnchor1("Road Anchor 1", Vector) = (0, 0, 0, 0)
        [HideInInspector] _RoadAnchor2("Road Anchor 2", Vector) = (0, 0, 0, 0)
        [HideInInspector] _RoadAnchor3("Road Anchor 3", Vector) = (0, 0, 0, 0)
        [HideInInspector] _RoadAnchor4("Road Anchor 4", Vector) = (0, 0, 0, 0)
        [HideInInspector] _RoadAnchor5("Road Anchor 5", Vector) = (0, 0, 0, 0)
        [HideInInspector] _RoadAnchor6("Road Anchor 6", Vector) = (0, 0, 0, 0)
        [HideInInspector] _RoadAnchor7("Road Anchor 7", Vector) = (0, 0, 0, 0)

        [Header(Wetness Near Shore)]
        _WetColor("Wet Shore Color", Color) = (0.14, 0.30, 0.12, 1)
        _WetDepth("Wetness Depth from Shore", Range(0.0, 1.0)) = 0.45
        _WetStrength("Wetness Strength", Range(0.0, 1.0)) = 0.5

        [Header(Detail Normal Map)]
        _DetailNormalMap("Detail Normal Map", 2D) = "bump" {}
        _DetailNormalScale("Large Scale", Float) = 0.25
        _DetailNormalStrength("Large Scale Strength", Range(0, 3)) = 0.8
        _DetailNormalFineScale("Fine Scale Multiplier", Float) = 4.0
        _DetailNormalFineStrength("Fine Scale Strength", Range(0, 3)) = 0.35
        _CliffNormalBoost("Cliff Boost", Range(0, 4)) = 2.2
        _CoastNormalBoost("Coast Normal Boost", Range(0, 4)) = 1.4
        _SnowNormalDamp("Snow Damping", Range(0, 1)) = 0.85

        [Header(Height Ambient Occlusion)]
        // Low-lying ground is slightly darker (ambient light blocked by terrain).
        _AOHeightRange("AO Height Range", Float) = 1.8
        _AODarkness("AO Valley Darkness", Range(0.0, 1.0)) = 0.70

        [Header(Snow Sparkle)]
        // Per-pixel ice crystals catch specular light as glinting sparkles.
        _SparkleScale("Crystal Size", Float) = 7.0
        _SparkleDensity("Sparkle Density", Range(0.0, 0.5)) = 0.12
        _SparkleSmooth("Sparkle Smoothness", Range(0.0, 1.0)) = 0.97
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "UniversalMaterialType"="Lit"
            "IgnoreProjector"="True"
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
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _SCREEN_SPACE_IRRADIANCE
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Assets/Shaders/Include/GlobalCloudShadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
            };

            TEXTURE2D(_BaseMap);       SAMPLER(sampler_BaseMap);
            TEXTURE2D(_DetailNormalMap); SAMPLER(sampler_DetailNormalMap);
            TEXTURE2D(_ShorelineDistanceMap); SAMPLER(sampler_ShorelineDistanceMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float  _TextureBlend;
                float  _BlendMode;

                float  _VariationScale;
                float  _VariationStrength;
                half4  _VariationColor;

                half4  _SandColor;
                half4  _GrassColor;
                half4  _MidColor;
                half4  _RockColor;
                half4  _SnowColor;

                float  _BeachHeightMax;
                float  _BeachBlend;
                float  _BeachSlopeMax;
                float  _BeachSlopeMin;
                float  _BeachNoiseScale;
                float  _BeachNoiseStrength;
                float  _LegacyBeachStrength;

                float4 _ShorelineMapOrigin;
                float4 _ShorelineMapSize;
                float  _ShorelineMapFlipX;
                float  _ShorelineMapFlipY;
                float  _ShorelineMapMaxDistance;
                float  _ShorelineBeachWidth;
                float  _ShorelineBeachSoftness;
                float  _ShorelineBeachStrength;
                half4  _ShorelineOverlayColor;
                float  _ShorelineOverlayStrength;
                float  _ShorelineTextureOverride;
                float  _ShorelineNormalOverride;
                float  _ShorelineBreakupScale;
                float  _ShorelineBreakupStrength;
                float  _ShorelineCliffOverride;
                float  _ShorelineCliffExclusionSoftness;
                float  _ShorelineSlopeFadeStart;
                float  _ShorelineSlopeFadeEnd;
                float  _ShorelineSteepStrength;
                float  _ShorelineDebugMode;

                float  _SnowHeightMin;
                float  _SnowHeightMax;
                float  _SnowBlend;
                float  _SnowSlopeMax;
                float  _SnowSlopeMin;

                float  _GrassHeightMin;
                float  _GrassHeightMax;

                float  _MidHeightMin;
                float  _MidHeightMax;

                float  _RockHeightMin;
                float  _RockHeightMax;

                float  _Metallic;

                // Per-zone smoothness
                float  _SmoothnessBeach;
                float  _SmoothnessGrass;
                float  _SmoothnessMid;
                float  _SmoothnessRock;
                float  _SmoothnessSnow;
                float  _SmoothnessCliff;

                // Cliff texture
                float  _CliffTextureFade;

                // Atmosphere fog
                half4  _AtmosFogColor;
                float  _AtmosFogStart;
                float  _AtmosFogEnd;
                float  _AtmosFogStrength;

                // Cloud Shadow custom overrides
                half4 _IslandShadowColor;
                float _IslandShadowIntensity;
                float _SunQuenchPower;
                float _ShadowLift;

                // Cliff
                half4  _CliffColor;
                float  _CliffSlopeStart;
                float  _CliffSlopeEnd;
                float  _CliffStrength;

                // Forest
                half4  _ForestColor;
                float  _ForestScale;
                float  _ForestThreshold;
                float  _ForestSoftness;
                float  _ForestStrength;
                float  _ForestHeightMin;
                float  _ForestHeightMax;

                // Farm
                half4  _FarmColor1;
                half4  _FarmColor2;
                half4  _FarmColor3;
                half4  _FarmBorderColor;
                float  _FarmScale;
                float  _FarmBorderWidth;
                float  _FarmStripeFreq;
                float  _FarmStripeContrast;
                float  _FarmSlopeMax;
                float  _FarmHeightMin;
                float  _FarmHeightMax;
                float  _FarmStrength;

                // Roads
                float  _RoadStrength;
                half4  _RoadColor;
                float  _RoadWidth;
                float  _RoadSoftness;
                float  _RoadSeed;
                float  _RoadDensity;
                float  _RoadBranchCount;
                float  _RoadRadius;
                float  _RoadMeanderScale;
                float  _RoadMeanderStrength;
                float  _RoadHeightMin;
                float  _RoadHeightMax;
                float  _RoadSlopeMax;
                float  _RoadSmoothness;
                float  _RoadNormalDamp;
                float  _RoadTextureOverride;
                float  _RoadMaxDarken;
                float  _RoadMaskCutoff;
                float  _RoadMaskContrast;
                float  _RoadDebugMode;
                float  _RoadAnchorCount;
                float4 _RoadAnchor0;
                float4 _RoadAnchor1;
                float4 _RoadAnchor2;
                float4 _RoadAnchor3;
                float4 _RoadAnchor4;
                float4 _RoadAnchor5;
                float4 _RoadAnchor6;
                float4 _RoadAnchor7;

                // Wetness
                half4  _WetColor;
                float  _WetDepth;
                float  _WetStrength;

                // Detail normal map
                float4 _DetailNormalMap_ST;
                float  _DetailNormalScale;
                float  _DetailNormalStrength;
                float  _DetailNormalFineScale;
                float  _DetailNormalFineStrength;
                float  _CliffNormalBoost;
                float  _CoastNormalBoost;
                float  _SnowNormalDamp;

                // Height AO
                float  _AOHeightRange;
                float  _AODarkness;

                // Snow sparkle
                float  _SparkleScale;
                float  _SparkleDensity;
                float  _SparkleSmooth;
            CBUFFER_END

            // ── Blend mode library ────────────────────────────────────────────
            // Multiply: darkens.  Result always <= both inputs.
            half3 BlendMultiply(half3 base, half3 blend) { return base * blend * 2.0; }

            // Overlay: contrast boost. Dark base*blend, bright base screens.
            half3 BlendOverlay(half3 base, half3 blend)
            {
                return lerp(2.0 * base * blend,
                            1.0 - 2.0 * (1.0 - base) * (1.0 - blend),
                            step(0.5, base));
            }

            // Screen: always lightens. Good when zone is bright.
            half3 BlendScreen(half3 base, half3 blend) { return 1.0 - (1.0 - base) * (1.0 - blend); }

            // Soft Light: gentle overlay, very natural feel.
            half3 BlendSoftLight(half3 base, half3 blend)
            {
                return (1.0 - 2.0 * blend) * base * base + 2.0 * blend * base;
            }

            // Interpolates between all four modes using _BlendMode (0-3)
            half3 ApplyBlend(half3 base, half3 blend, float mode)
            {
                half3 m = BlendMultiply(base, blend);   // 0
                half3 o = BlendOverlay(base, blend);    // 1
                half3 s = BlendScreen(base, blend);     // 2
                half3 sl = BlendSoftLight(base, blend); // 3
                half3 r = lerp(m,  o,  saturate(mode));
                       r = lerp(r,  s,  saturate(mode - 1.0));
                       r = lerp(r,  sl, saturate(mode - 2.0));
                return r;
            }

            // Lightweight value noise for breaking up hard edges
            float hash(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float vnoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i), hash(i + float2(1,0)), u.x),
                            lerp(hash(i + float2(0,1)), hash(i + float2(1,1)), u.x), u.y);
            }

            // ── Voronoi cellular noise for farm parcels ─────────────────────────
            // Returns: x=F1 (dist to nearest cell centre)
            //          y=F2-F1 (distance to nearest edge - small near borders)
            //          zw=cell ID (integer grid coords of the winning cell)
            float4 FarmVoronoi(float2 p)
            {
                float2 ip = floor(p);
                float2 fp = frac(p);
                float F1 = 8.0, F2 = 8.0;
                float2 cellId = float2(0.0, 0.0);
                UNITY_UNROLL
                for (int j = -1; j <= 1; j++)
                UNITY_UNROLL
                for (int i = -1; i <= 1; i++)
                {
                    float2 g   = float2(i, j);
                    float2 gid = ip + g;
                    float2 rnd = float2(
                        frac(sin(dot(gid, float2(127.1, 311.7))) * 43758.545),
                        frac(sin(dot(gid, float2(269.5, 183.3))) * 12345.678)
                    );
                    float2 r = g + rnd - fp;
                    float  d = dot(r, r); // squared
                    if (d < F1) { F2 = F1; F1 = d; cellId = gid; }
                    else if (d < F2) { F2 = d; }
                }
                return float4(sqrt(F1), sqrt(F2) - sqrt(F1), cellId);
            }

            float hash11(float p)
            {
                return frac(sin(p * 127.1) * 43758.5453);
            }

            float4 GetRoadAnchor(int index)
            {
                if (index == 0) return _RoadAnchor0;
                if (index == 1) return _RoadAnchor1;
                if (index == 2) return _RoadAnchor2;
                if (index == 3) return _RoadAnchor3;
                if (index == 4) return _RoadAnchor4;
                if (index == 5) return _RoadAnchor5;
                if (index == 6) return _RoadAnchor6;
                return _RoadAnchor7;
            }

            float RoadBranchMask(float2 worldXZ, float4 anchor, int branchIndex)
            {
                float2 anchorXZ = anchor.xy;
                float anchorRadius = min(max(anchor.z, 0.001), max(_RoadRadius, 0.001));
                float branchSeed = dot(anchorXZ, float2(12.9898, 78.233))
                                 + _RoadSeed * 37.719
                                 + branchIndex * 19.913;
                float activeRand = hash11(branchSeed + 1.71);
                float densityGate = step(activeRand, saturate(_RoadDensity));

                float angle = hash11(branchSeed + 5.37) * 6.2831853;
                float2 dir = float2(cos(angle), sin(angle));
                float2 side = float2(-dir.y, dir.x);
                float branchLength = anchorRadius * lerp(0.45, 1.0, hash11(branchSeed + 9.83));

                float2 rel = worldXZ - anchorXZ;
                float along = dot(rel, dir);
                float endFeather = max(_RoadWidth + _RoadSoftness, branchLength * 0.18);
                float alongMask = smoothstep(0.0, endFeather, along)
                                * (1.0 - smoothstep(branchLength - endFeather, branchLength, along));

                float meanderScale = max(abs(_RoadMeanderScale), 0.0001);
                float meanderA = vnoise(float2(along, branchSeed) * meanderScale);
                float meanderB = vnoise(float2(along, branchSeed + 41.0) * meanderScale * 2.17);
                float meander = ((meanderA * 0.7 + meanderB * 0.3) - 0.5)
                              * _RoadMeanderStrength
                              * smoothstep(0.0, endFeather, along);

                float halfWidth = max(_RoadWidth, 0.001);
                float softness = max(_RoadSoftness, 0.0001);
                float distToCenter = abs(dot(rel, side) - meander);
                float branchMask = 1.0 - smoothstep(halfWidth, halfWidth + softness, distToCenter);
                return branchMask * alongMask * densityGate;
            }

            float RoadMask(float2 worldXZ)
            {
                float count = min(_RoadAnchorCount, 8.0);
                float branchLimit = clamp(floor(_RoadBranchCount + 0.5), 0.0, 8.0);
                float mask = 0.0;

                UNITY_UNROLL
                for (int anchorIndex = 0; anchorIndex < 8; anchorIndex++)
                {
                    float anchorActive = step(anchorIndex + 0.5, count);
                    float4 anchor = GetRoadAnchor(anchorIndex);
                    float anchorStrength = saturate(anchor.w) * anchorActive;

                    float hubRadius = max(_RoadWidth * 1.8, 0.001);
                    float hubMask = 1.0 - smoothstep(
                        hubRadius,
                        hubRadius + max(_RoadSoftness, 0.0001),
                        distance(worldXZ, anchor.xy));
                    mask = max(mask, hubMask * anchorStrength * saturate(_RoadDensity));

                    UNITY_UNROLL
                    for (int branchIndex = 0; branchIndex < 8; branchIndex++)
                    {
                        float branchActive = step(branchIndex + 0.5, branchLimit);
                        mask = max(mask, RoadBranchMask(worldXZ, anchor, branchIndex) * anchorStrength * branchActive);
                    }
                }

                float grain = vnoise(worldXZ * 4.7 + float2(_RoadSeed, _RoadSeed));
                return saturate(mask * lerp(0.82, 1.08, grain));
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS  = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS  = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS    = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float  height = IN.positionWS.y;
                float3 nrm    = normalize(IN.normalWS);

                // slope: 0 = perfectly flat, 1 = perfectly vertical
                float slope = 1.0 - nrm.y;

                // ── Noise for organic beach edge ──────────────────────────────
                float noise = vnoise(IN.positionWS.xz * _BeachNoiseScale);

                // ── LEGACY BEACH MASK: low elevation + gentle slope ───────────
                float beachEdge = _BeachHeightMax + noise * _BeachNoiseStrength;
                float beachBlendSign = lerp(-1.0, 1.0, step(0.0, _BeachBlend));
                float safeBeachBlend = beachBlendSign * max(abs(_BeachBlend), 0.0001);
                float beachSlopeDelta = _BeachSlopeMin - _BeachSlopeMax;
                float beachSlopeSign = lerp(-1.0, 1.0, step(0.0, beachSlopeDelta));
                float safeBeachSlopeMin = _BeachSlopeMax + beachSlopeSign * max(abs(beachSlopeDelta), 0.0001);
                float beachH    = smoothstep(beachEdge, beachEdge - safeBeachBlend, height);
                float beachS    = smoothstep(_BeachSlopeMax, safeBeachSlopeMin, slope);
                float rawLegacyBeachMask = beachH * beachS;
                float legacyBeachMask = saturate(rawLegacyBeachMask * _LegacyBeachStrength);

                // ── FORCED SHORELINE BEACH MASK: simple land-distance ring ─────
                float2 shorelineMapSize = max(abs(_ShorelineMapSize.xy), float2(0.001, 0.001));
                float2 shorelineUV = (IN.positionWS.xz - _ShorelineMapOrigin.xy) / shorelineMapSize;
                float shorelineInMap = step(0.0, shorelineUV.x) * step(shorelineUV.x, 1.0)
                                     * step(0.0, shorelineUV.y) * step(shorelineUV.y, 1.0);
                float2 shorelineSampleUV = shorelineUV;
                shorelineSampleUV.x = lerp(shorelineSampleUV.x, 1.0 - shorelineSampleUV.x, step(0.5, _ShorelineMapFlipX));
                shorelineSampleUV.y = lerp(shorelineSampleUV.y, 1.0 - shorelineSampleUV.y, step(0.5, _ShorelineMapFlipY));
                float4 shorelineSample = SAMPLE_TEXTURE2D_LOD(
                    _ShorelineDistanceMap,
                    sampler_ShorelineDistanceMap,
                    saturate(shorelineSampleUV),
                    0);
                float shorelineMapDistance = shorelineSample.a;
                float shorelineSampledLand = 1.0 - smoothstep(0.001, 0.004, shorelineSample.b);
                float shorelineMapMaxDistance = max(_ShorelineMapMaxDistance, 0.0);
                float shorelineDistance = shorelineMapDistance * shorelineMapMaxDistance;
                float shorelineBaseWidth = max(_ShorelineBeachWidth, 0.0);
                float shorelineStrength = saturate(_ShorelineBeachStrength);

                // This is calibration, not an art knob. Values below 1 world unit are treated as off
                // so accidental tiny values cannot make the whole island evaluate as shoreline.
                float shorelineEnabled = shorelineInMap
                                      * shorelineSampledLand
                                      * step(1.0, shorelineMapMaxDistance)
                                      * step(0.0001, shorelineBaseWidth)
                                      * step(0.0001, shorelineStrength);

                float shorelineNoiseScale = max(abs(_ShorelineBreakupScale), 0.0001);
                float shorelineBreakup = (vnoise(IN.positionWS.xz * shorelineNoiseScale + float2(19.7, 31.3)) - 0.5)
                                       * max(_ShorelineBreakupStrength, 0.0);
                float shorelineWidth = max(shorelineBaseWidth + shorelineBreakup * shorelineEnabled, 0.0);
                float shorelineSoftness = max(_ShorelineBeachSoftness, 0.0);
                float shorelineSoftnessForSmooth = max(shorelineSoftness, 0.0001);
                float shorelineSoftRing = 1.0 - smoothstep(
                    max(0.0, shorelineWidth - shorelineSoftnessForSmooth),
                    shorelineWidth + shorelineSoftnessForSmooth,
                    shorelineDistance);
                float shorelineHardRing = step(shorelineDistance, shorelineWidth);
                float shorelineRing = lerp(shorelineHardRing, shorelineSoftRing, step(0.0001, shorelineSoftness))
                                    * shorelineEnabled;
                float shorelineSlopeFadeEnd = max(_ShorelineSlopeFadeEnd, _ShorelineSlopeFadeStart + 0.001);
                float shorelineSteepT = smoothstep(_ShorelineSlopeFadeStart, shorelineSlopeFadeEnd, slope);
                float shorelineSlopeMask = lerp(1.0, _ShorelineSteepStrength, shorelineSteepT);
                float forcedShorelineBeachMask = saturate(
                    shorelineRing * shorelineSlopeMask * shorelineStrength);

                // ── SNOW MASK: high elevation + gentle slope ───────────────────
                float snowH    = smoothstep(_SnowHeightMin, _SnowHeightMax, height);
                float snowS    = smoothstep(_SnowSlopeMax, _SnowSlopeMin, slope);
                float snowMask = snowH * snowS;

                // ── CLIFF DARKENING ───────────────────────────────────────────
                // Steep faces auto-darken to exposed rock. Reuses slope (free).
                float rawCliffMask = smoothstep(_CliffSlopeStart, _CliffSlopeEnd, slope);
                float shorelineCliffBlock = smoothstep(
                    max(0.0, _CliffSlopeStart - _ShorelineCliffExclusionSoftness),
                    _CliffSlopeStart,
                    slope);
                float shorelineCliffAllow = lerp(1.0 - shorelineCliffBlock, 1.0, _ShorelineCliffOverride);
                float shorelineOverlayMask = saturate(
                    forcedShorelineBeachMask * _ShorelineOverlayStrength * shorelineCliffAllow);
                float cliffMask = rawCliffMask;
                cliffMask *= (1.0 - forcedShorelineBeachMask * _ShorelineCliffOverride);
                // Suppress on snow so cliffs can still show snow
                cliffMask *= (1.0 - snowMask * 0.5);

                if (_ShorelineDebugMode > 0.5)
                {
                    float debugDistance = saturate(shorelineDistance / max(shorelineWidth + shorelineSoftness, 0.001));
                    float3 debugColor = shorelineMapDistance.xxx;
                    if (_ShorelineDebugMode > 1.5) debugColor = debugDistance.xxx;
                    if (_ShorelineDebugMode > 2.5) debugColor = forcedShorelineBeachMask.xxx;
                    if (_ShorelineDebugMode > 3.5) debugColor = shorelineOverlayMask.xxx;
                    if (_ShorelineDebugMode > 4.5) debugColor = rawLegacyBeachMask.xxx;
                    if (_ShorelineDebugMode > 5.5) debugColor = float3(shorelineMapDistance, shorelineSampledLand, shorelineOverlayMask);
                    return half4(debugColor, 1.0);
                }

                // ── GRASS MASK ───────────────────────────────────────────────
                float grassMask = smoothstep(_GrassHeightMin, _GrassHeightMax, height);

                // ── MID MASK (alpine meadow) ─────────────────────────────────
                float midMask = smoothstep(_MidHeightMin, _MidHeightMax, height);

                // ── ROCK MASK (bare stone) ──────────────────────────────────
                float rockMask = smoothstep(_RockHeightMin, _RockHeightMax, height);

                // ── ZONE COLOR: Sand → Grass → Mid → Rock → Snow ──────────────
                half3 zoneColor = _GrassColor.rgb;
                zoneColor = lerp(zoneColor, _SandColor.rgb, legacyBeachMask);
                zoneColor = lerp(zoneColor, _MidColor.rgb,  midMask);
                zoneColor = lerp(zoneColor, _RockColor.rgb, rockMask);
                zoneColor = lerp(zoneColor, _SnowColor.rgb, snowMask);

                // ── ZONE VARIATION RANDOMIZER ─────────────────────────────────
                // Layered noise at two frequencies for organic variation
                float var1 = vnoise(IN.positionWS.xz * _VariationScale);
                float var2 = vnoise(IN.positionWS.xz * _VariationScale * 2.7 + 5.3);
                float varNoise = var1 * 0.65 + var2 * 0.35; // 0..1
                // Damp variation in snow (snow should look cleaner)
                float varDamp = 1.0 - snowMask * 0.7;
                // Blend zone color toward _VariationColor where noise is high
                zoneColor = lerp(zoneColor, _VariationColor.rgb, varNoise * _VariationStrength * varDamp);

                zoneColor = lerp(zoneColor, _CliffColor.rgb, cliffMask * _CliffStrength);

                // ── FOREST CANOPY ─────────────────────────────────────────────
                // Large noise blobs in the grass/mid zone = canopy from above.
                // Two octaves: large blobs with clearing holes inside them.
                float forestNoise1 = vnoise(IN.positionWS.xz * _ForestScale);
                float forestNoise2 = vnoise(IN.positionWS.xz * _ForestScale * 3.1 + 7.3);
                // Clearings: subtract a finer noise so forest isn't wall-to-wall
                float forestRaw = forestNoise1 - forestNoise2 * 0.35;
                float forestMask = smoothstep(_ForestThreshold, _ForestThreshold + _ForestSoftness, forestRaw);
                // Only in grass/mid height band, not on cliffs or beach
                float forestHMask = smoothstep(_ForestHeightMin, _ForestHeightMin + 0.5, height)
                                  * smoothstep(_ForestHeightMax + 0.5, _ForestHeightMax, height);
                forestMask *= forestHMask * (1.0 - cliffMask) * (1.0 - legacyBeachMask);
                zoneColor = lerp(zoneColor, _ForestColor.rgb, forestMask * _ForestStrength);

                // ── FARM FIELDS (Voronoi parcels) ──────────────────────────────
                // Voronoi gives us: separate irregular parcels (F1/F2),
                // each with its own crop color and internal stripe angle.
                float2 farmUV = IN.positionWS.xz * _FarmScale;
                float4 voro   = FarmVoronoi(farmUV);
                float2 farmCellId = voro.zw;
                float  edgeDist   = voro.y; // small near field borders

                // Pick one of 3 crop colors per cell deterministically
                float colorRand = frac(sin(dot(farmCellId, float2(17.3, 53.7))) * 98765.4);
                half3 cropColor = _FarmColor1.rgb;
                cropColor = lerp(cropColor, _FarmColor2.rgb, step(0.33, colorRand));
                cropColor = lerp(cropColor, _FarmColor3.rgb, step(0.66, colorRand));

                // Per-cell random stripe angle (each parcel has its own row direction)
                float angleRand = frac(sin(dot(farmCellId, float2(91.3, 23.7))) * 54321.9);
                float angle     = angleRand * 3.14159;
                float2 rowDir   = float2(cos(angle), sin(angle));
                float rowCoord  = dot(IN.positionWS.xz, rowDir) * _FarmStripeFreq;
                // Subtle internal row (0.7-1.0 range so it doesn't dominate)
                float rowStripe = pow(abs(sin(rowCoord * 3.14159)), _FarmStripeContrast) * 0.25 + 0.75;

                // Dark hedgerow/border between parcels
                float border    = smoothstep(0.0, _FarmBorderWidth, edgeDist);
                half3 cellColor = lerp(_FarmBorderColor.rgb, cropColor * rowStripe, border);

                // Height + slope mask (flat, mid-elevation land only)
                float farmH    = smoothstep(_FarmHeightMin, _FarmHeightMin + 0.3, height)
                               * smoothstep(_FarmHeightMax + 0.3, _FarmHeightMax, height);
                float farmFlat = smoothstep(_FarmSlopeMax, _FarmSlopeMax * 0.3, slope);
                float farmMask = farmH * farmFlat * (1.0 - forestMask) * (1.0 - cliffMask);
                zoneColor = lerp(zoneColor, cellColor, farmMask * _FarmStrength);

                // ── WETNESS NEAR SHORE ────────────────────────────────────────
                // Transition zone between beach and grass is darker/more saturated.
                // Uses the legacy beach mask only; the virtual shoreline overlays later.
                float wetMask = smoothstep(0.0, _WetDepth, legacyBeachMask)
                              * smoothstep(_WetDepth + 0.1, _WetDepth * 0.4, legacyBeachMask)
                              * (1.0 - cliffMask);
                zoneColor = lerp(zoneColor, _WetColor.rgb, wetMask * _WetStrength);

                float roadMask = 0.0;
                float roadDebugAlpha = 0.0;
                half3 roadColor = _RoadColor.rgb;

                if (max(_RoadStrength, _RoadDebugMode) > 0.0001)
                {
                    // Paths and roads: anchored procedural trails in world XZ.
                    float roadMinHeight = min(_RoadHeightMin, _RoadHeightMax);
                    float roadMaxHeight = max(_RoadHeightMin, _RoadHeightMax);
                    float roadHeightSpan = max(roadMaxHeight - roadMinHeight, 0.001);
                    float roadHeightFeather = min(0.5, roadHeightSpan * 0.25);
                    float roadH = smoothstep(roadMinHeight, roadMinHeight + roadHeightFeather, height)
                                * (1.0 - smoothstep(roadMaxHeight - roadHeightFeather, roadMaxHeight, height));
                    float roadSlopeLimit = max(_RoadSlopeMax, 0.0001);
                    float roadFlat = 1.0 - smoothstep(roadSlopeLimit * 0.75, roadSlopeLimit, slope);
                    float roadTerrainMask = roadH
                                          * roadFlat
                                          * (1.0 - cliffMask)
                                          * (1.0 - snowMask)
                                          * (1.0 - saturate(legacyBeachMask + forcedShorelineBeachMask));
                    float rawRoadMask = RoadMask(IN.positionWS.xz) * roadTerrainMask;
                    float roadAlpha = saturate(
                        (rawRoadMask - _RoadMaskCutoff)
                        / max(1.0 - _RoadMaskCutoff, 0.001));
                    roadAlpha = pow(roadAlpha, max(_RoadMaskContrast, 0.001));
                    roadColor = _RoadColor.rgb * lerp(0.78, 1.12, vnoise(IN.positionWS.xz * 5.1 + float2(_RoadSeed, _RoadSeed)));

                    roadMask = roadAlpha * saturate(_RoadStrength);
                    roadDebugAlpha = roadAlpha;
                }

                // ── DETAIL NORMAL MAP ─────────────────────────────────────────────
                // World-space XZ projection — no UV stretching, works on any mesh.
                // Large octave = macro rock shapes. Fine octave = surface micro detail.
                float2 detUV1 = IN.positionWS.xz * _DetailNormalScale;
                float2 detUV2 = IN.positionWS.xz * (_DetailNormalScale * _DetailNormalFineScale);
                half3 dn1 = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detUV1),
                    _DetailNormalStrength);
                half3 dn2 = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, detUV2),
                    _DetailNormalFineStrength);
                // Combine octaves (XY = world XZ perturbation for Y-up terrain)
                float2 detXZ = dn1.xy + dn2.xy;
                // Per-zone strength: cliffs & coast boosted, snow & farms damped
                float normalMod = 1.0;
                normalMod = lerp(normalMod, _CoastNormalBoost, legacyBeachMask);
                normalMod = lerp(normalMod, _CliffNormalBoost, cliffMask);
                normalMod *= (1.0 - snowMask  * _SnowNormalDamp);
                normalMod *= (1.0 - farmMask  * 0.8);
                normalMod *= (1.0 - roadMask  * _RoadNormalDamp);
                // Perturb vertex normal in world space and renormalize
                float3 bumpedNrm = normalize(float3(
                    nrm.x + detXZ.x * normalMod,
                    nrm.y,
                    nrm.z + detXZ.y * normalMod
                ));
                bumpedNrm = normalize(lerp(bumpedNrm, nrm, shorelineOverlayMask * _ShorelineNormalOverride));

                // ── BASE TEXTURE + CLIFF FADE ───────────────────────────────────
                // Standard UV sample for all surfaces.
                half3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv).rgb;

                // Apply selected blend mode, then lerp with base by _TextureBlend.
                // The virtual shoreline can suppress this texture/detail before its color overlay.
                half3 blended = ApplyBlend(baseColor, zoneColor, _BlendMode);
                half3 albedo  = lerp(baseColor, blended, _TextureBlend);
                albedo = lerp(albedo, zoneColor, shorelineOverlayMask * _ShorelineTextureOverride);

                // On cliff faces the UV-mapped Tripo texture stretches badly.
                // Fade it out on steep slopes so the procedural cliff color shows cleanly.
                albedo = lerp(albedo, zoneColor, cliffMask * _CliffTextureFade);

                // ── HEIGHT AMBIENT OCCLUSION ─────────────────────────────────────
                // Valleys and low ground are darker — ambient light partially
                // blocked by surrounding terrain. Reuses existing `height`.
                float heightAO = saturate(height / max(_AOHeightRange, 0.01));
                albedo *= lerp(_AODarkness, 1.0, heightAO);

                // Roads are composited as a luminance-limited decal. This keeps a
                // broad or very dark road mask from crushing the island albedo.
                half3 lumaWeights = half3(0.2126, 0.7152, 0.0722);
                half3 roadTarget = lerp(albedo, roadColor, saturate(_RoadTextureOverride));
                half baseLuma = max(dot(albedo, lumaWeights), 0.001);
                half targetLuma = max(dot(roadTarget, lumaWeights), 0.001);
                half minRoadLuma = baseLuma * (1.0 - saturate(_RoadMaxDarken));
                roadTarget = saturate(roadTarget * max(1.0, minRoadLuma / targetLuma));
                albedo = lerp(albedo, roadTarget, roadMask);

                if (_RoadDebugMode > 0.5)
                {
                    albedo = lerp(albedo, half3(1.0, 0.08, 0.0), roadDebugAlpha);
                }

                // ── VIRTUAL SHORELINE OVERLAY ─────────────────────────────────────
                // Final decal-like layer: clean shoreline color over terrain texture,
                // cliffs, farms, forest, wetness, and normal-map detail.
                albedo = lerp(albedo, _ShorelineOverlayColor.rgb, shorelineOverlayMask);

                // ── ATMOSPHERE HEIGHT FOG ────────────────────────────────────
                // Distant terrain fades toward sky-haze color.
                // Most noticeable when camera tilts to capture the horizon.
                float camDist  = length(IN.positionWS - _WorldSpaceCameraPos.xyz);
                float fogRange = max(_AtmosFogEnd - _AtmosFogStart, 0.001);
                float fogT     = saturate((camDist - _AtmosFogStart) / fogRange);
                albedo = lerp(albedo, _AtmosFogColor.rgb, fogT * _AtmosFogStrength);

                // ── LIGHTING ──────────────────────────────────────────────────
                InputData inputData = (InputData)0;
                inputData.positionWS          = IN.positionWS;
                inputData.normalWS            = bumpedNrm;  // detail-bumped normal
                inputData.viewDirectionWS     = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                // Per-fragment shadow coord avoids cascade seam artifacts on large terrain triangles
                inputData.shadowCoord         = TransformWorldToShadowCoord(IN.positionWS);
                inputData.bakedGI             = SampleSH(bumpedNrm);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                inputData.shadowMask          = half4(1, 1, 1, 1);
                inputData.fogCoord            = 0;
                inputData.vertexLighting      = half3(0, 0, 0);

                // ── PER-ZONE SMOOTHNESS ────────────────────────────────────────
                // Beach = wet sand catches highlights. Cliff/rock = matte rough.
                // Snow = soft satin (not mirror, not dead matte).
                float zoneSmoothness = _SmoothnessGrass;
                zoneSmoothness = lerp(zoneSmoothness, _SmoothnessBeach, legacyBeachMask);
                zoneSmoothness = lerp(zoneSmoothness, _SmoothnessMid,   midMask);
                zoneSmoothness = lerp(zoneSmoothness, _SmoothnessRock,  rockMask);
                zoneSmoothness = lerp(zoneSmoothness, _SmoothnessSnow,  snowMask);
                zoneSmoothness = lerp(zoneSmoothness, _SmoothnessCliff, cliffMask);
                zoneSmoothness = lerp(zoneSmoothness, _SmoothnessBeach, shorelineOverlayMask);
                zoneSmoothness = lerp(zoneSmoothness, _RoadSmoothness, roadMask);

                // ── SNOW SPARKLE ────────────────────────────────────────────────
                // Per-pixel hash selects which snow pixels are ice crystals.
                // Those pixels get a sharp smoothness spike → specular glint.
                float sparkleHash = frac(sin(dot(
                    floor(IN.positionWS.xz * _SparkleScale),
                    float2(127.1, 311.7))) * 43758.5453);
                float sparkleFire = step(1.0 - _SparkleDensity, sparkleHash) * snowMask;
                zoneSmoothness = lerp(zoneSmoothness, _SparkleSmooth, sparkleFire);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo     = albedo;
                surfaceData.metallic   = _Metallic;
                surfaceData.smoothness = zoneSmoothness;
                surfaceData.normalTS   = half3(0.0, 0.0, 1.0);
                surfaceData.emission   = half3(0.0, 0.0, 0.0);
                surfaceData.occlusion  = 1.0;
                surfaceData.alpha      = 1.0;

                half4 color = UniversalFragmentPBR(inputData, surfaceData);

                // ── SHADOW LIFT ─────────────────────────────────────────────────
                // Floors how dark shadows can get. Instead of going pure black,
                // shadowed areas stay at least (_ShadowLift * albedo) bright.
                // This simulates realistic atmospheric/ambient fill light.
                half3 shadowFloor = half3(albedo) * _ShadowLift;
                color.rgb = max(color.rgb, shadowFloor);
                
                // --- CUSTOM TERRAIN SHADOW TWEAKING ---
                float cloudShadowRaw = GetGlobalCloudShadowRawMask(IN.positionWS);
                
                // Combine global animated shadow opacity with our local Island Material intensity dial
                float shadowPower = saturate(cloudShadowRaw * _GlobalCloudShadowTint.a * _IslandShadowIntensity);
                
                // 1. Base organic color darkening (Using local IslandShadowColor instead of the Global Tint)
                half3 shadowedColor = color.rgb * _IslandShadowColor.rgb;
                color.rgb = lerp(color.rgb, shadowedColor, shadowPower);
                
                // 2. HDR SUN REFLECTION QUENCHER (Using local SunQuenchPower)
                // Ensures ice sparkles or wet sand reflections don't pierce the cloud shadows
                float maxBrightness = 100.0 / (1.0 + shadowPower * _SunQuenchPower);
                float currentBrightness = max(color.r, max(color.g, color.b));
                
                if (currentBrightness > maxBrightness)
                {
                    color.rgb *= (maxBrightness / currentBrightness);
                }
                
                return color;
            }
            ENDHLSL
        }

        // Shadow caster pass (required for correct shadow casting)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma vertex IslandDepthOnlyVertex
            #pragma fragment IslandDepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct DepthOnlyAttributes
            {
                float4 positionOS : POSITION;
            };

            struct DepthOnlyVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            DepthOnlyVaryings IslandDepthOnlyVertex(DepthOnlyAttributes IN)
            {
                DepthOnlyVaryings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 IslandDepthOnlyFragment(DepthOnlyVaryings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode"="DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex IslandDepthNormalsVertex
            #pragma fragment IslandDepthNormalsFragment
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct DepthNormalsAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct DepthNormalsVaryings
            {
                float4 positionCS : SV_POSITION;
                half3 normalWS    : TEXCOORD0;
            };

            DepthNormalsVaryings IslandDepthNormalsVertex(DepthNormalsAttributes IN)
            {
                DepthNormalsVaryings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            void IslandDepthNormalsFragment(
                DepthNormalsVaryings IN,
                out half4 outNormalWS : SV_Target0
                #ifdef _WRITE_RENDERING_LAYERS
                , out uint outRenderingLayers : SV_Target1
                #endif
            )
            {
                float3 normalWS = NormalizeNormalPerPixel(IN.normalWS);

                #if defined(_GBUFFER_NORMALS_OCT)
                    float2 octNormalWS = PackNormalOctQuadEncode(normalWS);
                    float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);
                    half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);
                    outNormalWS = half4(packedNormalWS, 0.0);
                #else
                    outNormalWS = half4(normalWS, 0.0);
                #endif

                #ifdef _WRITE_RENDERING_LAYERS
                    outRenderingLayers = EncodeMeshRenderingLayer();
                #endif
            }
            ENDHLSL
        }
    }
}
