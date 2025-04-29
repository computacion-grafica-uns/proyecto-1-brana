Shader "CustomMatrixRenderer"
{
    SubShader
    {
        Pass
        {
            Cull Off
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            uniform float4x4 _ModelMatrix;
            uniform float4x4 _ViewMatrix;
            uniform float4x4 _ProjectionMatrix;
            uniform fixed4 _color;

            uniform fixed4 _LightColor;
            uniform float4 _LightPos;
            uniform float4x4 _PVMInverse;

            struct appdata {
                float4 vertex: POSITION;
                float3 normal: NORMAL;
            };
            struct v2f {
                float4 vertex: SV_POSITION;
                float3 normal: NORMAL;
            };

            v2f vert(appdata v) {
                v2f o;

                float4x4 PVM = mul ( _ProjectionMatrix, mul(_ViewMatrix, _ModelMatrix));
                o.vertex = mul(PVM , v.vertex); // Projection*View*Model
                
                // Transform normal by the transpose of the inverse of the Projection*View*Model matrix (computed on the CPU)
                o.normal = mul(transpose(_PVMInverse), v.normal);
                return o;
            }

            fixed4 frag(v2f i): SV_Target {
                // simple lighting model supporting only one light
                float ambientStrength = 0.1;
                float3 ambient = ambientStrength * _LightColor;

                float3 normalizedNormal = -normalize(i.normal);
                float3 lightDir = normalize(_LightPos - i.vertex);
                fixed diffuseFactor = max( dot(normalizedNormal, lightDir), 0.0);
                float3 diffuse = diffuseFactor * _LightColor;
                float3 finalColor = (ambient+diffuse) * _color;
                return half4(finalColor, 1.0);
            }

            ENDCG
        }
    }
}