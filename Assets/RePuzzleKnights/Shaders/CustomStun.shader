Shader "Custom/Effects/Stun"
{
    Properties
    {
        _Color ("Ring Color", Color) = (1.0, 0.88, 0.15, 1.0)
        _Speed ("Spin Speed", Float) = 3.6
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

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;
                float dist = length(uv);
                
                // 横方向に引き伸ばして斜め回転のリングを作る
                float2 skewedUV = uv;
                skewedUV.y *= 2.4; 
                float ringDist = length(skewedUV);

                float ring = smoothstep(0.04, 0.0, abs(ringDist - 0.36));

                float angle = atan2(skewedUV.y, skewedUV.x);
                float spin = angle - _Time.y * _Speed;
                
                // 3つの周回する星
                float stars = cos(spin * 3.0);
                stars = pow(max(0.0, stars), 10.0);

                float finalGlow = ring * 0.45 + stars * ring * 2.0;

                fixed4 finalCol = _Color;
                finalCol.a = finalGlow * _Color.a * i.color.a;
                finalCol.a *= smoothstep(0.5, 0.18, dist);

                return finalCol;
            }
            ENDCG
        }
    }
}
