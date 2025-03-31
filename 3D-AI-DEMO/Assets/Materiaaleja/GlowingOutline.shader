Shader "Custom/GlowingOutline"
{
    Properties
    {
        _OutlineWidth ("Outline Width", Range(0.001, 0.1)) = 0.02
        _OutlineColor ("Outline Color", Color) = (1, 0.5, 0, 1) // Orange
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Name "OUTLINE"
            Cull Front // Render an expanded version behind the mesh
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float _OutlineWidth;
            float4 _OutlineColor;

            v2f vert (appdata_t v)
            {
                v2f o;
                v.vertex.xyz += v.normal * _OutlineWidth; // Expand the mesh
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
}
