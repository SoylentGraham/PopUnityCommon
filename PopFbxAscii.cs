using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif


//	rename namespace to Pop in later refactor
namespace PopX
{
	public static class FbxAscii
	{
		public const string FileExtension = "fbx";

		public const string Tag_Comment = "; ";

		static void ExportHeader(System.Action<string> WriteLine)
		{
			var Header = @"
FBXHeaderExtension:
{
	FBXHeaderVersion: 1003
	FBXVersion: 7500
	CreationTimeStamp:
	{
		Version: 1000
		Year: 2018
		Month: 4
		Day: 13
		Hour: 17
		Minute: 19
		Second: 42
		Millisecond: 918
	}
}
";
			WriteLine(Header);
		}

		static string string_Join<T>(string Seperator, T[] Elements, string ElementPrefix = "")
		{
			if (Elements == null)
				return null;
			
			var Output = "";
			var First = true;
			foreach (var Element in Elements)
			{
				if (!First)
					Output += Seperator;
				Output += ElementPrefix;
				Output += Element.ToString();
				First = false;
			}
			return Output;
		}

		//	local transform to apply to vertexes etc 
		public static void ExportObject(System.Action<string> WriteLine, Mesh Object, Matrix4x4 LocalTransform)
		{
			var VertsString = string_Join(",", Object.vertices);

			//	triangles are just indexes of 3
			//	quads are 3 indexes then a negative index
			if (Object.GetTopology(0) != MeshTopology.Triangles)
				throw new System.Exception("Exporter doesn't currently support " + Object.GetTopology(0));
			var IndexesString = string_Join(",", Object.GetIndices(0), "i");
			var NormalsString = string_Join(",", Object.normals);

			WriteLine(@"	Model: ""Model::" + Object.name + @""", ""Mesh"" {");
			WriteLine("\t\tVersion: 232");
			WriteLine("\t\tProperties60: {}");
			WriteLine(@"		Culling: ""CullingOff""");
			WriteLine("\t\tVertices: " + VertsString);
			WriteLine("\t\tPolygonVertexIndex: " + IndexesString);
			WriteLine("\t\tGeometryVersion: " + 124);
			if (NormalsString != null )
			{
				WriteLine(@"		LayerElementNormal: 0 {
					Version: 101
					Name: ""
					MappingInformationType: ""ByPolygonVertex""
					ReferenceInformationType: ""Direct""
					Normals: " + NormalsString + @"
				}");
			}

			//end of mesh
			WriteLine("}");
			/*
		LayerElementUV: 0 {
			Version: 101
			Name: "UVMap"
			MappingInformationType: "ByPolygonVertex"
			ReferenceInformationType: "IndexToDirect"
			UV: 0.493900,0.746800,0.630800,0.745600,0.502100,0.684400,0.634900,0.684300
			UVIndex: 2,3,1,0
		}
		LayerElementTexture: 0 {
			Version: 101
			Name: ""
			MappingInformationType: "NoMappingInformation"
			ReferenceInformationType: "IndexToDirect"
			BlendMode: "Translucent"
			TextureAlpha: 1
			TextureId: 
		}
		LayerElementMaterial: 0 {
			Version: 101
			Name: ""
			MappingInformationType: "AllSame"
			ReferenceInformationType: "IndexToDirect"
			Materials: 0
		}
		Layer: 0 {
			Version: 100
			LayerElement:  {
				Type: "LayerElementNormal"
				TypedIndex: 0
			}
			LayerElement:  {
				Type: "LayerElementSmoothing"
				TypedIndex: 0
			}
			LayerElement:  {
				Type: "LayerElementUV"
				TypedIndex: 0
			}
			LayerElement:  {
				Type: "LayerElementTexture"
				TypedIndex: 0
			}
			LayerElement:  {
				Type: "LayerElementMaterial"
				TypedIndex: 0
			}
		}
		*/
	
		}

		public static void ExportObject(System.Action<string> WriteLine, Object Object, Matrix4x4 LocalTransform)
		{
			if (Object is Mesh)
			{
				ExportObject(WriteLine, Object as Mesh, LocalTransform);
			}
			else
			{
				throw new System.Exception("Don't know how to FBX-ascii export " + Object.GetType().ToString());
			}
		}

		public static void Export(System.Action<string> WriteLine,List<Object> Objects,Matrix4x4 LocalTransform)
		{
			WriteLine("Objects: {");
			foreach ( var Obj in Objects )
			{
				try
				{
					ExportObject( WriteLine, Obj, LocalTransform );
				}
				catch(System.Exception e)
				{
					Debug.LogException(e);
				}
			}
			WriteLine("}");
		}

		public static void Export(System.Action<string> WriteLine,Mesh Mesh,Matrix4x4 Transform,List<string> Comments=null)
		{
			Pop.AllocIfNull( ref Comments );
			Comments.Add("Using WIP PopX.FbxAscii exporter from @soylentgraham");
			foreach (var Comment in Comments)
				WriteLine(Tag_Comment + " " + Comment);
			WriteLine(null);

			ExportHeader(WriteLine);

			var Objects = new List<Object>();
			Objects.Add(Mesh);
			Export(WriteLine, Objects, Transform);


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
				if (Ext != FileExtension)
					continue;

				Filenames.Add(Path);
			}
			return Filenames;
		}
#endif

		/*
#if UNITY_EDITOR
		[MenuItem("Assets/Camera/Export Fbx Ascii")]
		static void ExportFbx_Camera(MenuCommand menuCommand)
		{
			var cam = menuCommand.context as Camera;

			string Filename;
			var WriteLine = IO.GetFileWriteLineFunction(out Filename, "Fbx", cam.name, FileExtension);
			Export(WriteLine, cam, WavefrontObj.UnityToMayaTransform);
			EditorUtility.RevealInFinder(Filename);
		}
#endif
		*/

#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Export Fbx Ascii")]
		static void ExportFbx_Mesh(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var m = mf.sharedMesh;

			string Filename;
			var WriteLine = IO.GetFileWriteLineFunction(out Filename, "Fbx", m.name, FileExtension);
			Export(WriteLine, m, WavefrontObj.UnityToMayaTransform);
			EditorUtility.RevealInFinder(Filename);
		}
#endif

	}
}


