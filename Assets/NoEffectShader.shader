Shader "Custom/NoEffectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            // Set the rendering behavior for sprites
            ZWrite Off
            Cull Off
            Lighting Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Shader properties
            sampler2D _MainTex;
            fixed4 _Color;

            // Vertex function
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // Convert the vertex position to clip space
                o.uv = v.texcoord; // Pass the texture coordinates to the fragment shader
                return o;
            }

            // Fragment function
            fixed4 frag(v2f i) : SV_Target
            {
                // Just sample the texture and multiply by the color
                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color;
                return texColor; // Return the color to be rendered
            }

            ENDCG
        }
    }

    Fallback "Diffuse"
}
