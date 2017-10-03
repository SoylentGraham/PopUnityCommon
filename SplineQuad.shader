// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "NewChromantics/Spline Quad"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		Radius("Radius",Range(0.01,2.0) ) = 1
		SkewBlur_Square("SkewBlur_Square", Range(0,1) ) = 0
		SkewBlur_SquareInverse("SkewBlur_SquareInverse", Range(0,1) ) = 0
		SkewBlur_SquareSquareInverse("SkewBlur_SquareSquareInverse", Range(0,1) ) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom

			struct v2f
			{
				float4 Position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			#include "Spline.cginc"
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 Position : POSITION;
				float2 uv : TEXCOORD0;
			};



			sampler2D _MainTex;
			float4 _MainTex_ST;


			v2f vert (appdata v)
			{
				v2f o;
				o.Position = v.Position;
				o.uv = v.uv;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return tex2D( _MainTex, float2(i.uv) );
				return float4( i.uv, 0, 1 );
			}
			ENDCG
		}
	}
}
