using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif



public class AnimFrame
{
	public Vector3 Position;
	public Quaternion Rotation;
	public Vector3 RotationEular{ get { return Rotation.eulerAngles; }}
	public Vector3 Scale;
	public float Time;
}

public class AnimObject
{
	public List<AnimFrame> Frames;

	public void GetCurveData(out float[] x, out float[] y, out float[] z, System.Func<AnimFrame, float> GetX, System.Func<AnimFrame, float> GetY,System.Func<AnimFrame, float> GetZ)
	{
		x = new float[Frames.Count];
		y = new float[Frames.Count];
		z = new float[Frames.Count];
		for (int f = 0; f < Frames.Count; f++)
		{
			var Frame = Frames[f];
			x[f] = GetX(Frame);
			y[f] = GetY(Frame);
			z[f] = GetZ(Frame);
		}
	}
	public void GetPositionCurveData(out float[] x, out float[] y, out float[] z)
	{
		System.Func<AnimFrame, float> GetX = (Frame) => { return Frame.Position.x; };
		System.Func<AnimFrame, float> GetY = (Frame) => { return Frame.Position.y; };
		System.Func<AnimFrame, float> GetZ = (Frame) => { return Frame.Position.z; };
		GetCurveData(out x, out y, out z, GetX, GetY, GetZ);
	}

	public void GetRotationCurveData(out float[] x, out float[] y, out float[] z)
	{
		System.Func<AnimFrame, float> GetX = (Frame) => { return Frame.RotationEular.x; };
		System.Func<AnimFrame, float> GetY = (Frame) => { return Frame.RotationEular.y; };
		System.Func<AnimFrame, float> GetZ = (Frame) => { return Frame.RotationEular.z; };
		GetCurveData(out x, out y, out z, GetX, GetY, GetZ);
	}

