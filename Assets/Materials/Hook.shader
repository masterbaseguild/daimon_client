Shader "Custom/Hook"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Glow ("Glow Intensity", Range(1,10)) = 5
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Pass
        {
            Blend One One // Additive blending for glow
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float fresnel : TEXCOORD0;
            };

            float4 _Color;
            float _Glow;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.fresnel = pow(1.0 - dot(viewDir, v.normal), _Glow);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(_Color.rgb * i.fresnel, 1.0);
            }
            ENDCG
        }
    }
}