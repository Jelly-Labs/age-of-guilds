#ifndef CLOUD_NOISE_INCLUDED
#define CLOUD_NOISE_INCLUDED

// Lightweight value noise used by the FBM
float hashW_Cloud(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }

float vnoiseW_Cloud(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float2 u = f * f * (3.0 - 2.0 * f);
    return lerp(lerp(hashW_Cloud(i), hashW_Cloud(i + float2(1,0)), u.x),
                lerp(hashW_Cloud(i + float2(0,1)), hashW_Cloud(i + float2(1,1)), u.x), u.y);
}

// 4-Octave Fractional Brownian Motion for procedural cloud shapes.
// Optimized without loops to guarantee compiler unrolling for extreme performance.
float CalculateCloudFBM(float2 posXZ, float scale, float2 windSpeed, float time, float deform)
{
    // Apply wind scrolling
    float2 uv = posXZ * scale + (windSpeed * time);
    
    // Organic Domain Warping: Clouds mathematically 'roll' and morph as they move
    float2 warpOffset = float2(
        vnoiseW_Cloud(uv * 0.5 + time * 0.05) - 0.5,
        vnoiseW_Cloud(uv * 0.5 + float2(13.5, 42.1) - time * 0.07) - 0.5
    );
    uv += warpOffset * deform;
    
    float value = 0.0;
    float amplitude = 0.5;
    
    // Octave 1
    value += vnoiseW_Cloud(uv) * amplitude;
    
    // Octave 2
    uv *= 2.0; amplitude *= 0.5;
    value += vnoiseW_Cloud(uv) * amplitude;
    
    // Octave 3
    uv *= 2.0; amplitude *= 0.5;
    value += vnoiseW_Cloud(uv) * amplitude;
    
    // Octave 4
    uv *= 2.0; amplitude *= 0.5;
    value += vnoiseW_Cloud(uv) * amplitude;
    
    // Returns smoothly layered noise in range [0.0, 0.9375]
    return value;
}

// Global evaluation function that turns the raw noise into distinct puffy clouds
float GetCloudMask(float2 posXZ, float scale, float2 windSpeed, float time, float coverage, float softness, float deform)
{
    float rawNoise = CalculateCloudFBM(posXZ, scale, windSpeed, time, deform);
    
    // Inverse relationship: A higher 'coverage' drops the threshold lower
    // So if noise = 0.6, coverage = 0.7. threshold = (1.0 - 0.7) = 0.3. 0.6 - 0.3 = 0.3 (Passes!)
    float mappedNoise = rawNoise - (1.0 - coverage);
    
    // Smooth the transition edge to avoid jagged pixelation
    return smoothstep(0.0, max(0.001, softness), mappedNoise);
}

#endif