	public void GetScaleCurveData(out float[] x, out float[] y, out float[] z)
	{
		System.Func<AnimFrame, float> GetX = (Frame) => { return Frame.Scale.x; };
		System.Func<AnimFrame, float> GetY = (Frame) => { return Frame.Scale.y; };
		System.Func<AnimFrame, float> GetZ = (Frame) => { return Frame.Scale.z; };
		GetCurveData(out x, out y, out z, GetX, GetY, GetZ);
	}
}


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
		public FbxValue_Floats(Vector3[] vs,System.Func<Vector3,Vector3> transform) { Numbers = null; foreach (var v in vs) Append(transform(v)); }

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

	public class AnimationCurve
	{
		
	}
	public class AnimationCurveNode
	{
		//	connect to property in other node
	}
	public class AnimationLayer
	{
		public List<AnimationCurveNode> AnimationCurveNodes;
	}
	public class AnimationStack
	{
		public string Name;
		public List<AnimationLayer> AnimationLayers;


	}
	// FBXTree.Objects.subNodes.AnimationCurve(connected to AnimationCurveNode )
	// FBXTree.Objects.subNodes.AnimationCurveNode ( connected to AnimationLayer and an animated property in some other node )
	// FBXTree.Objects.subNodes.AnimationLayer ( connected to AnimationStack )
	// FBXTree.Objects.subNodes.AnimationStack

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

		public const long KTime_Second = 46186158000;

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
			Model.Add("Vertices", new FbxValue_Floats(mesh.vertices, (n) => { return transform.MultiplyPoint(n); }));
			//	indexes start at 1, and last in poly is negative
			var FbxIndexes = GetMeshIndexes(mesh.GetIndices(0), mesh.GetTopology(0));
			Model.Add("PolygonVertexIndex", new FbxValue_Ints(FbxIndexes));
			Model.Add("GeometryVersion", 124);

			int LayerNumber = 0;
			var NormalLayer = Model.Add("LayerElementNormal", LayerNumber);
			NormalLayer.Add("Version", 101);
			NormalLayer.Add("Name", "");
			//	ByPolygon	It means that there is a normal for every polygon of the model.
			//	ByPolygonVertex	It means that there is a normal for every vertex of every polygon of the model.
			//	ByVertex	It means that there is a normal for every vertex of the model.
			//	gr: ByVertex "Unsupported wedge mapping mode type.Please report this bug."
			//		even though I think that's the right one to use.. as ByPolygonVertex looks wrong
			NormalLayer.Add("MappingInformationType", "ByPolygonVertex");
			NormalLayer.Add("ReferenceInformationType", "Direct");
			NormalLayer.Add("Normals",new FbxValue_Floats(mesh.normals, (n) => { return transform.MultiplyVector(n); }));

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


		struct FbxObject
		{
			//public string Name;
			public int Ident;

			public FbxObject(int Ident)
			{
				this.Ident = Ident;
			}
		}

		class FbxObjectManager
		{
			public List<FbxObject> Objects = new List<FbxObject>();
			int IdentCounter = 6000;

			int AllocIdent()
			{
				IdentCounter++;
				return IdentCounter;
			}

			public FbxObject AddAnimationCurveNode(FbxAnimationCurveNodeType NodeType,Vector3 DefaultValue)
			{
				var Node = new FbxObject(AllocIdent());
				return Node;
			}

			public FbxObject AddAnimationCurve( float[] curveData)
			{
				//	add a new object
				var InputId = new FbxObject(AllocIdent());

				// prepare some data
				string keyValueFloatDataStr = "";
				string timeArrayDataStr = "";

				for (int i = 0; i < curveData.Length; i++)
				{
					if (i == 0)
					{
						keyValueFloatDataStr += curveData[i].ToString();
						timeArrayDataStr += FbxHelper.getFbxSeconds(i, 60);
					}
					else
					{
						keyValueFloatDataStr += "," + curveData[i].ToString();
						timeArrayDataStr += "," + FbxHelper.getFbxSeconds(i, 60);
					}
				}

				//AnimationCurve: 106102887970656, "AnimCurve::", "" 
				string nodeData = inputId + ", \"AnimCurve::\", \"\"";
				FbxDataNode curveNode = new FbxDataNode("AnimationCurve", nodeData, 1);

				string dataLengthStr = curveData.Length.ToString();

				curveNode.addSubNode("Default", "0");
				curveNode.addSubNode("KeyVer", "4008");

				FbxDataNode keyTimeNode = new FbxDataNode("KeyTime", "*" + dataLengthStr, 2);
				keyTimeNode.addSubNode("a", timeArrayDataStr);
				curveNode.addSubNode(keyTimeNode);

				FbxDataNode keyValuesNode = new FbxDataNode("KeyValueFloat", "*" + dataLengthStr, 2);
				keyValuesNode.addSubNode("a", keyValueFloatDataStr);
				curveNode.addSubNode(keyValuesNode);

				curveNode.addSubNode(";KeyAttrFlags", "Cubic|TangeantAuto|GenericTimeIndependent|GenericClampProgressive");

				FbxDataNode keyAttrFlagsNode = new FbxDataNode("KeyAttrFlags", "*1", 2);
				keyAttrFlagsNode.addSubNode("a", "24840");
				curveNode.addSubNode(keyAttrFlagsNode);

				FbxDataNode keyRefCountNode = new FbxDataNode("KeyAttrRefCount", "*1", 2);
				keyRefCountNode.addSubNode("a", dataLengthStr);
				curveNode.addSubNode(keyRefCountNode);

				//	objects.add curvenode
				//return curveNode;
				return InputId;
			}
		}

		class FbxConnectionManager
		{
			public List<FbxConnection> Connections = new List<FbxConnection>();

			public void Add(FbxConnection Connection)
			{
				Connections.Add(Connection);
			}
		}


		struct FbxConnection
		{
			/*
			 * ;AnimCurveNode::T, Model::pCube1
			 * C: "OP",105553124109952,140364338281984, "Lcl Translation"
			 * 
			 */

			public string type1;
			public string name1;
			public FbxObject Object1;

			public string type2;
			public string name2;
			public FbxObject Object2;

			public string relation;
			public string Comment;

			public FbxConnection(string type1,string name1,FbxObject Object1, string type2,string name2,FbxObject Object2, string relation,string Comment)
			{
				this.type1 = type1;
				this.name1 = name1;
				this.Object1 = Object1;

				this.type2 = type2;
				this.name2 = name2;
				this.Object2 = Object2;

				this.relation = relation;
				this.Comment = Comment;
			}
			/*
			public string getOutputData()
			{
				if (hasRelationDesc)
					return "\t;" + type1 + "::" + name1 + ", " + type2 + "::" + name2 + "\n\tC: \"" + relation + "\"," + id1 + "," + id2 + ", \"" + relationDesc + "\"\n";
				else
					return "\t;" + type1 + "::" + name1 + ", " + type2 + "::" + name2 + "\n\tC: \"" + relation + "\"," + id1 + "," + id2 + "\n";
			}
*/
		}

		static void MakeAnimationNode(AnimObject Anim,FbxObject AnimatedObject,FbxObject AnimLayer,FbxObjectManager ObjectManager, FbxConnectionManager ConnectionManager)
		{
			//	make the animation nodes
			FbxObject AnimNodePosition, AnimNodeRotation, AnimNodeScale;
			MakeAnimationNode(Anim, AnimLayer, ObjectManager, ConnectionManager, out AnimNodePosition, out AnimNodeRotation, out AnimNodeScale);

			//	object connection
			ConnectionManager.Add( new FbxConnection("AnimCurveNode", "T", AnimNodePosition, "Model", AnimatedObject.Name, AnimatedObject, "OP", "Lcl Translation"));
			ConnectionManager.Add( new FbxConnection("AnimCurveNode", "R", AnimNodeRotation, "Model", AnimatedObject.Name, AnimatedObject, "OP", "Lcl Rotation"));
			ConnectionManager.Add( new FbxConnection("AnimCurveNode", "S", AnimNodeScale, "Model", AnimatedObject.Name, AnimatedObject, "OP", "Lcl Scaling"));
		}


		static void MakeAnimationNode(AnimObject Anim,FbxObject AnimLayer,FbxObjectManager ObjectManager,FbxConnectionManager ConnectionManager, out FbxObject AnimNodePosition, out FbxObject AnimNodeRotation, out FbxObject AnimNodeScale)
		{
			// add anim nodes
			var ao = Anim;

			// create Animation Curve Nodes
			var NodeT = ObjectManager.AddAnimationCurveNode(FbxAnimationCurveNodeType.Translation, ao.Frames[0].Position);
			var NodeR = ObjectManager.AddAnimationCurveNode(FbxAnimationCurveNodeType.Rotation, ao.Frames[0].RotationEular);
			var NodeS = ObjectManager.AddAnimationCurveNode(FbxAnimationCurveNodeType.Scale, ao.Frames[0].Scale);

			AnimNodePosition = NodeT;
			AnimNodeRotation = NodeR;
			AnimNodeScale = NodeS;

			//	get data
			float[] TXData;
			float[] TYData;
			float[] TZData;
			ao.GetPositionCurveData(out TXData, out TYData, out TZData);
			var CurveTX = ObjectManager.AddAnimationCurve(TXData);
			var CurveTY = ObjectManager.AddAnimationCurve(TYData);
			var CurveTZ = ObjectManager.AddAnimationCurve(TZData);

			float[] RXData;
			float[] RYData;
			float[] RZData;
			ao.GetRotationCurveData(out RXData, out RYData, out RZData);
			var CurveRX = ObjectManager.AddAnimationCurve(RXData);
			var CurveRY = ObjectManager.AddAnimationCurve(RYData);
			var CurveRZ = ObjectManager.AddAnimationCurve(RZData);

			float[] SXData;
			float[] SYData;
			float[] SZData;
			ao.GetScaleCurveData(out SXData, out SYData, out SZData);
			var CurveSX = ObjectManager.AddAnimationCurve(SXData);
			var CurveSY = ObjectManager.AddAnimationCurve(SYData);
			var CurveSZ = ObjectManager.AddAnimationCurve(SZData);



			//	animation 
			ConnectionManager.Add(new FbxConnection("AnimCurveNode", "T", NodeT, "AnimLayer", "BaseLayer", AnimLayer, "OO", ""));
			ConnectionManager.Add(new FbxConnection("AnimCurveNode", "R", NodeR, "AnimLayer", "BaseLayer", AnimLayer, "OO", ""));
			ConnectionManager.Add(new FbxConnection("AnimCurveNode", "S", NodeS, "AnimLayer", "BaseLayer", AnimLayer, "OO", ""));

			ConnectionManager.Add(new FbxConnection("AnimCurve", "", CurveTX, "AnimCurveNode", "T", NodeT, "OP", "d|X"));
			ConnectionManager.Add(new FbxConnection("AnimCurve", "", CurveTY, "AnimCurveNode", "T", NodeT, "OP", "d|Y"));
			ConnectionManager.Add(new FbxConnection("AnimCurve", "", CurveTZ, "AnimCurveNode", "T", NodeT, "OP", "d|Z"));

			ConnectionManager.Add(new FbxConnection("AnimCurve", "", CurveRX, "AnimCurveNode", "R", NodeR, "OP", "d|X"));
			ConnectionManager.Add(new FbxConnection("AnimCurve", "", CurveRY, "AnimCurveNode", "R", NodeR, "OP", "d|Y"));
			ConnectionManager.Add(new FbxConnection("AnimCurve", "", CurveRZ, "AnimCurveNode", "R", NodeR, "OP", "d|Z"));

			ConnectionManager.Add(new FbxConnection("AnimCurve", "", CurveSX, "AnimCurveNode", "S", NodeS, "OP", "d|X"));
			ConnectionManager.Add(new FbxConnection("AnimCurve", "", CurveSY, "AnimCurveNode", "S", NodeS, "OP", "d|Y"));
			ConnectionManager.Add(new FbxConnection("AnimCurve", "", CurveSZ, "AnimCurveNode", "S", NodeS, "OP", "d|Z"));

		}


	}
}


