Shader "Custom/Effects/Slow"
{
    Properties
    {
        _Color ("Ice Color", Color) = (0.22, 0.62, 1.0, 0.8)
        _Speed ("Rotation Speed", Float) = 0.85
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
                
                float angle = atan2(uv.y, uv.x);
                float rAngle = angle + _Time.y * _Speed;

                // 氷の結晶（6本の放射状のスリット）
                float crystal = cos(rAngle * 6.0);
                crystal = smoothstep(0.18, 0.78, crystal);

                // 同心円状のリング波紋
                float ring = sin(dist * 22.0 - _Time.y * 2.8) * 0.5 + 0.5;
                ring *= smoothstep(0.48, 0.32, dist);

                float alpha = (crystal * 0.45 + 0.55) * ring;
                alpha += smoothstep(0.1, 0.0, abs(dist - 0.32)); // 外周リングの追加

                alpha *= smoothstep(0.5, 0.06, dist);

                fixed4 finalCol = _Color;
                finalCol.a = alpha * _Color.a * i.color.a;

                return finalCol;
            }
            ENDCG
        }
    }
}
