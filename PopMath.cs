using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public struct Line3
{
	public Vector2	Start;
	public Vector2	End;

	public Line3(Vector2 _Start, Vector2 _End)
	{ 
		Start = _Start;
		End = _End;
	}

};

[System.Serializable]
public class UnityEvent_Line3 : UnityEngine.Events.UnityEvent <Line3> {}


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


	//	to match PopCommon.cginc
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

	static void	MakeMatrix(ref Matrix4x4 Mtx,float m00,float m10,float m20,float m01,float m11,float m21,float m02,float m12,float m22)
	{
		if ( Mtx == null )
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
				MakeMatrix( ref CUBEMAP_UVTRANSFORMS[(int)CubemapFaceIndex.CUBEMAP_UP],			-1,0,0,	0,0,1,	0,1,0 );
				MakeMatrix( ref CUBEMAP_UVTRANSFORMS[(int)CubemapFaceIndex.CUBEMAP_DOWN],		-1,0,0,	0,0,-1,	0,-1,0 );
				MakeMatrix( ref CUBEMAP_UVTRANSFORMS[(int)CubemapFaceIndex.CUBEMAP_FORWARD],	1,0,0,	0,1,0,	0,0,1 );
				MakeMatrix( ref CUBEMAP_UVTRANSFORMS[(int)CubemapFaceIndex.CUBEMAP_BACKWARD],	-1,0,0,	0,1,0,	0,0,-1 );
				MakeMatrix( ref CUBEMAP_UVTRANSFORMS[(int)CubemapFaceIndex.CUBEMAP_LEFT],		0,0,-1,	0,1,0,	1,0,0 );
				MakeMatrix( ref CUBEMAP_UVTRANSFORMS[(int)CubemapFaceIndex.CUBEMAP_RIGHT],		0,0,1,	0,1,0,	-1,0,0 );
			}
			return _CUBEMAP_UVTRANSFORMS;
		}
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

}
