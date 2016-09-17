Shader "NewChromantics/Distort" {
	Properties {
		_MainTex ("_MainTex", 2D) = "green" {}
		Invert("Invert",Int) = 0
		ZoomUv("ZoomUv", Range(-1,2) ) = 0
		Debug("Debug",Int) = 0
		LensOffsetX("LensOffsetX", Range(-1,1)) = 0
		LensOffsetY("LensOffsetY", Range(-1,1)) = 0
		RadialDistortionX("RadialDistortionX", Range(-1.57,1.57)) = 0
		RadialDistortionY("RadialDistortionY", Range(-1.57,1.57)) = 0
		TangentialDistortionX("TangentialDistortionX", Range(-1.57,1.57)) = 0
		TangentialDistortionY("TangentialDistortionY", Range(-1.57,1.57)) = 0
		K5Distortion("K5Distortion", Range(-1.57,1.57)) = 0
	}
SubShader 
	{
	Pass
	{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	#include "UnityCG.cginc"
			sampler2D _MainTex;
			int Debug;
			float RadialDistortionX;
			float RadialDistortionY;
			float TangentialDistortionX;
			float TangentialDistortionY;
			float K5Distortion;
			float LensOffsetX;
			float LensOffsetY;
			int Invert;
			float ZoomUv;


			struct VertexInput {
				float4 Position : POSITION;
				float2 uv_MainTex : TEXCOORD0;
			};
			
			struct FragInput {
				float4 Position : SV_POSITION;
				float2	uv_MainTex : TEXCOORD0;
			};
			
			
			FragInput vert(VertexInput In) {
				FragInput Out;
				
				Out.Position = mul (UNITY_MATRIX_MVP, In.Position );
				Out.uv_MainTex = In.uv_MainTex;

				return Out;
			}
			
  			//	http://stackoverflow.com/questions/21615298/opencv-distort-back
			float2 DistortPixel(float2 Point)
			{
				float Inverse = Invert?-1:1;
				float cx = LensOffsetX;
				float cy = LensOffsetY;
				float k1 = RadialDistortionX * Inverse;
				float k2 = RadialDistortionY * Inverse;
				float p1 = TangentialDistortionX * Inverse;
				float p2 = TangentialDistortionY * Inverse;
				float k3 = K5Distortion * Inverse;
		
		
			    float x = Point.x - cx;
    			float y = Point.y - cy;
			    float r2 = x*x + y*y;

			    // Radial distorsion
			    float xDistort = x * (1 + k1 * r2 + k2 * r2 * r2 + k3 * r2 * r2 * r2);
			    float yDistort = y * (1 + k1 * r2 + k2 * r2 * r2 + k3 * r2 * r2 * r2);

			    // Tangential distorsion
			    xDistort = xDistort + (2 * p1 * x * y + p2 * (r2 + 2 * x * x));
			    yDistort = yDistort + (p1 * (r2 + 2 * y * y) + 2 * p2 * x * y);

			    // Back to absolute coordinates.
			    xDistort = xDistort + cx;
			    yDistort = yDistort + cy;

			    return float2( xDistort, yDistort);
			}
			
			//	0..1 to -1..1
			float2 CenterUv(float2 uv)
			{
				uv = uv*float2(2,2) - float2(1,1);
				return uv;
			}
	
			float2 UncenterUv(float2 uv)
			{
				uv = (uv+float2(1,1)) / float2(2,2);
				return uv;
			}
			
			fixed4 frag(FragInput In) : SV_Target {
				
				float2 uv = In.uv_MainTex;
				uv = CenterUv(uv);
				uv *= 1.0f / ZoomUv;
				uv = DistortPixel( uv );
				uv = UncenterUv(uv);
				
				if ( uv.x > 1 )
					return float4(1,0,0,1);
				if ( uv.y > 1 )
					return float4(0,1,0,1);
				if ( uv.x < 0 )
					return float4(0,0,1,1);
				if ( uv.y < 0 )
					return float4(1,1,0,1);
						
				uv = CenterUv(uv);
				uv *= 1.0f / ZoomUv;
				uv = UncenterUv(uv);
				
				float4 Sample = tex2D( _MainTex, uv );
						
				if ( Debug )
					return float4( uv.x, uv.y, Sample.x, 1 );
					
				return Sample;
				
			}

		ENDCG
	}	
	} 
}