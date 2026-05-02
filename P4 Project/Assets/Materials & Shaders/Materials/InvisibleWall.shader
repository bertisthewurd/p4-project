Shader "Custom/DepthMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        
        ColorMask 0        // don't write any color
        ZWrite On          // DO write to depth buffer
        
        Pass {}
    }
}