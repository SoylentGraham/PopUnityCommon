using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Line3
{
	public Vector3	Start;
	public Vector3	End;

	public Line3(Vector3 _Start, Vector3 _End)
	{ 
		Start = _Start;
		End = _End;
	}

};

[System.Serializable]
public class UnityEvent_ListOfLine3 : UnityEngine.Events.UnityEvent <List<Line3>> {}


public class PopMath {

	public static Rect RectToScreen(Rect RectNorm)
	{
		return RectMult (RectNorm, new Rect (0, 0, Screen.width, Screen.height));
	}
	
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

	public static float Range(float Min,float Max,float Value)
	{
		Value -= Min;
		return Value / ( Max - Min);
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

}

//	gr: this should be somewhere else
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


	public static Mesh	GetPrimitiveMesh(PrimitiveType type)
	{
		return GameObject.CreatePrimitive( type ).GetComponent<MeshFilter>().sharedMesh;
	}


	public static void AllocIfNull<T>(ref T Object) where T : new()
	{
		if ( Object != null )
			return;

		Object = new T();
	}


	public static Material	GetMaterial(GameObject Object,bool SharedMaterial)
	{
		try {
			var Renderer = Object.GetComponent<MeshRenderer> ();
			var mat = SharedMaterial ? Renderer.sharedMaterial : Renderer.material;
		} catch {
		}

		try {
			var Renderer = Object.GetComponent<SpriteRenderer> ();
			var mat = SharedMaterial ? Renderer.sharedMaterial : Renderer.material;
		} catch {
		}

		return null;
	}
}


public class ScopedComputeBuffer
{
	public ComputeBuffer	Buffer;

	~ScopedComputeBuffer()
	{
		Buffer.Release();
	} 

	public static implicit operator ComputeBuffer(ScopedComputeBuffer scb)
	{
		return scb.Buffer;
    }
	
	//	mapping
	public ScopedComputeBuffer(int Count,int Stride)
	{
		Buffer = new ComputeBuffer( Count, Stride );
	}

	public ScopedComputeBuffer(int Count,int Stride,ComputeBufferType type)
	{
		Buffer = new ComputeBuffer( Count, Stride, type );
	}

	public void SetData(System.Array Data)
	{
		Buffer.SetData( Data );
	}

	public void GetData(System.Array Data)
	{
		Buffer.GetData( Data );
	}

	public void SetCounterValue(uint counterValue)
	{
		Buffer.SetCounterValue( counterValue );
	}
}


