Shader "Unlit/GEffectsShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GrayScaleLevel ("Grayscale Level", Range(0, 1)) = 0.0
        _TunnelVisionLevel ("Tunnel Vision Level", Range(0, 1)) = 0.0
        _ScreenSizeAdjustment ("Screensize Adjustment", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Cull Off
            Lighting Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _GrayScaleLevel;
            float _TunnelVisionLevel;
            float _ScreenSizeAdjustment;

            float luminance(float3 c)
            {
                return dot(c, float3(0.2126, 0.7152, 0.0722));
            }

            float3 desaturate(float3 color, float amount)
            {
                float l = luminance(color);
                return lerp(color, float3(l, l, l), saturate(amount));
            }

            float cinematicVignette(float2 uv, float amount, float aspectAdjust)
            {
                amount = saturate(amount);
                if (amount <= 0.0)
                    return 0.0;

                float2 p = uv - 0.5;
                p.y *= aspectAdjust;
                float d = length(p);

                const float minOuterRadius = 0.15;
                float outerRadius = lerp(0.72, minOuterRadius, pow(amount, 1.15));
                float fadeWidth = lerp(0.015, 0.35, pow(amount, 1.6));
                float innerRadius = max(outerRadius - fadeWidth, 0.0);
                float vignette = smoothstep(innerRadius, outerRadius, d);

                float globalDarkening = smoothstep(0.80, 1.00, amount) * 0.75;
                float darkness = max(vignette, globalDarkening);
                darkness = lerp(darkness, 1.0, smoothstep(0.98, 1.0, amount));
                return saturate(darkness);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 color = desaturate(col.rgb, _GrayScaleLevel);
                float vignette = cinematicVignette(i.uv, _TunnelVisionLevel, _ScreenSizeAdjustment);
                color = lerp(color, float3(0.0, 0.0, 0.0), vignette);
                return float4(color, col.a);
            }
            ENDCG
        }
    }
}
