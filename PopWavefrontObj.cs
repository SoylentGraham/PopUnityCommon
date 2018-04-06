using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif


//	rename namespace to Pop in later refactor
namespace PopX
{
	public static class WavefrontObj 
	{
		public const string FileExtension = "obj";

		public const string Tag_Object = "o";
		public const string Tag_Group = "g";
		public const string Tag_VertexPosition = "v";
		public const string Tag_VertexNormal = "vn";
		public const string Tag_VertexTexturecoord = "vt";
		public const string Tag_VertexParameter = "vp";	//	arbritry vertex data
		public const string Tag_Face = "f";

		public static Matrix4x4 UnityToMayaTransform
		{
			get
			{
				//	output cm
				var cm = 100.0f;

				//	swap x & z AND face opposite direction (tested in maya)
				var Transform = new Matrix4x4();
				Transform.SetRow(0,new Vector4(0, 0, -1, 0));
				Transform.SetRow(1,new Vector4(0, 1, 0, 0));
				Transform.SetRow(2,new Vector4(-1, 0, 0, 0));
				Transform.SetRow(3,new Vector4(0, 0, 0, 1));

				//	scale to cm
				Transform *= Matrix4x4.Scale(cm.xxx());

				return Transform;
			}
		}
		public static Matrix4x4 MayaToUnityTransform
		{
			get
			{
				return UnityToMayaTransform.inverse;
			}
		}


		public static void Export(System.Action<string> WriteLine,Mesh Mesh,Matrix4x4 Transform,List<string> Comments=null)
		{
			Transform = UnityToMayaTransform * Transform;

			if (Comments == null)
				Comments = new List<string>();
			Comments.Insert(0, "PopX.WavefrontObj");
			Comments.Add(System.DateTime.Now.ToLongDateString());
			Comments.Add(System.DateTime.Now.ToLongTimeString());

			foreach (var Comment in Comments)
				WriteLine("# " + Comment);
			WriteLine(null);
			WriteLine(null);

			//	note: to handle multiple meshes, we need to keep track of vertex count per mesh
			//	http://wiki.unity3d.com/index.php?title=ExportOBJ
			{
				//	section labels
				//	https://en.wikipedia.org/wiki/Wavefront_.obj_file#Other_geometry_formats
				//	group
				var MeshName = Mesh.name;
				if (string.IsNullOrEmpty(MeshName))
					MeshName = "Unnamed mesh";

				WriteLine(Tag_Object +" " + MeshName);

				var Positions = Mesh.vertices;
				foreach (var Position in Positions)
				{
					var PosWavefront = Transform.MultiplyPoint(Position);
					WriteLine(string.Format(Tag_VertexPosition + " {0} {1} {2}", PosWavefront.x, PosWavefront.y, PosWavefront.z));
				}
				WriteLine(null);

				var Normals = Mesh.normals;
				if (Normals != null)
				{
					foreach (var Normal in Normals)
					{
						var NormWavefront = Transform.MultiplyVector(Normal);
						WriteLine(string.Format(Tag_VertexNormal + " {0} {1} {2}", NormWavefront.x, NormWavefront.y, NormWavefront.z));
					}
					WriteLine(null);
				}

				var Uvs = Mesh.uv;
				if (Uvs != null)
				{
					foreach (var Uv in Uvs)
					{
						WriteLine(string.Format(Tag_VertexTexturecoord + " {0} {1}", Uv.x, Uv.y));
					}
					WriteLine(null);
				}


				var Triangles = Mesh.GetTriangles(0);
				var TriangleIndexOffset = 1;	//	!!
				for (var t = 0; t < Triangles.Length; t += 3)
				{
					var t0 = Triangles[t + 0];
					var t1 = Triangles[t + 1];
					var t2 = Triangles[t + 2];
					var ti0 = TriangleIndexOffset + t0;
					var ti1 = TriangleIndexOffset + t1;
					var ti2 = TriangleIndexOffset + t2;
					WriteLine(string.Format( Tag_Face + " {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", ti0, ti1, ti2));
				}
				WriteLine(null);
			}
		}

	}
}


