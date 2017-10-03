//	need to add this to your shader. Does nothing in .cginc
#pragma geometry geom

#define MAX_SPLINE_POINTS	20
#define MAX_SUBDIVISIONS	22
           
float3 WorldPositions[MAX_SPLINE_POINTS];
float WorldPositionsCount;
float Radius;
float SkewBlur_Square;
float SkewBlur_SquareInverse;
float SkewBlur_SquareSquareInverse;


void JoinEdges(v2f FrontA,v2f FrontB,v2f BackA,v2f BackB,inout TriangleStream<v2f> OutputStream)
{
	/*
	OutputStream.Append( FrontA );
	OutputStream.Append( BackA );
	OutputStream.Append( BackB );

	OutputStream.RestartStrip();

	OutputStream.Append( BackB );
	OutputStream.Append( FrontB );
	OutputStream.Append( FrontA );

	OutputStream.RestartStrip();
	*/
	OutputStream.Append( FrontA );
	OutputStream.Append( FrontB );
	OutputStream.Append( BackA );
	OutputStream.Append( BackB );

	OutputStream.RestartStrip();

}



v2f Pos2ToClipSpace(v2f Vert)
{
	float z = 0;
/*
	float4 MtxTranslation = mul(unity_ObjectToWorld,float4(0,0,0,1));
	MtxTranslation.w = 0;

	Vert.Position.zw = float2( z, 1 );
	Vert.Position = mul(unity_ObjectToWorld,Vert.Position);
	Vert.Position = mul(UNITY_MATRIX_VP,Vert.Position);
	*/
	Vert.Position.zw = float2( z, 1 );
	Vert.Position = mul(UNITY_MATRIX_VP,Vert.Position);
	return Vert;
}

float Range(float Min,float Max,float Value)
{
	return (Value-Min) / (Max-Min);
}

//	http://catlikecoding.com/unity/tutorials/curves-and-splines/
float3 GetBezierFirstDerivative(float3 Start,float3 Middle,float3 End,float Time)
{
	//Time = clamp(0,1,Time);

	float t = Time;
	float3 p0 = Start;
	float3 p1 = Middle;
	float3 p2 = End;

	return 2.0f * (1.0f - t) * (p1 - p0) + 2.0f * t * (p2 - p1);
}

float3 GetBezierPoint(float3 Start,float3 Middle,float3 End,float Time)
{
	//Time = clamp(0,1,Time);
	/*
	//	linear test
	if ( Time <= 0.5f )
	{
		return lerp ( Start, Middle, Range( 0, 0.5f, Time ) );
	}
	else
	{
		return lerp ( Middle, End, Range( 0.5f, 1.0f, Time ) );
	}           			
	*/
	float t = Time;
	float3 p0 = Start;
	float3 p1 = Middle;
	float3 p2 = End;

	float oneMinusT = 1.0f - t;
	return oneMinusT * oneMinusT * p0 + 2.0f * oneMinusT * t * p1 + t * t * p2;
}

float3 GetWorldMatrixPosition(int Index)
{
	//return mul( WorldPositions[Index], float4(0,0,0,1) );
	return WorldPositions[Index];
}

