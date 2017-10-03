// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "NewChromantics/WorldSpaceLaserGrid"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		GridSize("GridSize", Range(0,10) ) = 0.5
		LineWidth("LineWidth", Range(0,1) ) = 0.1
		LineColour("LineColour", COLOR ) = (0,1,0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
	Cull off
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 WorldSpace : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float GridSize;
			float LineWidth;
			float4 LineColour;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.WorldSpace = mul(unity_ObjectToWorld,v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 World = i.WorldSpace.xyz;
				float3 Grid;
				float temp;

				Grid.x = fmod( World.x, GridSize );
				Grid.y = fmod( World.y, GridSize );
				Grid.z = fmod( World.z, GridSize );
				Grid = abs(Grid);

				if ( Grid.x < LineWidth || Grid.y < LineWidth || Grid.z < LineWidth )
					return LineColour;
				discard;
				return 1;
			}
			ENDCG
		}
	}
}
