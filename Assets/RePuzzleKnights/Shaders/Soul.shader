Shader "Custom/Soul"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Main Tint", Color) = (1,1,1,1)
        
        [Header(Colors)]
        _ColorOuter ("Outer Color (Blue)", Color) = (0.0, 0.3, 1.0, 1.0)
        _ColorMid   ("Mid Color (Green)", Color)  = (0.0, 1.0, 0.6, 1.0)
        _ColorInner ("Inner Color (Orange)", Color) = (1.0, 0.4, 0.0, 1.0)
        _ColorCore  ("Core Color (White)", Color)  = (1.0, 1.0, 1.0, 1.0)

        [Header(Animation Settings)]
        _Speed ("Animation Speed", Float) = 1.0
        _WobbleStrength ("Wobble Strength", Range(0, 2)) = 1.0
        
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            
            // 追加したプロパティの変数宣言
            float4 _ColorOuter;
            float4 _ColorMid;
            float4 _ColorInner;
            float4 _ColorCore;
            float _Speed;
            float _WobbleStrength;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                // 座標の正規化
                float2 uv = IN.texcoord * 2.0 - 1.0;
                uv.y += 0.2; 
                
                float time = _Time.y * _Speed;

                // 多層の揺らぎ計算
                // _WobbleStrengthを乗算して揺れ幅を調整
                float noiseSlow = sin(uv.x * 4.0 + time * 1.5) * 0.06 * _WobbleStrength;
                noiseSlow += sin(uv.y * 6.0 - time * 1.2) * 0.04 * _WobbleStrength;

                float noiseMid = sin(uv.x * 12.0 + time * 3.5) * 0.03 * _WobbleStrength;
                noiseMid += sin(uv.y * 18.0 - time * 4.0) * 0.02 * _WobbleStrength;

                float noiseFast = sin(uv.x * 25.0 + time * 7.0) * 0.015 * _WobbleStrength;
                noiseFast += sin(uv.y * 35.0 - time * 9.0) * 0.01 * _WobbleStrength;
                
                // レイヤーごとのUV座標の作成
                float2 uvOuter = uv; 
                uvOuter.x += (noiseSlow + noiseMid * 0.5) * (uv.y + 0.5);
                
                float2 uvMidLayer = uv;
                uvMidLayer.x += (noiseSlow * 0.7 + noiseMid + noiseFast * 0.3) * (uv.y + 0.5);

                float2 uvInner = uv;
                uvInner.x += (noiseSlow * 0.3 + noiseFast) * (uv.y + 0.5);

                // 各カラーレイヤーの形状計算
                float dOuter = length(float2(uvOuter.x * (1.0 + pow(max(0.0, uvOuter.y + 0.5), 2.0) * 4.0), uvOuter.y));
                float outerLayer = smoothstep(0.5, 0.0, dOuter);
                outerLayer = pow(outerLayer, 1.5);

                float dMid = length(float2(uvMidLayer.x * (1.0 + pow(max(0.0, uvMidLayer.y + 0.4), 2.0) * 5.0), uvMidLayer.y + 0.05));
                float midLayer = smoothstep(0.35, 0.0, dMid);
                midLayer = pow(midLayer, 1.5);

                float dInner = length(float2(uvInner.x * (1.0 + pow(max(0.0, uvInner.y + 0.3), 2.0) * 6.0), uvInner.y + 0.1));
                float innerLayer = smoothstep(0.2, 0.0, dInner);
                innerLayer = pow(innerLayer, 1.2);

                float dCore = length(float2(uvInner.x * 2.0, uvInner.y + 0.15));
                float coreLayer = smoothstep(0.08, 0.0, dCore);

                // 色の合成
                float3 col = float3(0.0, 0.0, 0.0);
                col += _ColorOuter.rgb * outerLayer;       
                col += _ColorMid.rgb   * midLayer * 0.8;   
                col += _ColorInner.rgb * innerLayer * 0.9; 
                col += _ColorCore.rgb  * coreLayer * 1.2;  

                // 仕上げ
                col *= smoothstep(0.8, -0.5, uv.y);
                col = pow(col, float3(0.8, 0.8, 0.8));

                // 透過設定
                float alpha = saturate(max(col.r, max(col.g, col.b)));
                
                col *= IN.color.rgb;
                alpha *= IN.color.a;

                return float4(col, alpha);
            }
            ENDCG
        }
    }
}