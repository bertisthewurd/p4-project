struct region
{
    float3 mean;
    float variance;
};

region calcRegion(int2 lower, int2 upper, int samples, float UV)
{
    region r;
    
    float3 sum = 0.0;
    float3 squareSum = 0.0;

    for (int x = lower.x; x <= upper.x; x++)
    {
        for (int y = lower.y; y <= upper.y; y++)
        {
            
        }
    }
    
    r.mean = sum / samples;
    
    float3 variance = abs((squareSum / samples) - (r.mean * r.mean));
    r.variance = length(variance);
    
    return r;
}

void Kuwahara(float4 UV, Texture2D Tex, SamplerState Samp, int KernelSize, out float4 Color)
{
    int upper = (KernelSize - 1) / 2;
    int lower = -upper;
    int samples = (upper + 1) * (upper + 1);
    
    
}