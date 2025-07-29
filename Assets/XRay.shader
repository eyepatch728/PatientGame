Shader "Custom/XRayEffect"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _XRayColor ("X-Ray Color", Color) = (0.0, 0.8, 1.0, 1.0) // Bright blue-cyan color
        _OutlineColor ("Outline Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.02
        _Intensity ("X-Ray Intensity", Range(0.0, 2.0)) = 1.0
        _NoiseTexture ("Noise Texture", 2D) = "white" {}
        _NoiseIntensity ("Noise Intensity", Range(0.0, 0.5)) = 0.05
        _ScanLineSpeed ("Scan Line Speed", Range(0.0, 5.0)) = 2.0
        _ScanLineCount ("Scan Line Count", Range(1, 100)) = 30
        _ScanLineIntensity ("Scan Line Intensity", Range(0.0, 1.0)) = 0.1
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
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
                float4 screenPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _XRayColor;
            float4 _OutlineColor;
            float _OutlineWidth;
            float _Intensity;
            sampler2D _NoiseTexture;
            float _NoiseIntensity;
            float _ScanLineSpeed;
            float _ScanLineCount;
            float _ScanLineIntensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                // Sample noise texture
                float2 noiseUV = i.uv * 3.0 + float2(_Time.y * 0.1, _Time.y * 0.08);
                float noise = tex2D(_NoiseTexture, noiseUV).r * _NoiseIntensity;
                
                // Create scan lines effect
                float scanLine = sin((i.uv.y * _ScanLineCount + _Time.y * _ScanLineSpeed) * 3.14159) * 0.5 + 0.5;
                scanLine = pow(scanLine, 5) * _ScanLineIntensity;
                
                // Calculate distance from edge
                float2 center = float2(0.5, 0.5);
                float2 distVector = abs(i.uv - center);
                float edge = 1.0 - max(distVector.x, distVector.y) * 2.0;
                edge = saturate(edge);
                
                // Create outline effect
                float outline = 0.0;
                if (edge > 0.0 && edge < _OutlineWidth * 20.0)
                {
                    outline = 1.0;
                }
                
                // Calculate final color
                float4 baseColor = _XRayColor * _Intensity;
                baseColor.rgb += noise;
                baseColor.rgb += scanLine;
                baseColor.rgb = lerp(baseColor.rgb, _OutlineColor.rgb, outline);
                
                // Apply circular mask
                float mask = saturate((0.5 - length(i.uv - center)) * 2.0);
                baseColor.a *= mask;
                
                // Make the center more transparent than the edges
                float centerMask = saturate(length(i.uv - center) * 2.0);
                baseColor.a *= lerp(0.5, 1.0, centerMask);
                
                return baseColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}