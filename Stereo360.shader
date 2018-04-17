// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "New Chromantics/Stereo360"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[Toggle(ENABLE_STEREO)] _Stereo("Enable Stereo", Float) = 0
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

			#include "UnityCG.cginc"
			#include "PopCommon.cginc"


			//	global toggle
			int LeftEye = 0;


			struct appdata
			{
				float4 LocalPos : POSITION;
			};

			struct v2f
			{
				float4 ScreenPos : SV_POSITION;
				float3 WorldPos : TEXCOORD1;
			};

			sampler2D _MainTex;
			half4 _Stereo;
			float4 _MainTex_ST;


			bool IsStereoLeftEye()
			{
				return (LeftEye!=0) ? true : false;
			}

			float2 MonoUvToStereoUv(float2 uv)
			{
#ifdef ENABLE_STERE0
				bool Left = IsStereoLeftEye();
				float minu = Left ? 0 : 0.5f;
				float maxu = Left ? 0.5f : 1.0f;
				float minv = Left ? 0 : 0;
				float maxv = Left ? 1 : 1;

				return float2( lerp(minu,maxu,uv.x), lerp(minv,maxv,uv.y) );
#else
				return uv;
#endif
			}


			v2f vert (appdata v)
			{
				v2f o;
				o.WorldPos = mul( unity_ObjectToWorld, v.LocalPos );
				o.ScreenPos = UnityObjectToClipPos(v.LocalPos);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 ViewDir = _WorldSpaceCameraPos.xyz - i.WorldPos;
				float2 EquirectUv = ViewToEquirect( ViewDir );

				//	gr: why upside down?
				EquirectUv.y = 1 - EquirectUv.y;

				EquirectUv = MonoUvToStereoUv( EquirectUv );
				float4 Colour = tex2D( _MainTex, EquirectUv );

				return Colour;
			}
			ENDCG
		}
	}
}
