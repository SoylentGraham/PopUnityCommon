using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class int2
{
	public int x, y;

	public int2(int _x, int _y)
	{
		x = _x;
		y = _y;
	}
};


public class int3
{
	public int x, y,z;

	public int3(int _x, int _y,int _z)
	{
		x = _x;
		y = _y;
		z = _z;
	}
};


//	move to this please!
namespace PopX
{
	public static class Math
	{
		//	if the rotation is invalid, return identity
		//	catch invalid quaternions :/
		//	annoyingly even if we turn assertions into exceptions, we still get the message printed out and clogs up the console.
		//UnityEngine.Assertions.Assert.raiseExceptions = true;
		static public Quaternion GetSafeRotation(Quaternion Rotation)
		{
			var Rot4 = new Vector4(Rotation.x, Rotation.y, Rotation.z, Rotation.w);

			if (Rot4.sqrMagnitude < float.Epsilon)
				return Quaternion.identity;

			return Rotation;
		}

	}
}

public static class PopMath {

	//	Rect(struct) is pass by value, must return.
	public static Rect ClipToParent(this Rect Child,Rect Parent)
	{
		Child.xMin = Mathf.Clamp(Child.xMin, Parent.xMin, Parent.xMax);
		Child.yMin = Mathf.Clamp(Child.yMin, Parent.yMin, Parent.yMax);
		Child.xMax = Mathf.Clamp(Child.xMax, Parent.xMin, Parent.xMax);
		Child.yMax = Mathf.Clamp(Child.yMax, Parent.yMin, Parent.yMax);
		return Child;
	}

	public static Rect RectToScreen(Rect RectNorm)
	{
		return RectMult (RectNorm, new Rect (0, 0, Screen.width, Screen.height));
	}

	//	fit a normalised rect into a parent rect
	public static Rect RectMult(Rect RectNorm,Rect Body)
	{
		RectNorm.x *= Body.width;
		RectNorm.width *= Body.width;
		RectNorm.y *= Body.height;
		RectNorm.height *= Body.height;
		RectNorm.x += Body.x;
		RectNorm.y += Body.y;
		return RectNorm;
	}

	//	Rect = Rect.Scale(f)
	public static Rect Scale(this Rect rect,float Scale)
	{
		var Offsetx = (rect.width * Scale) - rect.width;
		var Offsety = (rect.height * Scale) - rect.height;
		rect.x -= Offsetx / 2.0f;
		rect.y -= Offsety / 2.0f;
		rect.width *= Scale;
		rect.height *= Scale;
		return rect;
	}

	public static float Range(float Min, float Max, float Value)
	{
		return (Value - Min) / (Max - Min);
	}

	public static float Range01Clamped(float Min, float Max, float Value)
	{
		return Mathf.Clamp01((Value - Min) / (Max - Min));
	}

	public static Vector2 Range(Vector2 Min, Vector2 Max, Vector2 Value)
	{
		var Out = new Vector2();
		Out.x = Range(Min.x, Max.x, Value.x);
		Out.y = Range(Min.y, Max.y, Value.y);
		return Out;
	}

	public static Vector3 Range(Vector3 Min, Vector3 Max, Vector3 Value)
	{
		var Out = new Vector3();
		Out.x = Range(Min.x, Max.x, Value.x);
		Out.y = Range(Min.y, Max.y, Value.y);
		Out.z = Range(Min.z, Max.z, Value.z);
		return Out;
	}

	public static Rect GetTextureRect(Texture Tex)
	{
		if ( !Tex )
			return new Rect(0,0,0,0);

		return new Rect( 0, 0, Tex.width, Tex.height );
	}

	public static Rect Round(Rect r)
	{
		r.x = Mathf.Round (r.x);
		r.y = Mathf.Round (r.y);
		r.width = Mathf.Round (r.width);
		r.height = Mathf.Round (r.height);
		return r;
	}

