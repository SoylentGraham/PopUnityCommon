// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "PopUnityCommon/FlipMirror" {
	Properties {
		_MainTex ("_MainTex", 2D) = "white" {}
		Flip("Flip", Int ) = 0
		Mirror("Mirror", Int ) = 0
	}
	SubShader {
	
	pass
	{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			struct VertexInput {
				float4 Position : POSITION;
				float2 uv_MainTex : TEXCOORD0;
			};
			
			struct FragInput {
				float4 Position : SV_POSITION;
				float2	uv_MainTex : TEXCOORD0;
			};

			sampler2D _MainTex;	//	new lum
			int	Flip;
			int	Mirror;
			
			FragInput vert(VertexInput In) {
				FragInput Out;
				Out.Position = UnityObjectToClipPos (In.Position );
				Out.uv_MainTex.y = Flip ? 1.0f - In.uv_MainTex.y : In.uv_MainTex.y;
				Out.uv_MainTex.x = Mirror ? 1.0f - In.uv_MainTex.x  : In.uv_MainTex.x;
				return Out;
			}
	
			float4 frag(FragInput In) : SV_Target 
			{
				return tex2D( _MainTex, In.uv_MainTex );
			}
		ENDCG
	}
	} 
}
