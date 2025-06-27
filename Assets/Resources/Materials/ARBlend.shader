Shader "Custom/ARBlend"
{
    Properties {
        _MainTex ("Unity Render", 2D) = "white" {}
        _ARCameraTex ("AR Camera Feed", 2D) = "white" {}
        _BlendFactor ("Blend", Range(0,1)) = 0.5
    }
    SubShader {
        Pass {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            sampler2D _MainTex, _ARCameraTex;
            float _BlendFactor;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };
            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target {
                fixed4 a = tex2D(_MainTex, i.uv);
                fixed4 b = tex2D(_ARCameraTex, i.uv);
                return lerp(a, b, _BlendFactor);
            }
            ENDCG
        }
    }
}