	public static Rect ViewRectToTextureRect(Rect ViewRect,Texture Tex)
	{
		return new Rect (ViewRect.x, Tex.height-ViewRect.yMax, ViewRect.width, ViewRect.height);
	}

	public static float GetTriangleArea(Vector2 t0,Vector2 t1,Vector2 t2)
	{
		var a = Vector2.Distance(t0,t1);
		var b = Vector2.Distance(t1,t2);
		var c = Vector2.Distance(t2,t0);
		var s = (a + b + c) / 2;
		return Mathf.Sqrt(s * (s-a) * (s-b) * (s-c));
	}


	//	to match PopCommon.cginc
	//	gr: replace with UnityEngine.CubemapFace for c# purposes
	public enum CubemapFaceIndex
	{
		CUBEMAP_UP			= 0,
		CUBEMAP_FORWARD		= 1,
		CUBEMAP_LEFT		= 2,
		CUBEMAP_BACKWARD	= 3,
		CUBEMAP_RIGHT		= 4,
		CUBEMAP_DOWN		= 5,
		CUBEMAP_FACECOUNT	= 6,
	};

	static void	MakeMatrix3x3(ref Matrix4x4 Mtx,float m00,float m10,float m20,float m01,float m11,float m21,float m02,float m12,float m22)
	{
		Mtx = new Matrix4x4();
		Mtx.m00 = m00;
		Mtx.m10 = m10;
		Mtx.m20 = m20;
		Mtx.m01 = m01;
		Mtx.m11 = m11;
		Mtx.m21 = m21;
		Mtx.m02 = m02;
		Mtx.m12 = m12;
		Mtx.m22 = m22;
	}

	static Matrix4x4[]			_CUBEMAP_UVTRANSFORMS = null;
	static public Matrix4x4[]	CUBEMAP_UVTRANSFORMS
	{
		get
		{
			if ( _CUBEMAP_UVTRANSFORMS == null )
			{
				//	https://github.com/SoylentGraham/panopo.ly/blob/master/site_upload/cubemap.php#L286
				//	& Cubemap.cginc
				_CUBEMAP_UVTRANSFORMS = new Matrix4x4[(int)CubemapFaceIndex.CUBEMAP_FACECOUNT];
				MakeMatrix3x3( ref CUBEMAP_UVTRANSFORMS[(int)CubemapFaceIndex.CUBEMAP_UP],			-1,0,0,	0,0,1,	0,1,0 );
				MakeMatrix3x3( ref CUBEMAP_UVTRANSFORMS[(int)CubemapFaceIndex.CUBEMAP_DOWN],		-1,0,0,	0,0,-1,	0,-1,0 );
				MakeMatrix3x3( ref CUBEMAP_UVTRANSFORMS[(int)CubemapFaceIndex.CUBEMAP_FORWARD],		1,0,0,	0,1,0,	0,0,1 );
				MakeMatrix3x3( ref CUBEMAP_UVTRANSFORMS[(int)CubemapFaceIndex.CUBEMAP_BACKWARD],	-1,0,0,	0,1,0,	0,0,-1 );
				MakeMatrix3x3( ref CUBEMAP_UVTRANSFORMS[(int)CubemapFaceIndex.CUBEMAP_LEFT],		0,0,-1,	0,1,0,	1,0,0 );
				MakeMatrix3x3( ref CUBEMAP_UVTRANSFORMS[(int)CubemapFaceIndex.CUBEMAP_RIGHT],		0,0,1,	0,1,0,	-1,0,0 );
			}
			return _CUBEMAP_UVTRANSFORMS;
		}
	}

	public static Vector3 CubemapUvToView(Vector2 Uv, CubemapFaceIndex CubemapFace)
	{
		Uv.x -= 0.5f;
		Uv.x *= 2.0f;
		Uv.y -= 0.5f;
		Uv.y *= 2.0f;
		Vector3 Uvw = new Vector3(Uv.x, Uv.y, 1.0f );
		var Mtx = CUBEMAP_UVTRANSFORMS[(int)CubemapFace];
		return Mtx.MultiplyPoint3x4( Uvw );
	}



