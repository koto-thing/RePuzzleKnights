Shader "Custom/PathPreview"
{
    Properties
    {
        _MainTex ("Texture (Dotted Pattern)", 2D) = "white" {}
        _Color ("Base Color Tint", Color) = (1, 0.15, 0.15, 0.25)
        _PulseColor ("Pulse Color Tint", Color) = (1, 0.05, 0.05, 0.95)
        _ScrollSpeed ("Dotted Scroll Speed", Float) = 1.5
        _LineSpeed ("Pulse Speed (Run)", Float) = 0.35
        _BandWidth ("Pulse Band Width", Float) = 0.22
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
            #pragma target 2.0

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
                float2 uv_raw : TEXCOORD1;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _PulseColor;
            float _ScrollSpeed;
            float _LineSpeed;
            float _BandWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv_raw = v.uv; // タイリングなしの 0(スタート)〜1(ゴール)
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 点線のスクロール（ベース）
                float2 scrolledUV = i.uv;
                scrolledUV.x -= _Time.y * _ScrollSpeed;
                fixed4 texColor = tex2D(_MainTex, scrolledUV);

                float baseAlpha = 0.3;

                // 2. 導線上を移動する線分（パルス）
                float progress = frac(_Time.y * _LineSpeed);
                float diff = progress - i.uv_raw.x;
                
                // ループによる繋ぎ目補正
                if (diff < 0.0)
                {
                    diff += 1.0;
                }

                float pulseAlpha = 0.0;
                if (diff >= 0.0 && diff < _BandWidth)
                {
                    // 先端が最も明るく、後方に向けて減衰する尾
                    pulseAlpha = 1.0 - (diff / _BandWidth);
                    pulseAlpha = pow(pulseAlpha, 1.8);
                }

                // 3. ベースガイドラインと走るパルスのブレンド
                fixed4 finalColor;
                if (pulseAlpha > 0.0)
                {
                    finalColor = lerp(_Color * texColor * baseAlpha, _PulseColor * texColor * 1.5, pulseAlpha);
                    finalColor.a = max(_Color.a * texColor.a * baseAlpha, pulseAlpha * _PulseColor.a * texColor.a);
                }
                else
                {
                    finalColor = _Color * texColor;
                    finalColor.a *= baseAlpha;
                }

                // フェードアウト
                finalColor.a *= i.color.a;

                return finalColor;
            }
            ENDCG
        }
    }
}
