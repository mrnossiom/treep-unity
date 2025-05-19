Shader "Custom/SoftFogRoundedRect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Width ("Visible Width", Range(0, 1)) = 0.5
        _Height ("Visible Height", Range(0, 1)) = 0.3
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.1
        _Fade ("Edge Fade", Range(0.001, 1.0)) = 0.1
        _Color ("Tint Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
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
            float _Width;
            float _Height;
            float _CornerRadius;
            float _Fade;
            float4 _Color;

            float roundedBox(float2 p, float2 halfSize, float radius)
            {
                float2 d = abs(p) - halfSize + radius;
                return length(max(d, 0.0)) - radius;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;
                
                float2 halfSize = float2(_Width * 0.5, _Height * 0.5);
                float dist = roundedBox(uv, halfSize, _CornerRadius);

                float alpha = smoothstep(0.0, _Fade, dist);

                fixed4 texColor = tex2D(_MainTex, i.uv);
                texColor *= _Color;
                texColor.a *= alpha;

                return texColor;
            }
            ENDCG
        }
    }
}
