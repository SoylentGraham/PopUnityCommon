Shader "NewChromantics/Blit Triangle"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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

			#define TriangleCount	1
			#define MAX_TRIANGLE_COUNT	TriangleCount
			float2 TriangleUvs[MAX_TRIANGLE_COUNT*3];
			float4 TriangleColours[MAX_TRIANGLE_COUNT*3];


			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}


			fixed4 frag (v2f i) : SV_Target
			{
				//	start with original colour
				float4 Colour = tex2D(_MainTex, i.uv);

				int TriangleIndex = 0;
				for ( int t=0;	t<TriangleCount;	t++ )
				{
					int ia = (t*3)+0;
					int ib = (t*3)+1;
					int ic = (t*3)+2;
					float2 uva = TriangleUvs[ia];
					float2 uvb = TriangleUvs[ib];
					float2 uvc = TriangleUvs[ic];
					if ( !PointInsideTriangle( i.uv, uva, uvb, uvc ) )
						continue;
					
					//float3 Coloura = float3(1,0,0);
					//float3 Colourb = float3(0,1,0);
					//float3 Colourc = float3(0,0,1);
					float4 Coloura = TriangleColours[ia];
					float4 Colourb = TriangleColours[ib];
					float4 Colourc = TriangleColours[ic];
					Colour = GetTriangleBarycentric3( i.uv, uva, uvb, uvc, Coloura,Colourb,Colourc);
				}

				return Colour;
			}
			ENDCG
		}
	}
}
