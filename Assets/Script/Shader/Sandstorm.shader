Shader "Custom/Sandstorm"
{
    Properties
    {
        _Intensity ("Intensity", Range(0,2)) = 0.8
        _Tint ("Tint", Color) = (0.95,0.85,0.6,1)
        _NoiseScale ("Noise Scale", Range(0.1,4)) = 1.2
        _Distort ("Distort Strength", Range(0,2)) = 0.35
        _Grain ("Grain", Range(0,1)) = 0.25
        _Streak ("Streak", Range(0,2)) = 0.7
        _Vignette ("Vignette", Range(0,1)) = 0.35
        _WindDir ("Wind Dir (xy)", Vector) = (1,0.15,0,0)
        _Speed ("Speed", Range(0,3)) = 0.6
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Overlay" }
        ZWrite Off Cull Off ZTest Always
        Pass
        {
            Name "SandstormFullScreen"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Intensity;
            float4 _Tint;
            float _NoiseScale;
            float _Distort;
            float _Grain;
            float _Streak;
            float _Vignette;
            float2 _WindDir;
            float _Speed;

            float hash(float2 p){ return frac(sin(dot(p, float2(127.1,311.7)))*43758.5453); }
            float noise(float2 p){
                float2 i=floor(p), f=frac(p);
                float a=hash(i), b=hash(i+float2(1,0));
                float c=hash(i+float2(0,1)), d=hash(i+float2(1,1));
                float2 u=f*f*(3-2*f);
                return lerp(lerp(a,b,u.x), lerp(c,d,u.x), u.y);
            }

            float streakNoise(float2 uv, float2 dir)
            {
                float n=0;
                float2 p = uv;
                [unroll] for (int i=0;i<4;i++)
                {
                    float t = dot(p, normalize(dir))*2.0;
                    n += smoothstep(0.4,1.0, noise(float2(t, t*0.7 + i*10.0)));
                    p *= 1.8;
                }
                return saturate(n/4.0);
            }

            float vignette(float2 uv)
            {
                float2 d = uv*2-1; 
                float r = dot(d,d);
                return smoothstep(1.2, 0.2, r); 
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                float t = _Time.y * _Speed;

                float2 wind = normalize(_WindDir + 1e-5);
                float2 flowUV = uv + wind * t * 0.05;
                float n1 = noise(flowUV * _NoiseScale * 1.0 + t*0.5);
                float n2 = noise(flowUV * _NoiseScale * 2.3 - t*0.3);
                float warp = (n1*0.6 + n2*0.4 - 0.5);
                float2 duv = uv + wind * warp * _Distort * 0.02;

                float4 src = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, duv);

                float grain = noise(uv * (_NoiseScale*6.0) + t*2.1);

                float streak = streakNoise(uv + wind*t*0.1, wind);

                float sandMask = saturate(0.4 + warp*0.8); 
                float dust = saturate(_Grain*grain + _Streak*streak) * _Intensity;
                float fog = saturate(sandMask * _Intensity);

                float3 col = src.rgb;
                col = lerp(col, col*_Tint.rgb, fog*0.35);       
                col = lerp(col, _Tint.rgb, fog*0.25);           
                col += dust * 0.08 * _Tint.rgb;                 

                float vig = vignette(uv);
                col = lerp(col, col*vig, _Vignette);

                return float4(col, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
