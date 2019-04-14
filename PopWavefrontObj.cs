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
#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Export Mesh as Wavefront Obj...")]
		static void ExportObj_Mesh(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var m = mf.sharedMesh;
			var Transform = mf.GetComponent<Transform>().localToWorldMatrix;

			string Filename;
			var WriteLine = IO.GetFileWriteLineFunction(out Filename, "Obj", m.name, MeshFileExtension);
			Export(WriteLine, m, Transform, null, null);
			EditorUtility.RevealInFinder(Filename);
		}
#endif

#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Export Mesh as Wavefront Obj (Maya)...")]
		static void ExportObj_Mesh_Maya(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var m = mf.sharedMesh;
			var Transform = mf.GetComponent<Transform>().localToWorldMatrix;
			var ScaleTransform = Matrix4x4.Scale(new Vector3(0.01f, 0.01f, 0.01f));
			Transform = ScaleTransform * Transform;
			Transform = UnityToMayaTransform * Transform;

			string Filename;
			var WriteLine = IO.GetFileWriteLineFunction(out Filename, "Obj", m.name, MeshFileExtension);
			Export(WriteLine, m, Transform, null, null);
			EditorUtility.RevealInFinder(Filename);
		}
#endif
	

		public const string MeshFileExtension = "obj";
		public const string MaterialFileExtension = "mtl";

		public const string Tag_Comment = "#";
		public const string Tag_MaterialLibraryFilename = "mtllib";
		public const string Tag_UseMaterial = "usemtl";
		public const string Tag_Object = "o";
		public const string Tag_Group = "g";
		public const string Tag_VertexPosition = "v";
		public const string Tag_VertexNormal = "vn";
		public const string Tag_VertexTexturecoord = "vt";
		public const string Tag_VertexParameter = "vp";	//	arbritry vertex data
		public const string Tag_Face = "f";

		//	mtl files
		public const string Tag_NewMaterial = "newmtl";
		public const string Tag_SetTextureDiffuse = "map_Kd";

		//	gr: this matrix messes up normals!
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


		public class ObjMaterial
		{
			public string	Name;

			public string	DiffuseTextureFilename;

			public ObjMaterial(string Name,string DiffuseTextureFilename)
			{
				this.Name = Name;
				this.DiffuseTextureFilename = System.IO.Path.GetFileName(DiffuseTextureFilename);
			}

			public void Export(System.Action<string> WriteLine)
			{
				WriteLine( Tag_NewMaterial + " " + Name );
				/*
				 * Ka 1.000 0.000 0.000
Kd 0.000 1.000 0.000
Ks 0.000 0.000 0.000
d 1.0
illum 1
map_Ka Mesh.jpg
map_Kd Mesh.jpg
*/
				if ( !string.IsNullOrEmpty(DiffuseTextureFilename) )
					WriteLine( Tag_SetTextureDiffuse + " " + DiffuseTextureFilename );
			}
		};


		static void InitComments(ref List<string> Comments)
		{
			if (Comments == null)
				Comments = new List<string>();
			Comments.Insert(0, "PopX.WavefrontObj");
			Comments.Add(System.DateTime.Now.ToLongDateString());
			Comments.Add(System.DateTime.Now.ToLongTimeString());
		}

		public static void Export(System.Action<string> WriteLine,ObjMaterial Material,List<string> Comments=null)
		{
			var Materials = new List<ObjMaterial>();
			Materials.Add( Material );

			Export( WriteLine, Materials, Comments );
		}

		public static void Export(System.Action<string> WriteLine,List<ObjMaterial> Materials,List<string> Comments=null)
		{
			InitComments( ref Comments );
	
			WriteLine(null);

			foreach ( var Mat in Materials )
				{
				WriteLine(null);
				Mat.Export( WriteLine );
			}
		}

		public static void Export(System.Action<string> WriteLine,Mesh Mesh,Matrix4x4 Transform,ObjMaterial Material,string MaterialFilename,List<string> Comments=null)
		{
			InitComments( ref Comments );

			foreach (var Comment in Comments)
				WriteLine("# " + Comment);
			WriteLine(null);
			WriteLine(null);

			if ( !string.IsNullOrEmpty(MaterialFilename) )
			{
				MaterialFilename = System.IO.Path.GetFileName(MaterialFilename);
				WriteLine( Tag_MaterialLibraryFilename + " " +  MaterialFilename );
			}

			//	note: to handle multiple meshes, we need to keep track of vertex count per mesh
			//	http://wiki.unity3d.com/index.php?title=ExportOBJ
			{
				//	section labels
				//	https://en.wikipedia.org/wiki/Wavefront_.obj_file#Other_geometry_formats
				//	group
				var MeshName = Mesh.name;
				if (string.IsNullOrEmpty(MeshName))
					MeshName = "Unnamed mesh";

				if ( Material != null )
					WriteLine(Tag_UseMaterial +" " + Material.Name);

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


		public static void Import(System.Func<string> ReadLine,System.Action<Vector3> OnPosition)
		{
			System.Action<string> ReadComment = (Comment) =>
			{

			};

			System.Action<string> ReadGroup = (GroupName) =>
			{

			};

			System.Action<string> ReadVertex = (PositionsString) =>
			{
				try
				{
					var Positions = PopX.Math.ParseFloats(PositionsString);
					var xyz = new Vector3(Positions[0], Positions[1], Positions[2]);

					//	todo: handle w
					if (Positions.Count >= 4)
					{
						var w = Positions[3];
						if (w != 1)
							Debug.LogWarning("Warning, not PopX.WavefrontObj.Import not handling xyzw and w=" + w);
					}
					OnPosition.Invoke(xyz);
				}
				catch(System.Exception e)
				{
					Debug.LogError("Error parsing Vertex line [" + PositionsString + "]: " + e.Message + " (Skipped)");
				}
			};

			while ( true )
			{
				var TagAndLine = ReadLine();
				if (TagAndLine == null)
					break;
				
				System.Action<string,System.Action<string>> TryHandleTag = (Tag,TagHandler)=>
				{
					if (!TagAndLine.StartsWith(Tag))
						return;
					//	remove tag + space
					var Line = TagAndLine.Substring(Tag.Length + 1);
					TagHandler(Line);
				};

				TryHandleTag(Tag_Comment, ReadComment);
				TryHandleTag(Tag_Group, ReadGroup);
				TryHandleTag(Tag_VertexPosition, ReadVertex);

			}

		}


#if UNITY_EDITOR
		static List<string> GetSelectedObjFilenames()
		{
			var Filenames = new List<string>();
			var AssetGuids = Selection.assetGUIDs;
			for (int i = 0; i < AssetGuids.Length; i++)
			{
				var Guid = AssetGuids[i];
				var Path = AssetDatabase.GUIDToAssetPath(Guid);
				//	skip .
				var Ext = System.IO.Path.GetExtension(Path).Substring(1).ToLower();
				if (Ext != MeshFileExtension)
					continue;

				Filenames.Add(Path);
			}
			return Filenames;
		}
#endif

#if UNITY_EDITOR
		//	unity doesn't load OBJ's that JUST have points in them, so lets handle that
		[MenuItem("Assets/Mesh/Import OBJ positions to Point Mesh", true)]
		static bool ImportObjPositionsToJson_Verify()
		{
			var Filenames = GetSelectedObjFilenames();
			return (Filenames.Count > 0);
		}
#endif

#if UNITY_EDITOR
		[MenuItem("Assets/Mesh/Import OBJ positions to Point Mesh")]
		static void ImportObjPositionsToJson()
		{
			var Filenames = GetSelectedObjFilenames();

			foreach (var Filename in Filenames)
			{
				try
				{
					var Mesh = ImportFileToPointMesh(Filename);
					AssetWriter.SaveAsset(Mesh);
				}
				catch (System.Exception e)
				{
					Debug.LogError("Failed to convert " + Filename + "; " + e.Message);
				}
			}
		}
#endif

		static public Mesh ImportFileToPointMesh(string Filename)
		{
			using (var Stream = new System.IO.StreamReader(Filename))
			{
				System.Func<string> ReadLine = () =>
				{
					var Line = Stream.ReadLine();
					return Line;
				};

				//	read mesh points...
				var Positions = new List<Vector3>();
				System.Action<Vector3> SavePosition = (Position) =>
				{
					Positions.Add(Position);
				};
				Import(ReadLine, SavePosition);

				//	and make a mesh
				var Mesh = new Mesh();
				Mesh.name = Filename;
				Mesh.vertices = Positions.ToArray();
				var Indexes = new int[Positions.Count];
				for (int i = 0; i < Indexes.Length; i++)
					Indexes[i] = i;
				Mesh.SetIndices(Indexes, MeshTopology.Points, 0);
				return Mesh;
			}
		}



	}
}


