using UnityEngine;
using System.Collections;
using System.Collections.Generic;



//	this is kind of like a swizzle... 
public static class PopColor
{
	public static Color WithAlpha(this Color rgba, float Alpha) 
	{
		return new Color(rgba.r, rgba.g, rgba.b, Alpha); 
	}

	public static Vector4 GetVector4(this Color rgba) 
	{
		return new Vector4(rgba.r, rgba.g, rgba.b, rgba.a); 
	}

	//	converts a normalised float from red(0) to green(1). Blue if OOB

	public static Color NormalToRedGreenClamped(float Normal)
	{
		return NormalToRedGreen(Normal, null);
	}

	public static Color NormalToRedGreen(float Normal)
	{
		return NormalToRedGreen(Normal, Color.blue);
	}

	public static Color NormalToRedGreen(float Normal, Color? OOBColour)
	{
		if ( Normal < 0.0f && OOBColour.HasValue )
		{
			return OOBColour.Value;
		}
		else if ( Normal < 0.5f )
		{
			Normal = Mathf.Max(Normal, 0);
			Normal /= 0.5f;
			return new Color( 1, Normal, 0);
		}
		else if ( Normal > 1 && OOBColour.HasValue )
		{
			return OOBColour.Value;
		}
		else // >= 0.5f
		{
			Normal = Mathf.Min(Normal, 1);

			float Yellow = PopMath.Range(1.0f, 0.5f, Normal);
			return new Color(Yellow, 1, 0);
		}
	}

}


/*
 * Swizzling extensions for vectors
 */
public static class PopFloat
{
	public static Vector2 xx(this float f) { return new Vector2(f, f); }
	public static Vector3 xxx(this float f) { return new Vector3(f, f, f); }
	public static Vector4 xxxx(this float f) { return new Vector4(f, f, f, f); }
}


public static class PopVector3
{
	public static Vector2 xy(this Vector3 three)			{	return new Vector2(three.x, three.y);	}
	public static Vector2 xz(this Vector3 three)			{	return new Vector2(three.x, three.z);	}
	public static Vector2 yx(this Vector3 three)			{	return new Vector2(three.y, three.x);	}
	public static Vector2 yz(this Vector3 three)			{	return new Vector2(three.y, three.z);	}
	public static Vector2 zx(this Vector3 three)			{	return new Vector2(three.z, three.x);	}
	public static Vector2 zy(this Vector3 three)			{	return new Vector2(three.z, three.y);	}

	public static Vector3 xzy(this Vector3 three)			{	return new Vector3(three.x, three.z, three.y); }

	public static Vector4 xyz0(this Vector3 three)			{	return new Vector4(three.x, three.y, three.z, 0);	}
	public static Vector4 xyz1(this Vector3 three)			{	return new Vector4(three.x, three.y, three.z, 1); }
	public static Vector4 xyzw(this Vector3 three,float w)	{	return new Vector4(three.x, three.y, three.z, w); }


	//	returns 0-1 for each component inside a min/mac
	public static Vector3 Range(this Vector3 Value,Vector3 Min, Vector3 Max)
	{
		var x = PopMath.Range(Min.x, Max.x, Value.x);
		var y = PopMath.Range(Min.y, Max.y, Value.y);
		var z = PopMath.Range(Min.z, Max.z, Value.z);
		return new Vector3(x, y, z);
	}
}

public static class PopVector2
{
	public static Vector2 yx(this Vector2 two) { return new Vector2(two.y, two.x); }
	public static Vector3 xy0(this Vector2 two) { return new Vector3(two.x, two.y, 0); }
	public static Vector4 xy00(this Vector2 two) { return new Vector4(two.x, two.y, 0,0); }
	public static Vector4 xy01(this Vector2 two) { return new Vector4(two.x, two.y, 0,1); }
	public static Vector3 xyz(this Vector2 two, float z) { return new Vector3(two.x, two.y, z); }
	public static Vector4 xyzw(this Vector2 two, float z, float w) { return new Vector4(two.x, two.y, z, w); }
	public static Vector3 xzy(this Vector2 two, float z) { return new Vector3(two.x, z, two.y); }

	//	returns 0-1 for each component inside a min/mac
	public static Vector2 Range(this Vector2 Value, Vector2 Min, Vector2 Max)
	{
		var x = PopMath.Range(Min.x, Max.x, Value.x);
		var y = PopMath.Range(Min.y, Max.y, Value.y);
		return new Vector2(x, y);
	}
}


public static class PopVector4
{
	public static Vector2 xy(this Vector4 three) { return new Vector2(three.x, three.y); }
	public static Vector2 xz(this Vector4 three) { return new Vector2(three.x, three.z); }
	public static Vector2 yx(this Vector4 three) { return new Vector2(three.y, three.x); }
	public static Vector2 yz(this Vector4 three) { return new Vector2(three.y, three.z); }
	public static Vector2 zx(this Vector4 three) { return new Vector2(three.z, three.x); }
	public static Vector2 zy(this Vector4 three) { return new Vector2(three.z, three.y); }

	//	returns 0-1 for each component inside a min/mac
	public static Vector4 Range(this Vector4 Value, Vector4 Min, Vector4 Max)
	{
		var x = PopMath.Range(Min.x, Max.x, Value.x);
		var y = PopMath.Range(Min.y, Max.y, Value.y);
		var z = PopMath.Range(Min.z, Max.z, Value.z);
		var w = PopMath.Range(Min.w, Max.w, Value.w);
		return new Vector4(x, y, z, w );
	}
}
