using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif


//	rename namespace to Pop in later refactor
namespace PopX
{
	//	fbx is a tree of nodes, override this to generate the tree
	public class FbxProperty
	{
		//	Tag: value [value, value]
		public List<string>			Comments;
		public string				Name;
		public List<FbxValue>		Values;
		public List<FbxProperty>	Children;

		public FbxProperty(string Name)
		{
			this.Name = Name;
			this.Values = new List<FbxValue>();
			this.Comments = new List<string>();
		}

		//	add value
		public void Add(FbxValue Value)
		{
			Values.Add(Value);
		}

		//	add child property
		public FbxProperty Add(string PropertyName, int Value) { return Add(PropertyName, FbxValues.Create(Value)); }
		public FbxProperty Add(string PropertyName, string Value) { return Add(PropertyName, FbxValues.Create(Value)); }

		public FbxProperty Add(string PropertyName, FbxValue Value)
		{
			var Prop = new FbxProperty(PropertyName);
			Prop.Add(Value);
			Add(Prop);
			return Prop;
		}

		public FbxProperty AddProperty(string PropertyName)
		{
			var Prop = new FbxProperty(PropertyName);
			Add(Prop);
			return Prop;
		}

		public void Add(FbxProperty Child)
		{
			if (Children == null)
				Children = new List<FbxProperty>();
			Children.Add(Child);
		}

		public void AddComment(string Comment)
		{
			if (this.Comments == null)
				this.Comments = new List<string>();
			this.Comments.Add(Comment);
		}

		public void AddComment(List<string> Comments)
		{
			foreach (var Comment in Comments)
				AddComment(Comment);
		}
	};
		
	public interface FbxValue
	{
		string GetString();
	};

	public static class FbxValues
	{
		static public FbxValue Create(string Value) { return new FbxValue_String(Value); }
		static public FbxValue Create(int Value) { return new FbxValue_Ints(Value); }
	};

	public struct FbxValue_Property : FbxValue
	{
		public FbxProperty Property;

		public string GetString()
		{
			throw new System.Exception("Don't call this on a property, export it via tree");
		}

		public FbxValue_Property(FbxProperty Property)
		{
			this.Property = Property;
		}
	};

	public struct FbxValue_Floats : FbxValue
	{
		public List<float> Numbers;

		static string GetString(float f)
		{
			return f.ToString("F3");
		}
		public string GetString()
		{
			var String = GetString(Numbers[0]);
			for (var i = 1; i < Numbers.Count; i++)
				String += FbxAscii.PropertySeperator + GetString(Numbers[i]);
			return String;
		}

		public FbxValue_Floats(Vector3 v) { Numbers = null;	Append(v); }
		public FbxValue_Floats(Vector3[] vs) { Numbers = null; foreach (var v in vs) Append(v); }
		public FbxValue_Floats(Vector3[] vs,Matrix4x4 transform) { Numbers = null; foreach (var v in vs) Append(transform.MultiplyPoint(v)); }

		void Append(Vector3 v)
		{
			if (Numbers == null)
				Numbers = new List<float>();
			Numbers.Add(v.x);
			Numbers.Add(v.y);
			Numbers.Add(v.z);
		}
	};

	public struct FbxValue_Ints : FbxValue
	{
		public List<int> Numbers;

		static string GetString(int f)
		{
			return f.ToString();
		}
		public string GetString()
		{
			var String = GetString(Numbers[0]);
			for (var i = 1; i < Numbers.Count; i++)
				String += FbxAscii.PropertySeperator + GetString(Numbers[i]);
			return String;
		}

		public FbxValue_Ints(int v) { Numbers = null; Append(v); }
		public FbxValue_Ints(int[] vs) { Numbers = null; foreach (var v in vs) Append(v); }
		public FbxValue_Ints(List<int> vs) { Numbers = null; foreach (var v in vs) Append(v); }

		void Append(int v)
		{
			if (Numbers == null)
				Numbers = new List<int>();
			Numbers.Add(v);
		}
	};

