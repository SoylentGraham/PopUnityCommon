using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Swizzling extensions for vectors
 */


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
	public static Vector3 xyz(this Vector2 two, float z) { return new Vector3(two.x, two.y, z); }
	public static Vector4 xyzw(this Vector2 two, float z, float w) { return new Vector4(two.x, two.y, z, w); }

	//	returns 0-1 for each component inside a min/mac
	public static Vector2 Range(this Vector2 Value, Vector2 Min, Vector2 Max)
	{
		var x = PopMath.Range(Min.x, Max.x, Value.x);
		var y = PopMath.Range(Min.y, Max.y, Value.y);
		return new Vector2(x, y);
	}
}

