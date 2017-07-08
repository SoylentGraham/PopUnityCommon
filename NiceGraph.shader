// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "GrahamIsNice/NiceGraph"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		ColourGood("ColourGood", COLOR ) = (0,1,0,1)
		ColourBad("ColourBad", COLOR ) = (0,0,0,1)

		Graph0("Graph0",VECTOR) = (0.0,0.1,0.2,0.3)
		Graph1("Graph1",VECTOR) = (0.4,0.5,0.6,0.7)
		Graph2("Graph2",VECTOR) = (0.8,0.9,1.0,0.9)
		Graph3("Graph3",VECTOR) = (0.0,0.1,0.2,0.3)
		Graph4("Graph4",VECTOR) = (0.0,0.1,0.2,0.3)
		Graph5("Graph5",VECTOR) = (0.0,0.1,0.2,0.3)
		Graph6("Graph6",VECTOR) = (0.0,0.1,0.2,0.3)
		Graph7("Graph7",VECTOR) = (0.0,0.1,0.2,0.3)
		Graph8("Graph8",VECTOR) = (0.0,0.1,0.2,0.3)
		Graph9("Graph9",VECTOR) = (0.0,0.1,0.2,0.3)

		GraphOffset("GraphOffset",Range(0,0.1)) = 0.1
		BlurSampleDistance("BlurSampleDistance", Range(0,0.01) ) = 0.0081
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};


			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 ColourGood;
			float4 ColourBad;
			float GraphOffset;
			float BlurSampleDistance;

			float4 Graph0,Graph1,Graph2,Graph3,Graph4,Graph5,Graph6,Graph7,Graph8,Graph9;
			#define GRAPH_VECTOR_COUNT	10
			#define GRAPH_COUNT	(GRAPH_VECTOR_COUNT*4)

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}




			float range(float Value,float Min,float Max)
			{
				return (Value-Min) / (Max - Min);
				}

			//	graph value, and it's T in the bar
			float2 GetGraphValue(float Time)
			{
				float4 Graphs[GRAPH_VECTOR_COUNT] = {
					Graph0,Graph1,Graph2,Graph3,Graph4,Graph5,Graph6,Graph7,Graph8,Graph9
				};

				float GraphIndexf = Time * (float)GRAPH_VECTOR_COUNT;
				int GraphIndex = (int)GraphIndexf;

				float4 GraphChunk = Graphs[GraphIndex];

				float ChunkNormal = (GraphIndexf - GraphIndex);

				int ChunkIndex = ChunkNormal * 4;

				float BarTime = 0;
				float GraphValue = 0;
				if ( ChunkIndex == 0 )	{	GraphValue = GraphChunk.x;	BarTime = range( ChunkNormal, 0.0f/4.0f, 1.0f/4.0f );	}
				if ( ChunkIndex == 1 )	{	GraphValue = GraphChunk.y;	BarTime = range( ChunkNormal, 1.0f/4.0f, 2.0f/4.0f );	}
				if ( ChunkIndex == 2 )	{	GraphValue = GraphChunk.z;	BarTime = range( ChunkNormal, 2.0f/4.0f, 3.0f/4.0f );	}
				if ( ChunkIndex == 3 )	{	GraphValue = GraphChunk.w;	BarTime = range( ChunkNormal, 3.0f/4.0f, 4.0f/4.0f );	}


				return float2( GraphValue, BarTime );
			}
	
			//	blurred value
			float GetGraphStrength(float2 uv)
			{
				float2 GraphValueChunkNormal = GetGraphValue( uv.x );
				float GraphValue = GraphValueChunkNormal.x;
				return uv.y > GraphValue ? 0 : 1;
			}

			float2 GetGraphStrengthChunk(float2 uv)
			{
				float2 GraphValueChunkNormal = GetGraphValue( uv.x );
				float GraphValue = GraphValueChunkNormal.x;
				float Strength = (uv.y > GraphValue ? 0 : 1);
				float GraphChunkNormal = GraphValueChunkNormal.y;

				return float2( Strength, GraphChunkNormal );
			}



			float4 GetMountainStrength(float2 uv)
			{
				float2 GraphStrength = GetGraphValue( uv );

				//float SampleOffset = GraphOffset;
				float SampleOffset = 1.0f / (float)GRAPH_COUNT;
							
				float2 GraphStrengthLeft = GetGraphValue( uv - float2(SampleOffset,0) );
				float2 GraphStrengthRight = GetGraphValue( uv + float2(SampleOffset,0) );

				float Strength = GraphStrength.x;
				float LerpDirection = ( GraphStrength.y - 0.5f ) * 2.0f;

				if ( LerpDirection < 0 )
				{
					//	-1..0 to 0..1
					float LerpAmount = abs(LerpDirection);
					LerpAmount /= 2.0f;
					Strength = lerp( GraphStrength.x, GraphStrengthLeft.x, LerpAmount );
				}
				else
				{
					//	/2 to meet half way
					float LeapAmount = LerpDirection / 2;
					Strength = lerp( GraphStrength.x, GraphStrengthRight.x, LeapAmount );
				}


				Strength = (uv.y > Strength ? 0 : 1);

				float4 Colour = lerp( ColourBad, ColourGood, Strength );
				//Colour.z = GraphStrength.y;
				//Colour.x = (uv.y > GraphStrength.x ? 0 : 1);
				return Colour;
			}

			float4 GetBlurredMountainStrength(float2 uv)
			{
				float4 Colour = GetMountainStrength(uv);

				//	now blur
				#define BLUR_STEP_COUNT	8
				float2 Steps[BLUR_STEP_COUNT] = { 
					float2( -1, -1 ),	float2( 0, -1 ),	float2( 1, -1 ),
					float2( -1, 0 ),	/*float2( 0, 0 ),*/	float2( 1,0 ),
					float2( -1, 1 ),	float2( 0, 1 ),	float2( 1, 1 )
				};

				for ( int i=0;	i<BLUR_STEP_COUNT;	i++ )
				{
					float2 SampleOffset = Steps[i] * BlurSampleDistance;
					Colour += GetMountainStrength( uv + SampleOffset );
				}
				Colour /= BLUR_STEP_COUNT+1;
				return Colour;
			}
		
			float4 GetBigBlurredMountainStrength(float2 uv)
			{
				float4 Colour = 0;
				//	now blur
				#define BIG_BLUR_STEP_COUNT	21
				float2 Steps[BIG_BLUR_STEP_COUNT] = { 
					/*float2( -2, -2 ),*/	float2( -1, -2 ),	float2( 0, -2 ),	float2(1,-2),	/*float2(2,-2),*/
					float2( -2, -1 ),	float2( -1, -1 ),	float2( 0, -1 ),	float2(1,-1),	float2(2,-1),
					float2( -2, 0 ),	float2( -1, -0 ),	float2( 0, 0 ),		float2(1,0),	float2(2,0),
					float2( -2, 1 ),	float2( -1, 1 ),	float2( 0, 1 ),		float2(1,1),	float2(2,1),
					/*float2( -2, 2 ),*/	float2( -1, 2 ),	float2( 0, 2 ),		float2(1,2),	/*float2(2,2),*/
				};
				float BlurWeights[BIG_BLUR_STEP_COUNT] = { 
					/*1,*/	1,	2,	1,	/*1,*/
						1,	3,	5,	3,	1,
						2,	5,	5,	5,	2,
						1,	3,	5,	3,	1,
					/*1,*/	1,	2,	1,	/*1,*/
				};

				float TotalWeight = 0;
				for ( int i=0;	i<BIG_BLUR_STEP_COUNT;	i++ )
				{
					float2 SampleOffset = Steps[i] * BlurSampleDistance;
					float Weight = BlurWeights[i];
					Colour += GetMountainStrength( uv + SampleOffset ) * Weight;
					TotalWeight += Weight;
				}
				Colour /= TotalWeight;
				return Colour;
			}
		


			float4 GetAverageStrength(float2 uv)
			{
				float GraphSampleOffset = GraphOffset;// 0.5f * ( 1.0f / GRAPH_COUNT );
				float Strength = 0;
				float Samples = 0;
				for ( float x=-4.0f;	x<=4.0f;	x+=1.0f )
				{
					float SampleStrength = GetGraphValue( uv + (x*GraphSampleOffset) ).x;
					Strength += (uv.y > SampleStrength ? 0 : 1) ;
					Samples += 1;
				}
				Strength /= Samples;
				float4 Colour = lerp( ColourBad, ColourGood, Strength );

				return Colour;
			}

			float4 GetSteppedStrength(float2 uv)
			{
				float Strength = 0;
				float Samples = 0;
				for ( float x=-4.0f;	x<=4.0f;	x+=1.0f )
				{
					float SampleStrength = GetGraphValue( uv + (x*GraphOffset) ).x;
					Strength += SampleStrength;
					Samples += 1;
				}
				Strength /= Samples;

				Strength = (uv.y > Strength ? 0 : 1) ;

				float4 Colour = lerp( ColourBad, ColourGood, Strength );

				return Colour;
			}


			fixed4 frag (v2f i) : SV_Target
			{
				return GetBigBlurredMountainStrength( i.uv );
				//return GetBlurredMountainStrength( i.uv );
				//return GetMountainStrength( i.uv );
				//return GetSteppedStrength( i.uv );
				//return GetAverageStrength( i.uv );

			}
			ENDCG
		}
	}
}