	public static Vector3[] GetBoundsCorners(Bounds Box)
	{
		var Corners = new Vector3[8];
		Corners[0] = Box.center + new Vector3 (Box.min.x, Box.min.y, Box.min.z);
		Corners[1] = Box.center + new Vector3 (Box.min.x, Box.min.y, Box.max.z);
		Corners[2] = Box.center + new Vector3 (Box.min.x, Box.max.y, Box.min.z);
		Corners[3] = Box.center + new Vector3 (Box.min.x, Box.max.y, Box.max.z);
		Corners[4] = Box.center + new Vector3 (Box.max.x, Box.min.y, Box.min.z);
		Corners[5] = Box.center + new Vector3 (Box.max.x, Box.min.y, Box.max.z);
		Corners[6] = Box.center + new Vector3 (Box.max.x, Box.max.y, Box.min.z);
		Corners[7] = Box.center + new Vector3 (Box.max.x, Box.max.y, Box.max.z);
		return Corners;
	}

	public static bool IsBoundsNeighbours(Bounds a,Bounds b,float MaxDistance=0.001f)
	{
		//	hacky, probably not that accurate... especially if corners are not near. probably needs a plane/plane distance test
		var Corners = GetBoundsCorners(b);
		var MinDistance = 99999.0f;
		foreach (var Corner in Corners) {
			var Distance = a.SqrDistance (Corner);
			MinDistance = Mathf.Min (Distance, MinDistance);
		}

		MinDistance = Mathf.Sqrt (MinDistance);

		//	swap magic number for say... 0.001f * volume/largest extent
		return MinDistance < MaxDistance;
	}

	public static float Vector3_DistanceSquared(Vector3 a,Vector3 b)
	{
		//	gr: this causes a constructor! not an alloc, but still more expensive :/
		//a -= b;
		a.x -= b.x;
		a.y -= b.y;
		a.z -= b.z;
		return a.sqrMagnitude;
	}

	public static Vector2 AngleRadianToVector2(float radian,float Length=1)
	{
		return new Vector2(Mathf.Sin(radian)* Length, Mathf.Cos(radian) * Length);
	}

	public static Vector2 AngleDegreeToVector2(float degree,float Length=1)
	{
		return AngleRadianToVector2(degree * Mathf.Deg2Rad, Length);
	}


	public static float AngleDegreeFromVector2(Vector2 Direction)
	{
		Direction.Normalize ();
		var Angle = Mathf.Atan2 (Direction.x, Direction.y) * Mathf.Rad2Deg;
		return Angle;
	}

	public static float AngleDegreeFromVector2(Vector2 From,Vector2 To)
	{
		return AngleDegreeFromVector2 (To - From);
	}

	public static float MakeAngleRelative(float Angle)
	{
		while (Angle < -180)
			Angle += 360;
		while (Angle > 180)
			Angle -= 360;
		return Angle;
	}



	//	gr: from PopCommon.cginc, from ...
	//	gr: from the tootle engine :) https://github.com/TootleGames/Tootle/blob/master/Code/TootleMaths/TLine.cpp
	public static Vector3 NearestToRay3(Vector3 Position,Ray Ray)
	{
		var Start = Ray.origin;
		var Direction = Ray.direction;

		Vector3 LineDir = Direction;
		float LineDirDotProduct = Vector3.Dot( LineDir, LineDir );

		//	avoid div by zero
		//	gr: this means the line has no length.... for shaders maybe we can fudge/allow this
		if ( LineDirDotProduct == 0 )
			return Start;

		var Dist = Position - Start;

		float LineDirDotProductDist = Vector3.Dot( LineDir, Dist );

		float TimeAlongLine = LineDirDotProductDist / LineDirDotProduct;

		//	gr: for line segment
		/* 	if ( TimeAlongLine <= 0.f ) 		return Start;  	if ( TimeAlongLine >= 1.f ) 		return GetEnd(); 	*/
		//	gr: lerp this for gpu speedup
		return Start + (LineDir * TimeAlongLine);
	}