float3 GetWorldPosition(float Time,out float SubTime,out float3 Direction)
{
	Time = clamp(0,1,Time);
	       
	/*
	//	linear
	SubTime = fmod( Time, 1.0f );
	return lerp( WorldPositions[0], WorldPositions[1], SubTime );
	*/

	/*
	//	linear between correct 2 points
	Time *= WorldPositionsCount;
	Time = clamp(0,WorldPositionsCount,Time);
	SubTime = fmod( Time, 1.0f );
	int Index = floor( Time );
	return lerp( WorldPositions[Index], WorldPositions[Index+1], SubTime );
	*/


	Time *= WorldPositionsCount;
	int Index;
	if ( fmod( Time, 1.0f ) < 0.5f )
	{
		Index = floor( Time );
	}
	else
	{
		Index = floor(Time)+1;
	}
	Index = max( 1, Index );
	Index = min( WorldPositionsCount-2, Index );


	int Indexa = Index-1;
	int Indexb = Index+0;
	int Indexc = Index+1;
	SubTime = Range( Indexa, Indexc, Time );

	float3 Posa = GetWorldMatrixPosition(Indexa);
	float3 Posb = GetWorldMatrixPosition(Indexb);
	float3 Posc = GetWorldMatrixPosition(Indexc);

	float3 Pos = GetBezierPoint( Posa, Posb, Posc, SubTime );

	Direction = GetBezierFirstDerivative( Posa, Posb, Posc, SubTime );

	Direction = normalize(Direction);
	//SubTime = fmod( Time, 1.0f );
	return Pos;
	//float SubTime = fmod( Time, 1.0f );
	//return lerp( WorldOffset[a], WorldOffset[b], SubTime );

}


//	gr: skew this based on velocity/how much the spline has been stretched from the original/min length
float GetSkewedUTime(float Time)
{
	bool InverseSquare =  SkewBlur_SquareInverse>0.5f;
	bool InverseSquareSquare =  SkewBlur_SquareSquareInverse>0.5f;
	bool Square = SkewBlur_Square>0.5f;

	if ( InverseSquareSquare )
	{
		Time = 1 - Time;
		Time *= Time;
		Time *= Time;
		Time = 1 - Time;
	}
	else if ( InverseSquare )
	{
		Time = 1 - Time;
		Time *= Time;
		Time = 1 - Time;
	}
	else if ( Square )
	{
		Time *= Time;
	}

	return Time;
}

//	v2, ignore the shape. Lots of wasted triangles. Just make stuff dynamically from center point
[maxvertexcount(120)]
void geom(triangle v2f input[3], inout TriangleStream<v2f> OutputStream)
{
	int Parts = MAX_SUBDIVISIONS;
	//Parts = min( SubDivisons, MAX_SUBDIVISIONS );

	float3 Part_Positions[MAX_SUBDIVISIONS];
	float3 Part_Directions[MAX_SUBDIVISIONS];
	float Part_Times[MAX_SUBDIVISIONS];
	float Part_SplineTimes[MAX_SUBDIVISIONS];

 	for ( int p=0;	p<MAX_SUBDIVISIONS;	p++ )
 	{
   	 	Part_Times[p] = p / (float)(Parts-1);
 		Part_Positions[p] = GetWorldPosition( Part_Times[p], Part_SplineTimes[p], Part_Directions[p] );
 	}

 	//v2f PartExtrusions[MAX_SUBDIVISIONS*2];

 	for ( int n=0;	n<MAX_SUBDIVISIONS;	n++ )
 	{
 		float Next_t = GetSkewedUTime( Part_Times[n] );
 		float3 NextPos = Part_Positions[n];
		float2 Dir = Part_Directions[n];

		float2 Left = float2( -Dir.y, Dir.x );
		float2 Right = -Left;

		v2f Vert[2];
		Vert[0] = input[0];
		Vert[1] = input[1];

		Vert[0].uv.x = Next_t;
		Vert[1].uv.x = Next_t;
		Vert[0].uv.y = 0;
		Vert[1].uv.y = 1;
		Vert[0].Position.xy = NextPos + (Left*Radius);
		Vert[1].Position.xy = NextPos + (Right*Radius);

		//PartExtrusions[(n*2)+0] = Vert[0];
		//PartExtrusions[(n*2)+1] = Vert[1];
		OutputStream.Append( Pos2ToClipSpace(Vert[0]) );
		OutputStream.Append( Pos2ToClipSpace(Vert[1]) );
	}

	/*
	//	one big tristrip
	for ( int i=0;	i<Parts*2;	i++ )
 	{
 		OutputStream.Append( Pos2ToClipSpace(PartExtrusions[i]) );
	}
	*/
	OutputStream.RestartStrip();



}
