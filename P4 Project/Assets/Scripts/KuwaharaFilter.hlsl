Texture2D _BlitTexture;
SamplerState sampler_BlitTexture;

struct region
{
    float3 mean;
    float variance;
};


region calcRegion(int2 lower, int2 upper, int samples, float2 UV, float2 texelSize)
{
    region r;
    
    float3 sum = 0.0;
    float3 squareSum = 0.0;

    for (int x = lower.x; x <= upper.x; x++)
    {
        for (int y = lower.y; y <= upper.y; y++)
        {
            float2 offset = float2(texelSize.x * x, texelSize.y * y);
            float3 tex = _BlitTexture.Sample(sampler_BlitTexture, UV + offset).rgb;

            sum += tex;
            squareSum += tex * tex;
        }
    }
    
    r.mean = sum / samples;
    
    float3 variance = abs((squareSum / samples) - (r.mean * r.mean));
    r.variance = length(variance);
    
    return r;
}

void Kuwahara_float(float2 UV, float2 TexelSize, float KernelSize, out float4 Color)
{
    int upper = (KernelSize - 1) / 2;
    int lower = -upper;
    int samples = (upper + 1) * (upper + 1);

    region r1 = calcRegion(int2(lower, lower), int2(0, 0),         samples, UV, TexelSize);
    region r2 = calcRegion(int2(0, lower),     int2(upper, 0),     samples, UV, TexelSize);
    region r3 = calcRegion(int2(lower, 0),     int2(0, upper),     samples, UV, TexelSize);
    region r4 = calcRegion(int2(0, 0),         int2(upper, upper), samples, UV, TexelSize);

    region best = r1;
    if (r2.variance < best.variance) best = r2;
    if (r3.variance < best.variance) best = r3;
    if (r4.variance < best.variance) best = r4;

    Color = float4(best.mean, 1.0);
}