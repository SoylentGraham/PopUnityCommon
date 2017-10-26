// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NewChromantics/DrawTriangleWorldPos"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		TriangleCount("TriangleCount", int ) = 0

		Triangle_Uv_0_0("Triangle_Uv_0_0", VECTOR ) = (0,0,0,0)
		Triangle_Uv_0_1("Triangle_Uv_0_1", VECTOR ) = (0,0,0,0)
		Triangle_Uv_0_2("Triangle_Uv_0_2", VECTOR ) = (0,0,0,0)

		Triangle_Pos_0_0("Triangle_Pos_0_0", VECTOR ) = (1,0,0,0)
		Triangle_Pos_0_1("Triangle_Pos_0_1", VECTOR ) = (0,1,0,0)
		Triangle_Pos_0_2("Triangle_Pos_0_2", VECTOR ) = (0,0,1,0)

	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "PopCommon.cginc"

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

			sampler2D _MainTex;
			float4 _MainTex_ST;

			int TriangleCount;
			float2 Triangle_Uv_0_0;
			float2 Triangle_Uv_0_1;
			float2 Triangle_Uv_0_2;

			float3 Triangle_Pos_0_0;
			float3 Triangle_Pos_0_1;
			float3 Triangle_Pos_0_2;


			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}



			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
			
				if ( PointInsideTriangle( i.uv, Triangle_Uv_0_0, Triangle_Uv_0_1, Triangle_Uv_0_2 ) )
				{
					float3 a = Triangle_Pos_0_0;
					float3 b = Triangle_Pos_0_1;
					float3 c = Triangle_Pos_0_2;
					col.xyz = GetTriangleBarycentric3( i.uv, Triangle_Uv_0_0, Triangle_Uv_0_1, Triangle_Uv_0_2,a,b,c);
					//col.xyz = float3(1,1,0);
				}

				return col;
			}
			ENDCG
		}
	}
}
