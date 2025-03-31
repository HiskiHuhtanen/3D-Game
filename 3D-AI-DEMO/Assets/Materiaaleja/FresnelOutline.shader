Shader "Custom/FresnelOutline"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1, 0.3, 0, 1) // Fire effect
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 2
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Blend SrcAlpha OneMinusSrcAlpha // For glow transparency
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 viewDir : TEXCOORD1;
            };

            float4 _OutlineColor;
            float _FresnelPower;
            float _GlowIntensity;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.viewDir = normalize(ObjSpaceViewDir(v.vertex)); // Gets view direction
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float fresnel = pow(1.0 - saturate(dot(i.viewDir, float3(0,0,1))), _FresnelPower);
                return _OutlineColor * fresnel * _GlowIntensity;
            }
            ENDCG
        }
    }
}
