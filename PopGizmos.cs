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
	}
}
