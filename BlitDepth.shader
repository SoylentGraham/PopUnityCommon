// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NewChromantics/DepthBlit" 
{
	Properties
	{
		//	far-near distance
		DepthMax ("DepthMax", Range(0,1000)) = 1000	

		//	camera.projectionMatrix.inverse of the ACTUAL CAMERA, as this blit is using an ortho camera to do texture->texture blit
		ScreenToViewMtx_Row0("ScreenToViewMtx_Row0", VECTOR) = (1,0,0,0)
		ScreenToViewMtx_Row1("ScreenToViewMtx_Row1", VECTOR) = (0,1,0,0)
		ScreenToViewMtx_Row2("ScreenToViewMtx_Row2", VECTOR) = (0,0,1,0)
		ScreenToViewMtx_Row3("ScreenToViewMtx_Row3", VECTOR) = (0,0,0,1)

		//	show some debug
		Debug_DepthBandRange("Debug_DepthBandRange", Range(0,40) ) = 0
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"


			float DepthMax;
			sampler2D _CameraDepthTexture;
			float4 ScreenToViewMtx_Row0;
			float4 ScreenToViewMtx_Row1;
			float4 ScreenToViewMtx_Row2;
			float4 ScreenToViewMtx_Row3;
			float Debug_DepthBandRange;

			struct v2f 
			{
			   float4 pos : SV_POSITION;
			   float4 scrPos:TEXCOORD1;
			};


			//Vertex Shader
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);

				//UNITY_TRANSFER_DEPTH(o.depth);

				o.scrPos=ComputeScreenPos(o.pos);
				o.scrPos.x = 1 - o.scrPos.x;

				//#if UNITY_UV_STARTS_AT_TOP
				//	o.scrPos.y = 1 - o.scrPos.y;
				//#endif
				return o;
			}

			float GetWorldDepth(v2f i)
			{
				//	gr: this is planar to the cubemap. Fine for cubemaps, but not real indication of DEPTH FROM EYE, so no good for equirect
				if ( false )
				{
					float Depth = DECODE_EYEDEPTH( SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.scrPos) );
					return Depth ;
				}
				else if ( false )
				{
					//	gr: this gives identical results to above
					float Depth = Linear01Depth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)).r);
					return Depth * DepthMax;
				}
				else if( true )
				{
					float Depth = DECODE_EYEDEPTH( SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.scrPos) );
					//	move to projection space
					float2 ScreenPos = (i.scrPos - 0.5f) * 2.0f;	
					float4 CameraRay = float4( ScreenPos.x, ScreenPos.y, 0, 1 );
					float4x4 ScreenToViewMtx = float4x4( ScreenToViewMtx_Row0, ScreenToViewMtx_Row1, ScreenToViewMtx_Row2, ScreenToViewMtx_Row3 );
					float4 ViewRay4 = mul( ScreenToViewMtx, CameraRay );
					float3 ViewRay = ViewRay4.xyz;
					//float3 ViewRay = normalize(ViewRay4.xyz);

					//Depth *= 0.20f;
					float3 ViewPos = ViewRay * Depth;
					return length( ViewPos );
				}
			}

			//Fragment Shader
			float4 frag (v2f i) : COLOR
			{
				float Depth = GetWorldDepth(i);

				//	gr: don't need to normalise, it's already normalised on the camera linear near->far
				float DepthNorm = Depth / DepthMax;

				bool ShowDebugBands = (Debug_DepthBandRange > 0.0f);

				if ( ShowDebugBands )
				{
					//	show bands
					int DepthInt = Depth * Debug_DepthBandRange;
					if ( ( DepthInt % 4) == 0 )
					{
						DepthNorm = 1;
					}
				}

  				DepthNorm = clamp( 0, 1, DepthNorm );
  				DepthNorm = 1 - DepthNorm;

  				return float4( DepthNorm, DepthNorm, DepthNorm, 1 );
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}