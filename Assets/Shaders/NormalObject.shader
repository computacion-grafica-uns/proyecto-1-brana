Shader "Lighting/NormalObject"
{
	SubShader
	{
		Pass
		{
			CGPROGRAM
			// declaramos que la funci�n llamada "vert" definir� el shader de v�rtices
			#pragma vertex vert
			// declaramos que la funci�n llamada "frag" definir� el shader de fragmentos
			#pragma fragment frag

			//datos de cada v�rtice
			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			//datos que pasaremos del shader de v�rtices al de fragmentos
			struct v2f {
				float4 vertex : SV_POSITION;
				float3 normal_o : TEXTCOORD0;
			};


			// shader de v�rtices
			v2f vert(appdata v)
			{
				v2f o;
				//veremos m�s adelante qu� hace esta funci�n
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal_o = v.normal;
				return o;
			}

			// shader de fragmentos
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 fragColor = 0;
				fragColor.rgb = i.normal_o * 0.5 + 0.5;
				return fragColor;
			}
			ENDCG
		}
	}
}