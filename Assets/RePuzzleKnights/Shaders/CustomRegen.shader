Shader "Custom/Effects/Regen"
{
    Properties
    {
        _Color ("Heal Color", Color) = (0.18, 0.98, 0.38, 1.0)
        _Speed ("Float Speed", Float) = 1.35
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            fixed4 _Color;
            float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            float drawPlus(float2 uv, float2 center, float size)
            {
                float2 d = abs(uv - center);
                float barWidth = size * 0.28;
                float barLength = size;

                float horizontal = (d.x < barLength && d.y < barWidth) ? 1.0 : 0.0;
                float vertical = (d.x < barWidth && d.y < barLength) ? 1.0 : 0.0;

                return max(horizontal, vertical);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float t = _Time.y * _Speed;

                // 3つのプラスマークの上昇スクロール
                float2 p1 = float2(0.3 + sin(t) * 0.1, frac(0.18 + t * 0.72));
                float plus1 = drawPlus(uv, p1, 0.075) * (1.0 - p1.y);

                float2 p2 = float2(0.72 + cos(t * 1.25) * 0.08, frac(0.48 + t * 0.88));
                float plus2 = drawPlus(uv, p2, 0.058) * (1.0 - p2.y);

                float2 p3 = float2(0.48 + sin(t * 0.68) * 0.12, frac(0.78 + t * 0.52));
                float plus3 = drawPlus(uv, p3, 0.068) * (1.0 - p3.y);

                float centerGlow = 1.0 - length(uv - float2(0.5, 0.15));
                centerGlow = pow(max(0.0, centerGlow), 4.2) * 0.38 * (1.0 - uv.y);

                float combined = plus1 + plus2 + plus3 + centerGlow;

                fixed4 finalCol = _Color;
                finalCol.a = combined * _Color.a * i.color.a;

                return finalCol;
            }
            ENDCG
        }
    }
}
