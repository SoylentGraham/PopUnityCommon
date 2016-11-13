
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

bool PointInTriangle_Direct(float2 pt, float2 v1, float2 v2, float2 v3)
{
	bool b1 = sign(pt, v1, v2) < 0.0f;
	bool b2 = sign(pt, v2, v3) < 0.0f;
	bool b3 = sign(pt, v3, v1) < 0.0f;

    return ((b1 == b2) && (b2 == b3));
}


//	glcore no longer allows intrinsic cross(float2)
//	rewrite GetTriangleBarycentric to not use cross at all to simplify code
float2 Cross2(float2 a2,float2 b2)
{
	float3 a3 = float3( a2.x, a2.y, 0 );
	float3 b3 = float3( b2.x, b2.y, 0 );

	float3 c3 = cross( a3, b3 );
	return c3.xy;
}

//	http://gamedev.stackexchange.com/a/23745
float3 GetTriangleBarycentric(float2 Point,float2 p1,float2 p2,float2 p3)
{
	float2 a = p1;
	float2 b = p2;
	float2 c = p3;
	float2 p = Point;
	float2 v0 = b - a;
	float2 v1 = c - a;
	float2 v2 = p - a;
	float d00 = dot(v0, v0);
	float d01 = dot(v0, v1);
	float d11 = dot(v1, v1);
	float d20 = dot(v2, v0);
	float d21 = dot(v2, v1);
	float denom = d00 * d11 - d01 * d01;
	float v = (d11 * d20 - d01 * d21) / denom;
	float w = (d00 * d21 - d01 * d20) / denom;
	float u = 1.0 - v - w;
	return float3(u,v,w);
}

float2 GetTriangleBarycentric2(float2 Point,float2 p1,float2 p2,float2 p3,float2 a,float2 b,float2 c)
{
	float3 Bary = GetTriangleBarycentric( Point, p1, p2, p3 );
	return (a * Bary.x) + (b * Bary.y) + (c * Bary.z);
}

float3 GetTriangleBarycentric3(float2 Point,float2 p1,float2 p2,float2 p3,float3 a,float3 b,float3 c)
{
	float3 Bary = GetTriangleBarycentric( Point, p1, p2, p3 );
	return (a * Bary.x) + (b * Bary.y) + (c * Bary.z);
}

float4 GetTriangleBarycentric3(float2 Point,float2 p1,float2 p2,float2 p3,float4 a,float4 b,float4 c)
{
	float3 Bary = GetTriangleBarycentric( Point, p1, p2, p3 );
	return (a * Bary.x) + (b * Bary.y) + (c * Bary.z);
}

bool PointInsideTriangle(float2 p,float2 t0,float2 t1,float2 t2)
{
	return PointInTriangle_Direct(p,t0,t1,t2);
	float2 uv = GetTriangleBarycentric2( p, t0, t1, t2, float2(0,0), float2(1,0), float2(1,1) );

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

float3 LatLonToView(float2 LatLon)
{
	//	http://en.wikipedia.org/wiki/N-vector#Converting_latitude.2Flongitude_to_n-vector
	float latitude = LatLon.x;
	float longitude = LatLon.y;
	float las = sin(latitude);
	float lac = cos(latitude);
	float los = sin(longitude);
	float loc = cos(longitude);

	return float3( los * lac, las, loc * lac );
}