	public struct FbxValue_String : FbxValue
	{
		public string String;
		public string GetString() { return '"' + String+ '"'; }

		public FbxValue_String(string Value)
		{
			this.String = Value;
		}
	};


	public static class FbxAscii
	{
		public const string FileExtension = "fbx";


		public const string Tag_Comment = "; ";
		public const string PropertySeperator = ", ";
		public const int Version = (VersionMajor * 1000) + (VersionMinor * 100) + (VersionRelease * 10);
		public const int VersionMajor = 6;
		public const int VersionMinor = 1;
		public const int VersionRelease = 0;
		public const bool ReversePolygonOrder = true;


		static FbxProperty GetHeaderProperty(string Creator="Pop FbxAscii Exporter")
		{
			var Root = new FbxProperty("FBXHeaderExtension");
			Root.Add("FBXHeaderVersion",1003);
			Root.Add("FBXVersion", Version);
			Root.Add("Creator", Creator);

			//	won't load in unity without this comment at the top
			Root.AddComment("FBX " + VersionMajor+"."+VersionMinor+"."+VersionRelease.ToString("D2") +"  project file");
			return Root;
		}

		static List<int> GetMeshIndexes(int[] Indexes,MeshTopology Topology)
		{
			int PolyIndexCount;
			if (Topology == MeshTopology.Triangles)
				PolyIndexCount = 3;
			else if (Topology == MeshTopology.Quads)
				PolyIndexCount = 4;
			else
				throw new System.Exception("meshes of " + Topology +" are unsupported");

			var FbxIndexes = new List<int>(Indexes.Length);
			var Poly = new List<int>( new int[PolyIndexCount] );

			for (int i = 0; i < Indexes.Length; i +=PolyIndexCount)
			{
				for (int p = 0; p < PolyIndexCount; p++)
					Poly[p] = Indexes[i + p];
				//	add in reverse order - imports with reverse winding it seems
				if (ReversePolygonOrder)
					Poly.Reverse();
			
				//	last one denotes end of polygon and is negative (+1)
				Poly[PolyIndexCount - 1] = -(Poly[PolyIndexCount - 1]+1);
				FbxIndexes.AddRange(Poly);
			}
			return FbxIndexes;
		}

		static FbxProperty GetObjectProperty(Mesh mesh,Matrix4x4 transform)
		{
			var Model = new FbxProperty("Model");
			Model.Add(FbxValues.Create("Model::" + mesh.name));
			Model.Add(FbxValues.Create("Mesh"));

			Model.Add("Version", 232);
			Model.Add("Vertices", new FbxValue_Floats(mesh.vertices,transform));
			//	indexes start at 1, and last in poly is negative
			var FbxIndexes = GetMeshIndexes(mesh.GetIndices(0), mesh.GetTopology(0));
			Model.Add("PolygonVertexIndex", new FbxValue_Ints(FbxIndexes));
			Model.Add("GeometryVersion", 124);

			int LayerNumber = 0;
			var NormalLayer = Model.Add("LayerElementNormal", LayerNumber);
			NormalLayer.Add("Version", 101);
			NormalLayer.Add("Name", "");
			NormalLayer.Add("MappingInformationType", "ByPolygonVertex");
			NormalLayer.Add("ReferenceInformationType", "Direct");
			NormalLayer.Add("Normals",new FbxValue_Floats(mesh.normals));

			var Layer = Model.Add("Layer", LayerNumber);
			Layer.Add("Version", 100);
			var len = Layer.AddProperty("LayerElement");
			len.Add("Type", "LayerElementNormal");
			len.Add("TypedIndex", 0);
			var les = Layer.AddProperty("LayerElement");
			les.Add("Type", "LayerElementSmoothing");
			les.Add("TypedIndex", 0);
			var leuv = Layer.AddProperty("LayerElement");
			leuv.Add("Type", "LayerElementUV");
			leuv.Add("TypedIndex", 0);
			var let = Layer.AddProperty("LayerElement");
			let.Add("Type", "LayerElementTexture");
			let.Add("TypedIndex", 0);
			var lem = Layer.AddProperty("LayerElement");
			lem.Add("Type", "LayerElementMaterial");
			lem.Add("TypedIndex", 0);

			return Model;
		}

