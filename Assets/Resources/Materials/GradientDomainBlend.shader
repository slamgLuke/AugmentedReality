Shader "Custom/GradientDomainBlend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CameraTex ("Camera Texture", 2D) = "white" {}
        _UnityTex ("Unity Texture", 2D) = "white" {}
        _BlendStrength ("Blend Strength", Range(0, 1)) = 0.5
        _ShowMask ("Show Mask", Float) = 0
    }
    
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            sampler2D _MainTex;
            sampler2D _CameraTex;
            sampler2D _UnityTex;
            float _BlendStrength;
            float _ShowMask;

            // Helper function to convert RGB to luminance
            float rgb2lum(float3 color)
            {
                return dot(color, float3(0.299, 0.587, 0.114));
            }

            // Poisson blending using 4-sample neighborhood
            float4 poissonBlend(float2 uv)
            {
                float4 cameraColor = tex2D(_CameraTex, uv);
                float4 unityColor = tex2D(_UnityTex, uv);
                
                // Calculate gradients
                float2 pixelSize = float2(ddx(uv.x), ddy(uv.y));
                
                // Camera image gradients
                float4 cameraRight = tex2D(_CameraTex, uv + float2(pixelSize.x, 0));
                float4 cameraBottom = tex2D(_CameraTex, uv + float2(0, pixelSize.y));
                float4 cameraCenter = cameraColor;
                
                // Unity content gradients
                float4 unityRight = tex2D(_UnityTex, uv + float2(pixelSize.x, 0));
                float4 unityBottom = tex2D(_UnityTex, uv + float2(0, pixelSize.y));
                float4 unityCenter = unityColor;
                
                // Calculate gradient differences
                float4 diffRight = (unityRight - unityCenter) - (cameraRight - cameraCenter);
                float4 diffBottom = (unityBottom - unityCenter) - (cameraBottom - cameraCenter);
                
                // Weighted blend based on luminance differences
                float lumDiff = abs(rgb2lum(unityColor.rgb) - rgb2lum(cameraColor.rgb));
                float blendWeight = saturate(lumDiff * _BlendStrength);
                
                // Blend between original and gradient-corrected version
                float4 blendedColor = unityColor - (diffRight + diffBottom) * 0.5;
                return lerp(unityColor, blendedColor, blendWeight);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                if (_ShowMask > 0.5)
                {
                    float4 cameraColor = tex2D(_CameraTex, i.uv);
                    float4 unityColor = tex2D(_UnityTex, i.uv);
                    float lumDiff = abs(rgb2lum(unityColor.rgb) - rgb2lum(cameraColor.rgb));
                    float blendWeight = saturate(lumDiff * _BlendStrength);
                    return fixed4(blendWeight.xxx, 1);
                }
                else
                {
                    return poissonBlend(i.uv);
                }
            }
            ENDCG
        }
    }
}