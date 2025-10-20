Shader "Custom/SmoothGaussianBlur2D"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 1.0
        _BlurRadius ("Blur Radius", Int) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // x = 1/width, y = 1/height
            float _BlurSize;
            int _BlurRadius;

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

            // Gaussian weight calculation helper
            float Gaussian(float x, float sigma)
            {
                return exp(- (x * x) / (2.0 * sigma * sigma));
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float radius = max(_BlurRadius, 1);
                float2 texelSize = _MainTex_TexelSize.xy * _BlurSize;

                fixed4 colorSum = fixed4(0,0,0,0);
                float weightSum = 0.0;

                // Approximate Gaussian blur by sampling in a square kernel
                // with weights from Gaussian distribution

                float sigma = radius / 2.0;

                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        float2 offset = float2(x, y) * texelSize;
                        float weight = Gaussian(length(float2(x, y)), sigma);
                        fixed4 sample = tex2D(_MainTex, i.uv + offset);

                        colorSum += sample * weight;
                        weightSum += weight;
                    }
                }

                return colorSum / weightSum;
            }
            ENDCG
        }
    }
}
