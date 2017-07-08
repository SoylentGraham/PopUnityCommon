// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NewChromantics/FishEyeDistortion"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		
		ZoomUv("ZoomUv",float) = 1.0
		BarrelPower("BarrelPower", Range(-1,1) ) = 0.5
		AntiFisheyeMult("AntiFisheyeMult", Range(0,20) ) = 10.0
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
			#pragma exclude_renderers gles3
			#include "UnityCG.cginc"

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
			float ZoomUv;
			float BarrelPower;
			float AntiFisheyeMult;

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			float2 DistortPixel(float2 Point)
			{
				float2 fragCoord;
				fragCoord.x = (Point.x+1.0f)/2.0f;
				fragCoord.y = (Point.y+1.0f)/2.0f;
				float2 p = fragCoord.xy;//normalized coords with some cheat
				//(assume 1:1 prop)
				float2 m = float2(0.5, 0.5);//center coords
				float2 d = p - m;//vector from center to current fragment
				float r = sqrt(dot(d, d)); // distance of pixel from center
				
				float power = ( 2.0 * 3.141592 / (2.0 * sqrt(dot(m, m))) ) * (BarrelPower - 0.5);//amount of effect
				float bind;//radius of 1:1 effect
				bind = (power > 0.0)  ? sqrt(dot(m, m)) : m.y;//stick to corners
					
					
				//Weird formulas
				float2 uv;
				if (power > 0.0)//fisheye
					uv = m + normalize(d) * tan(r * power*AntiFisheyeMult) * bind / tan( bind * power*AntiFisheyeMult);
				else if (power < 0.0)//antifisheye
					uv = m + normalize(d) * atan(r * -power * AntiFisheyeMult) * bind / atan(-power * bind * AntiFisheyeMult);
				else
					uv = p;//no effect for power = 1.0
					
				uv.x = (uv.x * 2.0f) - 1.0f;
				uv.y = (uv.y * 2.0f) - 1.0f;
				return float2(uv.x, uv.y );
			}

			//	0..1 to -1..1
			float2 CenterUv(float2 uv)
			{
				//	gr: distort maths is for 0=bottom... so flip here for now
				uv.y = 1 - uv.y;

				uv = uv*float2(2,2) - float2(1,1);
				return uv;
			}

			float2 UncenterUv(float2 uv)
			{
				uv = (uv+float2(1,1)) / float2(2,2);

				//	gr: distort maths is for 0=bottom... so flip here for now
				uv.y = 1 - uv.y;
				return uv;
			}



			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = i.uv;
				
				uv = CenterUv(uv);
				uv *= 1.0f / ZoomUv;
				uv = DistortPixel( uv );
				uv = UncenterUv(uv);
	
	
				if ( uv.x > 1 )
				{
					return float4(1,0,0,1);
				}
				if ( uv.y > 1 )
				{
					return float4(0,1,0,1);
				}
				if ( uv.x < 0 )
				{
					return float4(0,0,1,1);
				}
				if ( uv.y < 0 )
				{
					return float4(1,1,0,1);
				}
				
				uv = CenterUv(uv);
				uv *= 1.0f / ZoomUv;
				uv = UncenterUv(uv);
				
	
				float4 Sample = tex2D( _MainTex, uv );
				return Sample;
			}
			
			ENDCG
		}
	}
}