	public static PopX.Sphere3 GetColliderSphere(Collider collider)
	{
		if (collider is SphereCollider) {
			var sc = collider as SphereCollider;
			return new PopX.Sphere3 (sc.center, sc.radius);
		}

		if (collider is BoxCollider) {
			var bc = collider as BoxCollider;
			var sc = new PopX.Sphere3 ();
			sc.center = bc.center;

			//	gr: this is VERY innacurate. use some accumulation instead;
			//	https://github.com/TootleGames/Tootle/blob/master/Code/TootleMaths/TSphere.cpp
			sc.radius = bc.size.x;
			sc.radius = Mathf.Max (sc.radius, bc.size.y);
			sc.radius = Mathf.Max (sc.radius, bc.size.z);
			return sc;
		}

		throw new System.Exception ("Unhandled " + collider.GetType() + " -> SphereCollider conversion");
	}

	public static PopX.Sphere3 GetColliderWorldSphere(Collider collider)
	{
		var ct = collider.transform;
		var Transform = ct.localToWorldMatrix;
		return GetColliderWorldSphere(collider, Transform);
	}

	public static PopX.Sphere3 GetColliderWorldSphere(Collider collider,Matrix4x4 Transform)
	{
		//	grab local one
		var LocalSphere = GetColliderSphere(collider);

		var EdgePos = LocalSphere.center + new Vector3(0, 0, LocalSphere.radius);

		//	transform
		LocalSphere.center = Transform.MultiplyPoint(LocalSphere.center);
		var WorldEdgePos = Transform.MultiplyPoint(EdgePos);

		LocalSphere.radius = Vector3.Distance(WorldEdgePos, LocalSphere.center);

		return LocalSphere;
	}

	public static Ray WorldRayToLocalRay(Ray WorldRay,Transform trans)
	{
		var LocalOrigin = trans.InverseTransformPoint (WorldRay.origin);
		var LocalDir = trans.InverseTransformDirection (WorldRay.direction);
		return new Ray (LocalOrigin, LocalDir);
	}	

	public static float Min(float a,float b,float c)
	{
		return Mathf.Min(a, Mathf.Min(b, c));
	}

	public static float Max(float a,float b,float c)
	{
		return Mathf.Max(a, Mathf.Max(b, c));
	}

	//	shader-style swizzle min() where x is min x, y is min y, z is min z
	public static Vector2 Min(Vector2 a, Vector2 b)
	{
		return new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
	}

	public static Vector2 Min(Vector2 a, Vector2 b, Vector2 c)
	{
		return new Vector2(Min(a.x, b.x, c.x), Min(a.y, b.y, c.y) );
	}

	public static Vector3 Min(Vector3 a, Vector3 b)
	{
		return new Vector3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z));
	}

	public static Vector3 Min(Vector3 a, Vector3 b, Vector3 c)
	{
		return new Vector3(Min(a.x, b.x, c.x), Min(a.y, b.y, c.y), Min(a.z, b.z,c.z));
	}

	public static Vector3 Max(Vector3 a, Vector3 b)
	{
		return new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z));
	}

	public static Vector3 Max(Vector3 a, Vector3 b, Vector3 c)
	{
		return new Vector3(Max(a.x, b.x,c.x), Max(a.y, b.y,c.y), Max(a.z, b.z,c.z));
	}

}

//	gr: move this to namespace and System or Alloc static class
public class Pop
{ 
	public static int sizeofElement<T>(T[] Array)
	{
		return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
	}

	public static int sizeofElement<T>(List<T> Array)
	{
		return System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
	}


	public static void AllocIfNull<T>(ref T Object) where T : new()
	{
		if ( Object != null )
			return;

		Object = new T();
	}


}

