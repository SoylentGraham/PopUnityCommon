using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace PopX
{
	public static class Gizmos
	{
		static public void DrawCross(Vector3 Position,Vector3 Size)
		{
			var Up = Vector3.up * Size.y * 0.5f;
			var Left = Vector3.left * Size.x * 0.5f;
			var Forward = Vector3.forward * Size.z * 0.5f;

			UnityEngine.Gizmos.DrawLine(Position + Up, Position - Up);
			UnityEngine.Gizmos.DrawLine(Position + Left, Position - Left);
			UnityEngine.Gizmos.DrawLine(Position + Forward, Position - Forward);
		}

		static public void DrawWireArrow(Vector3 Start,Vector3 End,float ArrowHeadSize)
		{
			//	draw a line
			UnityEngine.Gizmos.DrawLine(Start, End);

			//	todo: draw a cone instead of a sphere :)
			UnityEngine.Gizmos.DrawWireSphere(End, ArrowHeadSize);
		}

		//	todo: better plane selection
		static public void DrawCircleXZ(Vector3 Center,float Radius,int Segments=30)
		{
			Segments = Mathf.Max(3, Segments);
			System.Func<float,Vector3> GetCirclePosition = (Time) =>
			{
				var Pos = Center;
				Pos.x += Mathf.Sin(Mathf.Deg2Rad * 360.0f * Time) * Radius;
				Pos.z += Mathf.Cos(Mathf.Deg2Rad * 360.0f * Time) * Radius;
				return Pos;
			};

			for (int i = 0; i < Segments; i++)
			{
				float t0 = (i + 0) / ((float)Segments - 1);
				float t1 = (i + 1) / ((float)Segments - 1);
				var Pos0 = GetCirclePosition(t0);
				var Pos1 = GetCirclePosition(t1);
				UnityEngine.Gizmos.DrawLine(Pos0, Pos1);
			}

		}

		static public void DrawWireRingSphere(Vector3 Center,float Radius,int Segments=30)
		{
			//	gr: the vertical lines are nice too, this also verifies use of radius
			UnityEngine.Gizmos.DrawWireSphere (Center, Radius);

			for (var Ring = 0; Ring < Segments;	Ring++ )
			{
				var t = Ring / (float)Segments;
				var RadiusTheta = Mathf.Lerp(0 * Mathf.Deg2Rad, 180 * Mathf.Deg2Rad,t);
				var RingRadius = Mathf.Sin(RadiusTheta);
				var RingPos = Center;

				var YTheta = Mathf.Lerp(0 * Mathf.Deg2Rad, 180 * Mathf.Deg2Rad,t);
				var YOffset = Mathf.Cos(YTheta);

				RingPos.y = Center.y + (YOffset*Radius);

				DrawCircleXZ(RingPos, RingRadius * Radius, Segments);
			}
			
		}
	}
}
