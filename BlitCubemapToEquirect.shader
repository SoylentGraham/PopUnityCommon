Shader "NewChromantics/BlitCubemapToEquirect"
{
	Properties
	{
		CubemapLeft ("CubemapLeft", 2D) = "white" {}
		CubemapRight ("CubemapRight", 2D) = "white" {}
		CubemapFront ("CubemapFront", 2D) = "white" {}
		CubemapBack ("CubemapBack", 2D) = "white" {}
		CubemapTop ("CubemapTop", 2D) = "white" {}
		CubemapBottom ("CubemapBottom", 2D) = "white" {}
		Cubemap("Cubemap", CUBE ) = "white" {}
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

			//	if neither keyword set, first is used
			#pragma multi_compile USE_MULTIFACES USE_CUBEMAP

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

			sampler2D CubemapLeft;
			sampler2D CubemapRight;
			sampler2D CubemapFront;
			sampler2D CubemapBack;
			sampler2D CubemapTop;
			sampler2D CubemapBottom;
			samplerCUBE Cubemap;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}

			//	gr: const int's were all 0 on DX
		#define TOP 0
		#define BOTTOM 1
		#define LEFT 2
		#define RIGHT 3
		#define FRONT 4
		#define BACK 5

			//	returns index and xy sample
			int GetCubemapIndex(float3 View,out float2 st)
			{
				//	gr: if this appears to be getting the wrong angle (left/right/forward/back), it's because the screens need to face the same direction as the actor

				float x = View.x;
				float y = View.y;
				float z = View.z;
				float ax = abs(x);
				float ay = abs(y);
				float az = abs(z);


				if (ax >ay && ax > az && x>=0)
				{
					st.x = -(z / ax);
					st.y = -y / ax;

					return RIGHT;
				}
				else if (ax >ay && ax > az && x<0)
				{
					st.x = (z / ax);
					st.y = -y / ax;

					return LEFT;
				}
				else if (ay > ax && ay > az && y>=0 )
				{
					st.x = x / ay;
					st.y = z / ay;

					return TOP;
				}
				else if (ay > ax && ay > az && y<0 )
				{
					st.x = x / ay;
					st.y = -(z / ay);

					return BOTTOM;
				}
				else if ( z >= 0 )
				{
					st.x = x / az;
					st.y = -y / az;
					return FRONT;
				}
				else
				{
					st.x = -(x / az);
					st.y = -y / az;
					return BACK;
				}

				return -1;
			}


			float4 SampleTexCube(float3 View)
			{
				float2 st;
				View = normalize(View);
				int TextureIndex = GetCubemapIndex( View, st );
				//return float4(View.x, View.y,View.z,1);
				//return float4(st.x,st.y,0,1);
				st += float2(1,1);
				st /= float2(2,2);
				st.y = 1 - st.y;

				bool RenderFaceDebug = false;
				if ( RenderFaceDebug )
				{
					if ( TextureIndex == TOP )		return float4( 1, 0, 0, 1 );//tex2D( CubemapTop, st );
					if ( TextureIndex == BOTTOM )	return float4( 0, 1, 0, 1 );//tex2D( CubemapBottom, st );
					if ( TextureIndex == LEFT )		return float4( 0, 0, 1, 1 );//tex2D( CubemapLeft, st );
					if ( TextureIndex == RIGHT )	return float4( 1, 1, 0, 1 );//tex2D( CubemapRight, st );
					if ( TextureIndex == FRONT )	return float4( 0, 1, 1, 1 );//tex2D( CubemapFront, st );
					if ( TextureIndex == BACK )		return float4( 1, 0, 1, 1 );//tex2D( CubemapBack, st );
				}        	
				if ( TextureIndex == TOP )		return tex2D( CubemapTop, st );
				if ( TextureIndex == BOTTOM )	return tex2D( CubemapBottom, st );
				if ( TextureIndex == LEFT )		return tex2D( CubemapLeft, st );
				if ( TextureIndex == RIGHT )	return tex2D( CubemapRight, st );
				if ( TextureIndex == FRONT )	return tex2D( CubemapFront, st );
				if ( TextureIndex == BACK )		return tex2D( CubemapBack, st );


				return float4( 1, 0, 1, 1 );
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//	rotate uv (??)
				float2 uv = i.uv.yx;

				#if !defined(USE_CUBEMAP)
				uv.y = 1 - uv.y;
				#endif

				float2 LatLon;


				//#if defined(USE_CUBEMAP)
				uv.y *= 2;
				//#endif

				LatLon.x = lerp( -UNITY_PI/2, UNITY_PI/2, uv.x );
				LatLon.y = lerp( -UNITY_PI/2, UNITY_PI/2, uv.y );
				float3 View = LatLonToView( LatLon );




				#if defined(USE_CUBEMAP)
				return float4( View, 1 );
					float4 Rgba = texCUBE( Cubemap, View );
					Rgba.a = 1;
					return Rgba;
				#elif USE_MULTIFACES
					
					return SampleTexCube( View );
				#endif
				return float4(1,0,0,1);
			}
			ENDCG
		}
	}
}
