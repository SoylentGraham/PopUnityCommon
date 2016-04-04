
float max3(float a,float b,float c)
{
	return max( a, max( b,c ) );
}

float min3(float a,float b,float c)
{
	return min( a, min( b,c ) );
}

float3 RgbToHsl(float3 rgb)
{
	float r = rgb.x;
	float g = rgb.y;
	float b = rgb.z;

	float Max = max3( r, g, b );
	float Min = min3( r, g, b );

	float h = 0;
	float s = 0;
	float l = ( Max + Min ) / 2.f;

	if ( Max == Min )
	{
		//	achromatic/grey
        h = s = 0; 
    }
	else
	{
        float d = Max - Min;
        s = l > 0.5f ? d / (2 - Max - Min) : d / (Max + Min);
        if ( Max == r )
		{
            h = (g - b) / d + (g < b ? 6 : 0);
		}
		else if ( Max == g )
		{
            h = (b - r) / d + 2;
        }
		else //if ( Max == b )
		{
			h = (r - g) / d + 4;
		}

        h /= 6;
    }

	return float3( h, s, l );
}



float hue2rgb(float p,float q,float t)
{
    if(t < 0) t += 1.f;
    if(t > 1) t -= 1.f;
    if(t < 1.f/6.f) return p + (q - p) * 6.f * t;
    if(t < 1.f/2.f) return q;
    if(t < 2.f/3.f) return p + (q - p) * (2.f/3.f - t) * 6.f;
    return p;
}


float3 HslToRgb(float3 Hsl)
{
	float h = Hsl.x;
	float s = Hsl.y;
	float l = Hsl.z;

	if(s == 0){
		return float3( l, l, l );
	}else{
		float q = l < 0.5f ? l * (1 + s) : l + s - l * s;
		float p = 2.f * l - q;
		float3 Rgb;
		Rgb.x = hue2rgb(p, q, h + 1.f/3.f);
		Rgb.y = hue2rgb(p, q, h);
		Rgb.z = hue2rgb(p, q, h - 1.f/3.f);
		return Rgb;
	}
}




float4 LerpRgba(float4 a,float4 b,float Time)
{
	float3 hsla = RgbToHsl( a.xyz );
	float3 hslb = RgbToHsl( b.xyz );

	float3 hsl_lerp = lerp( hsla, hslb, Time );
	float3 rgb_lerped = HslToRgb( hsl_lerp );
	return float4( rgb_lerped.x, rgb_lerped.y, rgb_lerped.z, lerp( a.w, b.w, Time ) );
}


float sign (float2 p1, float2 p2, float2 p3)
{
    return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
}

bool PointInTriangle (float2 pt, float2 v1, float2 v2, float2 v3)
{
    bool b1, b2, b3;

    b1 = sign(pt, v1, v2) < 0.0f;
    b2 = sign(pt, v2, v3) < 0.0f;
    b3 = sign(pt, v3, v1) < 0.0f;

    return ((b1 == b2) && (b2 == b3));
}


//	glcore no longer allows intrinsic cross(float2)
//	rewrite GetTriangleBarycentric to not use cross at all to simplify code
float2 Cross2(float2 a2,float2 b2)
{
	float3 a3 = float3( a2.x, a2.x, 0 );
	float3 b3 = float3( b2.x, b2.x, 0 );

	float3 c3 = cross( a3, b3 );
	return c3.xy;
}

float2 GetTriangleBarycentric(float2 Point,float2 p1,float2 p2,float2 p3,float2 uv1,float2 uv2,float2 uv3)
{
	float2 f = Point;
	
	// calculate vectors from point f to vertices p1, p2 and p3:
	float2 f1 = p1-f;
	float2 f2 = p2-f;
	float2 f3 = p3-f;
	

	// calculate the areas and factors (order of parameters doesn't matter):
	float a = length(Cross2(p1-p2, p1-p3)); // main triangle area a
	float a1 = length(Cross2(f2, f3)) / a; // p1's triangle area / a
	float a2 = length(Cross2(f3, f1)) / a; // p2's triangle area / a 
	float a3 = length(Cross2(f1, f2)) / a; // p3's triangle area / a
	// find the uv corresponding to point f (uv1/uv2/uv3 are associated to p1/p2/p3):
	float uv = uv1 * a1 + uv2 * a2 + uv3 * a3;
	return uv;
}

bool PointInsideTriangle(float2 p,float2 t0,float2 t1,float2 t2)
{
	return PointInTriangle(p,t0,t1,t2);
	float2 uv = GetTriangleBarycentric( p, t0, t1, t2, float2(0,0), float2(1,0), float2(1,1) );

	//	Just evaluate s, t and 1-s-t. The point p is inside the triangle if and only if they are all positive.
	
	if ( uv.x >= 0 && uv.y >= 0 && 1-uv.x-uv.y >=0 )
	return true;
	//return uv.x >= 0 && uv.y >= 0 && uv.x <= 1 && uv.y <= 1;
	return false;
}

bool Approximately(float a,float b)
{
	return abs(a-b)<= 0.001f;
}



