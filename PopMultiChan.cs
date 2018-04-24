using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
	Bastardisation of Nuke's Chan format to support multiple objects (and ditched rotation)
	FRAME OBJECTINDEX POSX POSY POSZ [NAME]
*/

//	rename namespace to Pop in later refactor
namespace PopX
{
	public static class MultiChan
	{
		public const string FileExtension = "multichan";

		public struct Frame
		{
			public List<ObjectFrame> Objects;
			public List<string> Comments;

			public void AddObject(Vector3 ObjectPosition, string ObjectName)
			{
				Pop.AllocIfNull(ref Objects);
				var ObjFrame = new ObjectFrame(ObjectPosition, ObjectName);
				Objects.Add(ObjFrame);
			}

			public void AddComment(string Comment)
			{
				Pop.AllocIfNull(ref Comments);
				Comments.Add(Comment);
			}
		}

		public struct ObjectFrame
		{
			public string 	ObjectName;
			public Vector3	Position;
			public float	x { get { return Position.x; } }
			public float	y { get { return Position.y; } }
			public float	z { get { return Position.z; } }

			public ObjectFrame(Vector3 Position,string ObjectName)
			{
				this.ObjectName = ObjectName;
				this.Position = Position;
			}
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
				for (int o = 0; o < Frame.Objects.Count; o++)
				{
					var ObjFrame = Frame.Objects[o];
					var Format = "{0} {1} {2} {3} {4} {5}";
					var fn = i + 1;
					var on = o;
					var x = ObjFrame.x;
					var y = ObjFrame.y;
					var z = ObjFrame.z;
					var Name = ObjFrame.ObjectName;

					var Line = string.Format(Format, fn, on, x, y, z, Name);
					WriteLine(Line);
					/*
					if ( Frame.Comments != null )
						foreach ( var Comment in Frame.Comments )
							WriteLine( Tag)
							*/
				}
			}
		}

	}
}


