Shader "PopUnityCommon/ColourToLum" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
	Pass
	{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
	
			struct VertexInput {
				float4 Position : POSITION;
				float2 uv_MainTex : TEXCOORD0;
			};
			
			struct FragInput {
				float4 Position : SV_POSITION;
				float2	uv_MainTex : TEXCOORD0;
			};

			
			float RgbaToLum(fixed4 Rgba)
			{
				//	gr: get proper lum weights
				float Lumr = Rgba.x * 0.3333f;
				float Lumg = Rgba.y * 0.3333f;
				float Lumb = Rgba.z * 0.3333f;
				return Lumr + Lumg + Lumb;
			}
			
			float GetLuminance(float2 BlockUv,int XOffset,int YOffset)
			{
				fixed4 Rgba = tex2D ( _MainTex, BlockUv );
				return RgbaToLum( Rgba );
			}
			
			FragInput vert(VertexInput In) {
				FragInput Out;
				Out.Position = mul (UNITY_MATRIX_MVP, In.Position );
				Out.uv_MainTex = In.uv_MainTex;
				return Out;
			}

			fixed4 frag(FragInput In) : SV_Target {
				
				float Lum = GetLuminance( In.uv_MainTex, 0, 0 );
	
				return float4( Lum, Lum, Lum, 1 );
				return fixed4(1,1,0,1);
			}

		ENDCG
			}	
	} 
}
