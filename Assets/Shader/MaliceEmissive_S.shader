Shader "Custom/MiasmaEmissiveURP"
{
    Properties
    {
        _ColorA ("Color A (Purple)", Color) = (0.63, 0.0, 1.0, 1)
        _ColorB ("Color B (Red)",    Color) = (1.0, 0.0, 0.2, 1)
        _Dark   ("Dark Color",       Color) = (0.02, 0.0, 0.03, 1)

        _EmissionStrength ("Emission Strength", Range(0, 50)) = 8
        _PulseSpeed       ("Pulse Speed",       Range(0, 10)) = 2
        _PulseAmount      ("Pulse Amount",      Range(0, 5))  = 1.2

        _NoiseScale1 ("Noise Scale 1", Range(0.01, 10)) = 1.5
        _NoiseScale2 ("Noise Scale 2", Range(0.01, 10)) = 3.2
        _DistortStrength ("Distort Strength", Range(0, 2)) = 0.6

        _Alpha ("Alpha", Range(0,1)) = 0.8
        _CutLow ("Mask Low", Range(0,1)) = 0.35
        _CutHigh("Mask High", Range(0,1)) = 0.75
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorA;
                float4 _ColorB;
                float4 _Dark;

                float _EmissionStrength;
                float _PulseSpeed;
                float _PulseAmount;

                float _NoiseScale1;
                float _NoiseScale2;
                float _DistortStrength;

                float _Alpha;
                float _CutLow;
                float _CutHigh;
            CBUFFER_END

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

            // --- small hash/value noise ---
            float hash21(float2 p)
            {
                // cheap deterministic hash
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));

                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                for (int i = 0; i < 4; i++)
                {
                    v += a * noise(p);
                    p *= 2.02;
                    a *= 0.5;
                }
                return v;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float t = _Time.y;

                float2 uv = IN.uv;

                // two scrolling layers
                float2 uv1 = uv * _NoiseScale1 + float2(0.07, 0.03) * t;
                float2 uv2 = uv * _NoiseScale2 + float2(-0.03, 0.08) * t;

                float n1 = fbm(uv1);
                float n2 = fbm(uv2);

                float n = saturate(n1 * 0.7 + n2 * 0.6);

                // distortion by noise
                float2 duv = uv + (n * 2.0 - 1.0) * _DistortStrength * 0.05;
                float nd = fbm(duv * (_NoiseScale2 * 0.9) + float2(0.02, -0.05) * t);

                // final mask + shaping
                float m = saturate(lerp(n, nd, 0.6));
                m = pow(m, 1.6);

                float mask = smoothstep(_CutLow, _CutHigh, m);

                // pulse (slightly irregular)
float pulseA = sin(t * _PulseSpeed) * 0.5 + 0.5;
float pulseB = sin(t * (_PulseSpeed * 0.73) + m * 6.2831) * 0.5 + 0.5;
float pulse = saturate(lerp(pulseA, pulseB, 0.6));

// IMPORTANT: color is driven by pulse, not by noise
float3 pulsingCol = lerp(_ColorA.rgb, _ColorB.rgb, pulse);

// Noise only controls where the effect appears (mask) + how dark it gets
float3 baseCol = lerp(_Dark.rgb, pulsingCol, mask);

// Emission: don’t add base+emiss too aggressively (avoids “missing texture” pink)
float3 emiss = baseCol * _EmissionStrength;
float3 finalCol = emiss; // or: baseCol * 0.2 + emiss if you want a bit of non-emissive body


                float alpha = mask * _Alpha;
                return half4(finalCol, alpha);
            }
            ENDHLSL
        }
    }
}
