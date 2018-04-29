using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
	Nuke "chan" file format exporter, which is for paths. (and typically I believe... camera paths)

	an importer would be easy, but who knows what the ideal format would be. 
	Animation asset I guess :)
*/

//	rename namespace to Pop in later refactor
namespace PopX
{
	public static class NukeChan
	{
		public const string FileExtension = "chan";

		//	perhaps a matrix would be better than a custom struct
		public struct Frame
		{
			public Vector3 Position;
			public Quaternion Rotation;
			public float? CameraFov;
		};

		public static void Export(System.Action<string> WriteLine, System.Func<float, Frame> GetFrameAtTime, float StartTime,float EndTime,float FrameRate)
		{
			if (EndTime < StartTime)
				throw new System.Exception("Trying to export start time (" + StartTime + ") that is after end time (" + EndTime + ")");
			
			var FrameDuration = 1.0f / FrameRate;
			var FrameCountf = (EndTime - StartTime) * FrameRate;
			var FrameCount = (uint)FrameCountf;

			//	get time for frame
			System.Func<uint,float> GetFrameTime = (FrameNumber) =>
			{
				var Delta = FrameNumber * FrameDuration;
				return Delta + StartTime;
			};

			System.Func<uint, Frame> GetFrame = (FrameNumber) =>
			{
				var FrameTime = GetFrameTime(FrameNumber);
				return GetFrameAtTime(FrameTime);
			};

			Export(WriteLine, GetFrame, FrameCount);
		}

		public static void Export(System.Action<string> WriteLine,System.Func<uint,Frame> GetFrame,uint FrameCount)
		{
			for (uint i = 0; i < FrameCount;	i++ )
			{
				var Frame = GetFrame(i);
				var FormatNull = "{0} {1} {2} {3} {4} {5} {6}";
				var FormatCamera = "{0} {1} {2} {3} {4} {5} {6} {7}";
				var Format = Frame.CameraFov.HasValue ? FormatCamera : FormatNull;

				//	gr: can't find documentation on whether the format is expecting radians or degrees :/
				var RotationAngles = Frame.Rotation.eulerAngles;

				var fn = i + 1;
				var x = Frame.Position.x;
				var y = Frame.Position.y;
				var z = Frame.Position.z;

				var pitch = RotationAngles.x;
				var yaw = RotationAngles.y;
				var roll = RotationAngles.z;

				var Fov = Frame.CameraFov.HasValue ? Frame.CameraFov.Value : 0;

				var Line = string.Format(Format, fn, x, y, z, pitch, yaw, roll, Fov);
				WriteLine(Line);
			}
		}

	}
}


