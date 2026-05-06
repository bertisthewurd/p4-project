Shader "Custom/FrameImageEffect"
{
    Properties
    {
        _MainTex        ("Texture",         2D)    = "white" {}
        _EffectIntensity("Effect Intensity", Range(0,1)) = 1.0

        [Header(Ripple)]
        _RippleStrength ("Ripple Strength", Range(0, 0.15)) = 0.06
        _RippleFrequency("Ripple Frequency",Range(1, 40))   = 12.0
        _RippleSpeed    ("Ripple Speed",    Range(0, 10))   = 4.0

        [Header(Blur)]
        _BlurRadius     ("Blur Radius",     Range(0, 30))  = 18.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "FrameImageEffect"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                float  _EffectIntensity;
                float  _RippleStrength;
                float  _RippleFrequency;
                float  _RippleSpeed;
                float  _BlurRadius;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float  t  = _Time.y;

                // --- Ripple (Mario 64-style UV distortion) ---
                float ripple       = _RippleStrength * _EffectIntensity;
                float freq         = _RippleFrequency;
                float animatedSpeed = _RippleSpeed * _EffectIntensity; // slows to a halt at intensity 0
                uv.x += sin(uv.y * freq       + t * animatedSpeed)        * ripple;
                uv.y += sin(uv.x * freq * 1.1 + t * animatedSpeed * 0.73) * ripple;

                // --- Box blur (mean kernel) ---
                // Kernel: 5x5 at full intensity, collapses toward 1x1 at zero intensity.
                float blurStep = _MainTex_TexelSize.x * _BlurRadius * _EffectIntensity;

                half4 col = 0;
                // Unrolled 5x5 kernel (25 samples). Using explicit offsets so the
                // compiler can optimise — a loop would work identically.
                [unroll]
                for (int dx = -2; dx <= 2; dx++)
                {
                    [unroll]
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        float2 offset = float2(dx, dy) * blurStep;
                        // Preserve aspect ratio using TexelSize y for vertical step
                        offset.y = dy * _MainTex_TexelSize.y * _BlurRadius * _EffectIntensity;
                        col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + offset);
                    }
                }
                col /= 25.0;

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
