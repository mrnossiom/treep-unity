Shader "Custom/SpriteBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Strength", Range(0, 0.02)) = 0.005
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _BlurSize;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 offset = float2(_BlurSize, _BlurSize);
                fixed4 col = tex2D(_MainTex, i.uv) * 0.2;
                col += tex2D(_MainTex, i.uv + offset) * 0.2;
                col += tex2D(_MainTex, i.uv - offset) * 0.2;
                col += tex2D(_MainTex, i.uv + float2(offset.x, -offset.y)) * 0.2;
                col += tex2D(_MainTex, i.uv - float2(offset.x, -offset.y)) * 0.2;
                return col;
            }
            ENDCG
        }
    }
}
