Shader "Custom/Effects/Burn"
{
    Properties
    {
        _Color1 ("Inner Flame Color", Color) = (1, 0.75, 0.1, 1)
        _Color2 ("Outer Flame Color", Color) = (1, 0.12, 0.0, 1)
        _Speed ("Flame Speed", Float) = 3.2
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

            fixed4 _Color1;
            fixed4 _Color2;
            float _Speed;

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i + float2(0.0,0.0)), hash(i + float2(1.0,0.0)), u.x),
                            lerp(hash(i + float2(0.0,1.0)), hash(i + float2(1.0,1.0)), u.x), u.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                float2 noiseUV = uv * float2(3.5, 1.8);
                noiseUV.y -= _Time.y * _Speed;

                float n = noise(noiseUV) * 0.58 + noise(noiseUV * 2.2 + _Time.y) * 0.28;

                float distFromCenter = abs(uv.x - 0.5);
                float flameShape = (1.0 - uv.y) * 0.48 - distFromCenter;
                
                float flameAmount = flameShape + n * (1.0 - uv.y) * 0.9;

                float alpha = smoothstep(0.0, 0.22, flameAmount);
                float inner = smoothstep(0.1, 0.32, flameAmount);

                fixed4 finalCol = lerp(_Color2, _Color1, inner);
                finalCol.a = alpha * i.color.a;

                // 最上部はフェードアウト
                finalCol.a *= (1.0 - uv.y);

                return finalCol;
            }
            ENDCG
        }
    }
}
