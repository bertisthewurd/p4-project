void InverseColor_float(float4 Color, out float4 InvColor)
{
    InvColor = float4(0, 0, 0, Color.a);
    InvColor.rgb = 1 - Color.rgb;
}