		static FbxProperty GetDefinitionsProperty(int MeshCount)
		{
			var Defs = new FbxProperty("Definitions");
			Defs.Add("Version", 100);
			Defs.Add("Count", MeshCount);
			var otm = Defs.Add("ObjectType", "Model");
			otm.Add("Count", MeshCount);
			var otg = Defs.Add("ObjectType", "Geometry");
			otg.Add("Count", MeshCount);

			return Defs;
		}

		static FbxProperty GetConnectionsProperty(List<object[]> Connections)
		{
			var ConnectionsProp = new FbxProperty("Connections");

			foreach ( var Connection in Connections )
			{
				var Prop = ConnectionsProp.AddProperty("Connect");
				foreach ( var Value in Connection )
				{
					if (Value is string)
						Prop.Add(FbxValues.Create((string)Value));
					if (Value is Mesh)
						Prop.Add(FbxValues.Create("Model::"+((Mesh)Value).name));
					if (Value is Material)
						Prop.Add(FbxValues.Create("Material::"+((Material)Value).name));
				}
			}

			return ConnectionsProp;
		}

		public static string GetIndent(int Indents)
		{
			var String = "";
			for (int i = 0; i < Indents; i++)
				String += "\t";
			return String;
		}

		public static void Export(System.Action<string> WriteLine, FbxProperty Property,int Indent=0)
		{
			foreach (var Comment in Property.Comments)
				WriteLine(Tag_Comment + Comment);

			var ValuesLine = GetIndent(Indent) + Property.Name + ": ";
			var Values = Property.Values;

			for (int i = 0; i < Values.Count;	i++ )
			{
				var Value = Values[i];
				if (i > 0)
					ValuesLine += PropertySeperator;
				ValuesLine += Value.GetString();
			}

			WriteLine(ValuesLine);

			//	open tree
			if (Property.Children!=null)
			{
				WriteLine(GetIndent(Indent)+"{");
				foreach ( var Child in Property.Children )
					Export(WriteLine, Child, Indent + 1);
				WriteLine(GetIndent(Indent) +"}");
			}
		}

		public static void Export(System.Action<string> WriteLine,List<FbxProperty> Tree, List<string> Comments = null)
		{
			Pop.AllocIfNull(ref Comments);
			Comments.Add("Using WIP PopX.FbxAscii exporter from @soylentgraham");
			foreach (var Comment in Comments)
				WriteLine(Tag_Comment + " " + Comment);
			WriteLine(null);

			//	write out the tree
			foreach ( var Prop in Tree)
			{
				Export(WriteLine,Prop);
				WriteLine(null);
			}
		}

		public static void Export(System.Action<string> WriteLine,Mesh mesh,Matrix4x4 transform,List<string> Comments=null)
		{
			Pop.AllocIfNull(ref Comments);
			Comments.Add("Using WIP PopX.FbxAscii exporter from @soylentgraham");

			var Header = GetHeaderProperty();
			Header.AddComment(Comments);
			Export(WriteLine,Header);

			var Objects = new FbxProperty("Objects");
			Objects.Add(GetObjectProperty(mesh,transform));
			Export(WriteLine, Objects);

			var Definitions = GetDefinitionsProperty(1);
			Export(WriteLine, Definitions);

			var SceneMesh = new Mesh();
			SceneMesh.name = "Scene";
			//	need a dummy material or it doesn't show up in unity
			var meshMaterial = "Material::CactusPack_Sprite";
			//var meshMaterial = new Material("Contents");
			//meshMaterial.name = "DummyMaterial";

			var Connections = new List<object[]>();
			Connections.Add(new object[] { "OO", mesh, SceneMesh });
			Connections.Add(new object[] { "OO", meshMaterial, mesh });

			var ConnectionsProp = GetConnectionsProperty(Connections);
			Export(WriteLine, ConnectionsProp);
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


