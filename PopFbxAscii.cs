using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

public enum FbxAnimationCurveNodeType
{
	Translation,
	Rotation,
	Scale,
	Visibility
}

public static class FbxHelper
{

	public static long GetFbxSeconds(int frameIndex, int frameRate)
	{
		long result = 46186158000;
		result = result * frameIndex;
		result = result / frameRate;

		return result;
	}

	public static long GetFbxSeconds(float Time)
	{
		//	argh
		var KTimeSecondf = 46186158000.0f;
		var KTimef = Time * KTimeSecondf;
		var KTime = (long)KTimef;
		return KTime;
	}

}


public struct AnimFrame
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

	public float GetStartTime()
	{
		return Frames[0].Time;
	}

	public float GetEndTime()
	{
		return Frames[Frames.Count-1].Time;
	}

	public void AddFrame(Vector3 Position,Quaternion Rotation,float Time)
	{
		var Frame = new AnimFrame();
		Frame.Position = Position;
		Frame.Rotation = Rotation;
		Frame.Scale = Vector3.one;
		Frame.Time = Time;
		Pop.AllocIfNull(ref Frames);
		Frames.Add(Frame);
	}

	public void GetCurveTimes(out float[] Times)
	{
		Times = new float[Frames.Count];
		for (int f = 0; f < Frames.Count; f++)
		{
			var Frame = Frames[f];
			Times[f] = Frame.Time;
		}
	}

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
		public List<string> Comments;
		public string Name;
		public List<FbxValue> Values;
		public List<FbxProperty> Children;

		public FbxProperty(string Name)
		{
			this.Name = Name;
			this.Values = new List<FbxValue>();
			this.Comments = new List<string>();
		}

		//	add value
		public void AddValue(FbxValue Value) { Values.Add(Value); }
		public void AddValue(string Value) { AddValue(FbxValues.Create(Value)); }
		public void AddValue(int Value) { AddValue(FbxValues.Create(Value)); }
		public void AddValue(long Value) { AddValue(FbxValues.Create(Value)); }
		public void AddValue(float Value) { AddValue(FbxValues.Create(Value)); }
		public void AddValue(Vector3 Value) { AddValue(FbxValues.Create(Value)); }


		//	add child property
		public FbxProperty AddProperty(string PropertyName, int Value) { return AddProperty(PropertyName, FbxValues.Create(Value)); }
		public FbxProperty AddProperty(string PropertyName, string Value) { return AddProperty(PropertyName, FbxValues.Create(Value)); }
		public FbxProperty AddProperty(string PropertyName, Vector3 Value) { return AddProperty(PropertyName, FbxValues.Create(Value)); }

		public FbxProperty AddProperty(string PropertyName, FbxValue Value)
		{
			var Prop = new FbxProperty(PropertyName);
			Prop.AddValue(Value);
			AddProperty(Prop);
			return Prop;
		}

		public FbxProperty AddProperty(string PropertyName)
		{
			var Prop = new FbxProperty(PropertyName);
			AddProperty(Prop);
			return Prop;
		}

		public void AddProperty(FbxProperty Child)
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
		static public FbxValue Create(long Value) { return new FbxValue_Ints(Value); }
		static public FbxValue Create(float Value) { return new FbxValue_Floats(Value); }
		static public FbxValue Create(Vector3 Value) { return new FbxValue_Floats(Value); }
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

		public FbxValue_Floats(float v) { Numbers = null; Append(v); }
		public FbxValue_Floats(float[] vs) { Numbers = null; foreach (var v in vs) Append(v); }
		public FbxValue_Floats(Vector3 v) { Numbers = null; Append(v); }
		public FbxValue_Floats(Vector3[] vs) { Numbers = null; foreach (var v in vs) Append(v); }
		public FbxValue_Floats(Vector3[] vs, System.Func<Vector3, Vector3> transform) { Numbers = null; foreach (var v in vs) Append(transform(v)); }

		void Append(Vector3 v)
		{
			Append(v.x);
			Append(v.y);
			Append(v.z);
		}

		void Append(float v)
		{
			if (Numbers == null)
				Numbers = new List<float>();
			Numbers.Add(v);
		}
	};

	public struct FbxValue_Ints : FbxValue
	{
		public List<long> Numbers;

		static string GetString(long f)
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

		public FbxValue_Ints(long v) { Numbers = null; Append(v); }
		public FbxValue_Ints(int v) { Numbers = null; Append(v); }
		public FbxValue_Ints(int[] vs) { Numbers = null; foreach (var v in vs) Append(v); }
		public FbxValue_Ints(long[] vs) { Numbers = null; foreach (var v in vs) Append(v); }
		public FbxValue_Ints(List<int> vs) { Numbers = null; foreach (var v in vs) Append(v); }
		public FbxValue_Ints(List<long> vs) { Numbers = null; foreach (var v in vs) Append(v); }

		void Append(long v)
		{
			if (Numbers == null)
				Numbers = new List<long>();
			Numbers.Add(v);
		}
	};

	public struct FbxValue_String : FbxValue
	{
		public string String;
		public string GetString() { return '"' + String + '"'; }

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
		public const int VersionMajor = 7;
		public const int VersionMinor = 5;
		public const int VersionRelease = 0;
		public const bool ReversePolygonOrder = true;

		public const long KTime_Second = 46186158000;

		static FbxProperty GetHeaderProperty(string Creator = "Pop FbxAscii Exporter")
		{
			var Root = new FbxProperty("FBXHeaderExtension");
			Root.AddProperty("FBXHeaderVersion", 1003);
			Root.AddProperty("FBXVersion", Version);
			Root.AddProperty("Creator", Creator);

			//	won't load in unity without this comment at the top
			Root.AddComment("FBX " + VersionMajor + "." + VersionMinor + "." + VersionRelease.ToString("D2") + "  project file");
			return Root;
		}

		static List<int> GetMeshIndexes(int[] Indexes, MeshTopology Topology)
		{
			int PolyIndexCount;
			if (Topology == MeshTopology.Triangles)
				PolyIndexCount = 3;
			else if (Topology == MeshTopology.Quads)
				PolyIndexCount = 4;
			else
				throw new System.Exception("meshes of " + Topology + " are unsupported");

			var FbxIndexes = new List<int>(Indexes.Length);
			var Poly = new List<int>(new int[PolyIndexCount]);

			for (int i = 0; i < Indexes.Length; i += PolyIndexCount)
			{
				for (int p = 0; p < PolyIndexCount; p++)
					Poly[p] = Indexes[i + p];
				//	add in reverse order - imports with reverse winding it seems
				if (ReversePolygonOrder)
					Poly.Reverse();

				//	last one denotes end of polygon and is negative (+1)
				Poly[PolyIndexCount - 1] = -(Poly[PolyIndexCount - 1] + 1);
				FbxIndexes.AddRange(Poly);
			}
			return FbxIndexes;
		}

		static FbxObject CreateAnimLayerObject(FbxObjectManager ObjectManager)
		{
			var Name = "AnimLayer::BaseLayer";
			var Object = ObjectManager.CreateObject("AnimationLayer",Name);
			Object.Definition.AddValue(Object.Ident);
			Object.Definition.AddValue(Name);
			Object.Definition.AddValue("");
			//AnimationLayer: 3445458688, , "AnimLayer::BaseLayer" {
			Object.Definition.AddProperty("Dummy");
			return Object;
		}

		static FbxObject CreateAnimStackObject(AnimStack Anim,FbxObjectManager ObjectManager)
		{
			var Name = "AnimStack::" + Anim.Name;
			var Object = ObjectManager.CreateObject("AnimationStack",Name);
			var Def = Object.Definition;
			Def.AddValue(Object.Ident);
			Def.AddValue(Name);
			Def.AddValue("");
			var Properties = Def.AddProperty("Properties70");

			var LocalStart = Properties.AddProperty("P");
			LocalStart.AddValue("LocalStart");
			LocalStart.AddValue("KTime");
			LocalStart.AddValue("Time");
			LocalStart.AddValue("");
			LocalStart.AddValue(FbxHelper.GetFbxSeconds(Anim.LocalStartTime));

			var LocalStop = Properties.AddProperty("P");
			LocalStop.AddValue("LocalStop");
			LocalStop.AddValue("KTime");
			LocalStop.AddValue("Time");
			LocalStop.AddValue("");
			LocalStop.AddValue(FbxHelper.GetFbxSeconds(Anim.LocalEndTime));

			var ReferenceStart = Properties.AddProperty("P");
			ReferenceStart.AddValue("ReferenceStart");
			ReferenceStart.AddValue("KTime");
			ReferenceStart.AddValue("Time");
			ReferenceStart.AddValue("");
			ReferenceStart.AddValue(FbxHelper.GetFbxSeconds(Anim.ReferenceStartTime));

			var ReferenceStop = Properties.AddProperty("P");
			ReferenceStop.AddValue("ReferenceStop");
			ReferenceStop.AddValue("KTime");
			ReferenceStop.AddValue("Time");
			ReferenceStop.AddValue("");
			ReferenceStop.AddValue(FbxHelper.GetFbxSeconds(Anim.ReferenceEndTime));

			/*
			AnimationStack: 362632224, "AnimStack::Take 001", "" {
			Properties70:
				{
				P: "LocalStart", "KTime", "Time", "",0
				P: "LocalStop", "KTime", "Time", "", 46186158000
				P: "ReferenceStart", "KTime", "Time", "",0
				P: "ReferenceStop", "KTime", "Time", "", 46186158000
			}
			*/
			return Object;
		}

		static FbxObject CreateFbxObject_Material(string MaterialName, FbxObjectManager ObjectManager)
		{
			var Name = "Material::" + MaterialName;
			var Object = ObjectManager.CreateObject("Material",Name);

			var Model = Object.Definition;
			Model.AddValue(Object.Ident);
			Model.AddValue(Name);
			Model.AddValue("");
			return Object;
		}

		static FbxObject CreateFbxObject(Mesh mesh, Matrix4x4 transform, FbxObjectManager ObjectManager,int? ExplicitIdent=null)
		{
			//var Object = ObjectManager.CreateObject(mesh.name);
			//Object.Definition = new FbxProperty("Model");

			var Name = "Model::" + mesh.name;

			FbxObject Object;
			if ( ExplicitIdent.HasValue )
				Object = ObjectManager.CreateObject(ExplicitIdent.Value, "Model", Name);
			else
				Object = ObjectManager.CreateObject( "Model", Name);

			var Model = Object.Definition;
			Model.AddValue(Object.Ident);
			Model.AddValue(Name);
			Model.AddValue("Mesh");

			Model.AddProperty("Version", 232);
			Model.AddProperty("Vertices", new FbxValue_Floats(mesh.vertices, (n) => { return transform.MultiplyPoint(n); }));
			//	indexes start at 1, and last in poly is negative
			var FbxIndexes = GetMeshIndexes(mesh.GetIndices(0), mesh.GetTopology(0));
			Model.AddProperty("PolygonVertexIndex", new FbxValue_Ints(FbxIndexes));
			Model.AddProperty("GeometryVersion", 124);

			int LayerNumber = 0;
			var NormalLayer = Model.AddProperty("LayerElementNormal", LayerNumber);
			NormalLayer.AddProperty("Version", 101);
			NormalLayer.AddProperty("Name", "");
			//	ByPolygon	It means that there is a normal for every polygon of the model.
			//	ByPolygonVertex	It means that there is a normal for every vertex of every polygon of the model.
			//	ByVertex	It means that there is a normal for every vertex of the model.
			//	gr: ByVertex "Unsupported wedge mapping mode type.Please report this bug."
			//		even though I think that's the right one to use.. as ByPolygonVertex looks wrong
			NormalLayer.AddProperty("MappingInformationType", "ByPolygonVertex");
			NormalLayer.AddProperty("ReferenceInformationType", "Direct");
			NormalLayer.AddProperty("Normals", new FbxValue_Floats(mesh.normals, (n) => { return transform.MultiplyVector(n); }));

			var Layer = Model.AddProperty("Layer", LayerNumber);
			Layer.AddProperty("Version", 100);
			var len = Layer.AddProperty("LayerElement");
			len.AddProperty("Type", "LayerElementNormal");
			len.AddProperty("TypedIndex", 0);
			var les = Layer.AddProperty("LayerElement");
			les.AddProperty("Type", "LayerElementSmoothing");
			les.AddProperty("TypedIndex", 0);
			var leuv = Layer.AddProperty("LayerElement");
			leuv.AddProperty("Type", "LayerElementUV");
			leuv.AddProperty("TypedIndex", 0);
			var let = Layer.AddProperty("LayerElement");
			let.AddProperty("Type", "LayerElementTexture");
			let.AddProperty("TypedIndex", 0);
			var lem = Layer.AddProperty("LayerElement");
			lem.AddProperty("Type", "LayerElementMaterial");
			lem.AddProperty("TypedIndex", 0);

			return Object;
		}

		struct NodeAttribute
		{
			public string Name;

			public NodeAttribute(string Name)
			{
				this.Name = Name;
			}
		}

		struct FbxCamera
		{
			public string Name;

			public FbxCamera(string Name)
			{
				this.Name = Name;
			}
		}

		static FbxObject CreateFbxObject(NodeAttribute Attrib, FbxObjectManager ObjectManager)
		{
			var Name = "NodeAttribute::" + Attrib.Name;

			FbxObject Object;
			Object = ObjectManager.CreateObject("NodeAttribute", Name);

			var Model = Object.Definition;
			Model.AddValue(Object.Ident);
			Model.AddValue(Name);
			Model.AddValue("Camera");
		
			Model.AddProperty("TypeFlags", "Camera");
			Model.AddProperty("GeometryVersion", 124);
			Model.AddProperty("Position", Vector3.zero);
			Model.AddProperty("Up", Vector3.up);
			Model.AddProperty("LookAt", Vector3.forward);

			return Object;
		}

		static FbxObject CreateFbxObject(FbxCamera mesh, Matrix4x4 transform, FbxObjectManager ObjectManager,FbxConnectionManager ConnectionManager)
		{
			var Name = "Model::" + mesh.Name;

			FbxObject Object;
			Object = ObjectManager.CreateObject("Model", Name);

			var Model = Object.Definition;
			Model.AddValue(Object.Ident);
			Model.AddValue(Name);
			Model.AddValue("Camera");

			Model.AddProperty("Version", 232);

			var Properties = Model.AddProperty("Properties70");
			var DefaultAttributeIndex = Properties.AddProperty("P");
			DefaultAttributeIndex.AddValue("DefaultAttributeIndex");
			DefaultAttributeIndex.AddValue("int");
			DefaultAttributeIndex.AddValue("Integer");
			DefaultAttributeIndex.AddValue("");
			DefaultAttributeIndex.AddValue(0);

			var LclTranslation = Properties.AddProperty("P");
			LclTranslation.AddValue("Lcl Translation");
			LclTranslation.AddValue("");
			LclTranslation.AddValue("A+");
			LclTranslation.AddValue(Vector3.zero);

			var LclRotation = Properties.AddProperty("P");
			LclRotation.AddValue("Lcl Rotation");
			LclRotation.AddValue("");
			LclRotation.AddValue("A+");
			LclRotation.AddValue(Vector3.zero);

			var LclScaling = Properties.AddProperty("P");
			LclScaling.AddValue("Lcl Scaling");
			LclScaling.AddValue("");
			LclScaling.AddValue("A+");
			LclScaling.AddValue(Vector3.one);


			//	need a camera attrib and need it connected
			var CameraAttrib = new NodeAttribute(mesh.Name);
			var AttribObject = CreateFbxObject(CameraAttrib, ObjectManager);
			ConnectionManager.Add(new FbxConnection(AttribObject, Object, FbxRelationType.OO));

			return Object;
		}

		static FbxProperty GetDefinitionsProperty(FbxObjectManager ObjectManager)
		{
			var Defs = new FbxProperty("Definitions");
			Defs.AddProperty("Version", 100);

			Defs.AddProperty("Count", ObjectManager.Objects.Count);

			//	get types
			var TypeCounts = new Dictionary<string, int>();
			foreach (var Object in ObjectManager.Objects)
			{
				var TypeName = Object.TypeName;
				if (!TypeCounts.ContainsKey(TypeName))
					TypeCounts.Add(TypeName, 1);
				else
					TypeCounts[TypeName]++;
			}

			foreach (var TypeCount in TypeCounts)
			{
				var ot = Defs.AddProperty("ObjectType", TypeCount.Key);
				ot.AddProperty("Count", TypeCount.Value);
			}
			return Defs;
		}


		public static string GetIndent(int Indents)
		{
			var String = "";
			for (int i = 0; i < Indents; i++)
				String += "\t";
			return String;
		}

		public static void Export(System.Action<string> WriteLine, FbxProperty Property, int Indent = 0)
		{
			var IndentStr = GetIndent(Indent);
			foreach (var Comment in Property.Comments)
				WriteLine(IndentStr + Tag_Comment + Comment);

			var ValuesLine = IndentStr + Property.Name + ": ";
			var Values = Property.Values;

			for (int i = 0; i < Values.Count; i++)
			{
				var Value = Values[i];
				if (i > 0)
					ValuesLine += PropertySeperator;
				ValuesLine += Value.GetString();
			}

			WriteLine(ValuesLine);

			//	open tree
			if (Property.Children != null)
			{
				WriteLine(IndentStr + "{");
				foreach (var Child in Property.Children)
					Export(WriteLine, Child, Indent + 1);
				WriteLine(IndentStr + "}");
			}
		}

		public static void Export(System.Action<string> WriteLine, List<FbxProperty> Tree, List<string> Comments = null)
		{
			Pop.AllocIfNull(ref Comments);
			Comments.Add("Using WIP PopX.FbxAscii exporter from @soylentgraham");
			foreach (var Comment in Comments)
				WriteLine(Tag_Comment + " " + Comment);
			WriteLine(null);

			//	write out the tree
			foreach (var Prop in Tree)
			{
				Export(WriteLine, Prop);
				WriteLine(null);
			}
		}

		static void Export(System.Action<string> WriteLine, FbxObjectManager ObjectManager)
		{
			var ObjectPropertys = new FbxProperty("Objects");
			foreach (var Object in ObjectManager.Objects)
			{
				ObjectPropertys.AddProperty(Object.Definition);
			}
			Export(WriteLine, ObjectPropertys);
		}

		enum FbxConnectionType
		{
			//Connect,	not valid in 7.5.00
			C
		};
		enum FbxRelationType
		{
			OO,	//	object to object
			OP	//	object to property
		};

		static void Export(System.Action<string> WriteLine, FbxConnectionManager ConnectionsManager)
		{
			var ConnectionsProp = new FbxProperty("Connections");
			foreach (var Connection in ConnectionsManager.Connections)
			{
				//	meshes need "Connect", anims need "C"
				//	id vs name?
				var ConnectionProp = ConnectionsProp.AddProperty(Connection.ConnectionType.ToString());

				var Desc = string.Format("{0}::{1} -> {2}::{3}", Connection.type1, Connection.name1, Connection.type2, Connection.name2);
				ConnectionProp.AddComment("");
				ConnectionProp.AddComment(Desc);

				ConnectionProp.AddValue(Connection.Relation.ToString());

				/*
				if (Connection.ConnectionType == FbxConnectionType.Connect)
				{
					ConnectionProp.AddValue(Connection.name1);
					ConnectionProp.AddValue(Connection.name2);
					//	"Model::" + ((Mesh)Value).name);
					//	"Material::" + ((Material)Value).name);
				}
				else */if(Connection.ConnectionType == FbxConnectionType.C)
				{
					ConnectionProp.AddValue(Connection.Object1.Ident);
					ConnectionProp.AddValue(Connection.Object2.Ident);
				}

				if (Connection.PropertyName != null)
					ConnectionProp.AddValue(Connection.PropertyName);
			}
			Export(WriteLine, ConnectionsProp);
		}

		public struct AnimStack
		{
			public string Name;
			public float LocalStartTime;
			public float LocalEndTime;
			public float ReferenceStartTime { get { return LocalStartTime; } }
			public float ReferenceEndTime { get { return LocalEndTime; } }

			public AnimStack(string Name,float StartTime,float EndTime)
			{
				this.Name = Name;
				this.LocalEndTime = EndTime;
				this.LocalStartTime = StartTime;
			}
		}

		public static void Export(System.Action<string> WriteLine, Mesh mesh, Matrix4x4 transform, List<string> Comments = null)
		{
			//	temp objects
			var MeshAnim = new AnimObject();
			MeshAnim.AddFrame(new Vector3(0,0,0), Quaternion.identity, 0);
			MeshAnim.AddFrame(new Vector3(0, 1000, 0), Quaternion.Euler(0, 90, 0), 1);
			MeshAnim.AddFrame(new Vector3(0, 0, 0), Quaternion.Euler(0, 180, 0), 2);
			MeshAnim.AddFrame(new Vector3(0, 1000, 0), Quaternion.Euler(0, 270, 0), 3);
			MeshAnim.AddFrame(new Vector3(0, 0, 0), Quaternion.Euler(0, 359, 0), 4);

			var AnimStack = new AnimStack("Take001", MeshAnim.GetStartTime(), MeshAnim.GetEndTime());

			var Cam = new FbxCamera("Camera1");

			
			Pop.AllocIfNull(ref Comments);
			Comments.Add("Using WIP PopX.FbxAscii exporter from @soylentgraham");

			var Header = GetHeaderProperty();
			Header.AddComment(Comments);
			Export(WriteLine, Header);

			var ConnectionManager = new FbxConnectionManager();
			var ObjectManager = new FbxObjectManager();

			//var MeshObject = CreateFbxObject(mesh, transform, ObjectManager);
			var CameraObject = CreateFbxObject(Cam, transform, ObjectManager, ConnectionManager);
			var AnimLayerObject = CreateAnimLayerObject(ObjectManager);
			var AnimStackObject = CreateAnimStackObject(AnimStack,ObjectManager);


			MakeAnimationNode(MeshAnim, CameraObject, AnimLayerObject, ObjectManager, ConnectionManager);

			ConnectionManager.Add(new FbxConnection(AnimLayerObject.TypeName, AnimLayerObject.ObjectName, AnimLayerObject, AnimStackObject.TypeName, AnimStackObject.ObjectName, AnimStackObject, FbxRelationType.OO));



			var Definitions = GetDefinitionsProperty(ObjectManager);
			Export(WriteLine, Definitions);

			Export(WriteLine, ObjectManager);



			//	fake connections after we've exported ObjectManager
			var SceneMesh = new Mesh();
			SceneMesh.name = "Root";
			var SceneMeshObject = CreateFbxObject(SceneMesh, Matrix4x4.identity, ObjectManager, FbxObjectManager.RootNodeIdent);
			var MeshMaterialObject = CreateFbxObject_Material("DummyMaterial", ObjectManager);

			//ConnectionManager.Add(new FbxConnection( MeshObject, SceneMeshObject, FbxRelationType.OO));
			ConnectionManager.Add(new FbxConnection( CameraObject, SceneMeshObject, FbxRelationType.OO));
			//ConnectionManager.Add(new FbxConnection( MeshMaterialObject.TypeName, MeshMaterialObject.ObjectName, MeshMaterialObject, MeshObject.TypeName, MeshObject.ObjectName, MeshObject, FbxRelationType.OO));


			Export(WriteLine, ConnectionManager);
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


		class FbxObject
		{
			public int Ident;
			public FbxProperty Definition;  //	property that goes in objects
			public string TypeName	{ get { return Definition.Name; }}

			public string ExplicitObjectName = null;
			public string ObjectName	
			{
				get
				{
					if (ExplicitObjectName != null)
						return ExplicitObjectName;
					return "";
				}
			}


			public FbxObject(int Ident,string TypeName,string ObjectName=null)
			{
				this.ExplicitObjectName = ObjectName;
				this.Ident = Ident;
				this.Definition = new FbxProperty(TypeName);
			}
		}

		static string GetTypeString(FbxAnimationCurveNodeType Type)
		{
			switch (Type)
			{
				case FbxAnimationCurveNodeType.Translation: return "T";
				case FbxAnimationCurveNodeType.Rotation: return "R";
				case FbxAnimationCurveNodeType.Scale: return "S";
				case FbxAnimationCurveNodeType.Visibility: return "Visibility";
				default: throw new System.Exception("Unknown type " + Type);
			}
		}

		class FbxObjectManager
		{
			public List<FbxObject> Objects = new List<FbxObject>();
			int IdentCounter = 6000;
			public const int RootNodeIdent = 0;

			int AllocIdent()
			{
				IdentCounter++;
				return IdentCounter;
			}

			public FbxObject CreateObject(string TypeName, string ObjectName = null)
			{
				return CreateObject(AllocIdent(), TypeName, ObjectName);
			}

			public FbxObject CreateObject(int ExplicitIdent,string TypeName, string ObjectName = null)
			{
				var Node = new FbxObject(ExplicitIdent, TypeName, ObjectName);
				Objects.Add(Node);
				return Node;
			}



			public FbxObject AddAnimationCurveNode(FbxAnimationCurveNodeType NodeType,Vector3 DefaultValue)
			{
				//	note no name. matters?
				var Node = new FbxObject(AllocIdent(),"AnimationCurveNode");
				Objects.Add(Node);

				//string nodeData = inputId + ", \"AnimCurveNode::" + curveTypeStr + "\", \"\"";
				//FbxDataNode animCurveNode = new FbxDataNode(nodeName, nodeData, 1);
				string CurveTypeStr = GetTypeString(NodeType);
				Node.Definition.AddValue(Node.Ident);
				Node.Definition.AddValue("AnimCurveNode::" + CurveTypeStr);
				Node.Definition.AddValue("");

				//FbxDataNode propertiesNode = new FbxDataNode("Properties70", "", 2);
				//animCurveNode.addSubNode(propertiesNode);
				var PropertiesNode = Node.Definition.AddProperty("Properties70");
				//propertiesNode.addSubNode(new FbxDataNode("P", "\"d|X\", \"Number\", \"\", \"A\"," + initData.x, 3));
				//propertiesNode.addSubNode(new FbxDataNode("P", "\"d|Y\", \"Number\", \"\", \"A\"," + initData.y, 3));
				//propertiesNode.addSubNode(new FbxDataNode("P", "\"d|Z\", \"Number\", \"\", \"A\"," + initData.z, 3));
				var px = PropertiesNode.AddProperty("P");
				px.AddValue("d|X");
				px.AddValue("Number");
				px.AddValue("");
				px.AddValue("A");
				px.AddValue(DefaultValue.x);

				var py = PropertiesNode.AddProperty("P");
				py.AddValue("d|Y");
				py.AddValue("Number");
				py.AddValue("");
				py.AddValue("A");
				py.AddValue(DefaultValue.y);


				var pz = PropertiesNode.AddProperty("P");
				pz.AddValue("d|Z");
				pz.AddValue("Number");
				pz.AddValue("");
				pz.AddValue("A");
				pz.AddValue(DefaultValue.z);

				// release memory
				//animCurveNode.saveDataOnDisk(saveFileFolder);
				//objMainNode.addSubNode(animCurveNode);


				return Node;
			}

			public FbxObject AddAnimationCurve(float[] CurveDatas,float[] CurveTimes)
			{
				//	todo: use proper time!
				var TimeDatas = new List<long>();
				for (int i = 0; i < CurveDatas.Length; i++)
				{
					TimeDatas.Add(FbxHelper.GetFbxSeconds(CurveTimes[i]));
				}

				//	add a new object
				var CurveNodeObj = new FbxObject(AllocIdent(), "AnimationCurve");
				Objects.Add(CurveNodeObj);
				var CurveNode = CurveNodeObj.Definition;
				CurveNode.AddValue(CurveNodeObj.Ident);
				CurveNode.AddValue("AnimCurve::");	//	name
				CurveNode.AddValue("");

				//AnimationCurve: 106102887970656, "AnimCurve::", "" 
				//string nodeData = inputId + ", \"AnimCurve::\", \"\"";
				//FbxDataNode curveNode = new FbxDataNode("AnimationCurve", nodeData, 1);


				CurveNode.AddProperty("Default", 0);
				CurveNode.AddProperty("KeyVer", 4008);

				var keyTimeNode = CurveNode.AddProperty("KeyTime");
				keyTimeNode.AddValue("*" + CurveDatas.Length);
				keyTimeNode.AddProperty("a",new FbxValue_Ints(TimeDatas));
				//FbxDataNode keyTimeNode = new FbxDataNode("KeyTime", "*" + dataLengthStr, 2);
				//keyTimeNode.addSubNode("a", timeArrayDataStr);
				//curveNode.addSubNode(keyTimeNode);

				var keyValuesNode = CurveNode.AddProperty("KeyValueFloat");
				keyValuesNode.AddValue("*" + CurveDatas.Length);
				keyValuesNode.AddProperty("a",new FbxValue_Floats(CurveDatas));
				//var keyValuesNode = new FbxDataNode("KeyValueFloat", "*" + dataLengthStr, 2);
				//keyValuesNode.addSubNode("a", keyValueFloatDataStr);
				//curveNode.addSubNode(keyValuesNode);

				//curveNode.addSubNode(";KeyAttrFlags", "Cubic|TangeantAuto|GenericTimeIndependent|GenericClampProgressive");
				var keyAttrFlagsNode = CurveNode.AddProperty("KeyAttrFlags");
				keyAttrFlagsNode.AddComment("KeyAttrFlags = Cubic | TangeantAuto | GenericTimeIndependent | GenericClampProgressive");
				//FbxDataNode keyAttrFlagsNode = new FbxDataNode("KeyAttrFlags", "*1", 2);
				keyAttrFlagsNode.AddValue("*1");
				keyAttrFlagsNode.AddProperty("a", "24840");
				//curveNode.addSubNode(keyAttrFlagsNode);

				var keyRefCountNode = CurveNode.AddProperty("KeyAttrRefCount");
				keyRefCountNode.AddValue("*1");
				//FbxDataNode keyRefCountNode = new FbxDataNode("KeyAttrRefCount", "*1", 2);
				keyRefCountNode.AddProperty("a", CurveDatas.Length);
				//keyRefCountNode.addSubNode("a", dataLengthStr);
				//curveNode.addSubNode(keyRefCountNode);

				//	objects.add curvenode
				//return curveNode;
			
				return CurveNodeObj;
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
			 */public FbxConnectionType ConnectionType{ get { return FbxConnectionType.C; }}
			public FbxRelationType Relation;

			public FbxObject Object1;
			public string type1 { get { return Object1.TypeName; } }
			public string name1 { get { return Object1.ObjectName; } }
			public FbxObject Object2;
			public string type2 { get { return Object2.TypeName; } }
			public string name2 { get { return Object2.ObjectName; } }

			public string PropertyName;

			public FbxConnection(string type1, string name1, FbxObject Object1, string type2, string name2, FbxObject Object2, FbxRelationType Relation, string PropertyName = null)
				:this(Object1, Object2, Relation, PropertyName)
			{
			}

			public FbxConnection(FbxObject Object1,FbxObject Object2, FbxRelationType Relation,string PropertyName=null)
			{

				this.Object1 = Object1;

				this.Object2 = Object2;

				this.Relation = Relation;

				if (Relation == FbxRelationType.OP)
					if (string.IsNullOrEmpty(PropertyName))
						throw new System.Exception("Connection object to property, missing property name");
				this.PropertyName = PropertyName;
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
			if ( AnimNodePosition != null )
				ConnectionManager.Add( new FbxConnection("AnimCurveNode", "T", AnimNodePosition, "Model", AnimatedObject.TypeName, AnimatedObject, FbxRelationType.OP, "Lcl Translation"));
		
			if (AnimNodeRotation != null)
				ConnectionManager.Add( new FbxConnection("AnimCurveNode", "R", AnimNodeRotation, "Model", AnimatedObject.TypeName, AnimatedObject, FbxRelationType.OP, "Lcl Rotation"));

			if (AnimNodeScale != null)
				ConnectionManager.Add( new FbxConnection("AnimCurveNode", "S", AnimNodeScale, "Model", AnimatedObject.TypeName, AnimatedObject, FbxRelationType.OP, "Lcl Scaling"));
		}


		static void MakeAnimationNode(AnimObject Anim,FbxObject AnimLayer,FbxObjectManager ObjectManager,FbxConnectionManager ConnectionManager, out FbxObject AnimNodePosition, out FbxObject AnimNodeRotation, out FbxObject AnimNodeScale)
		{
			// add anim nodes
			var ao = Anim;
			float[] TimeData;
			ao.GetCurveTimes(out TimeData);

			bool MakeTranslation = true;
			bool MakeRotation = false;
			bool MakeScale = false;


			if (MakeTranslation)
			{
				var NodeT = ObjectManager.AddAnimationCurveNode(FbxAnimationCurveNodeType.Translation, ao.Frames[0].Position);
				AnimNodePosition = NodeT;
				//	get data
				float[] TXData;
				float[] TYData;
				float[] TZData;
				ao.GetPositionCurveData(out TXData, out TYData, out TZData);
				var CurveTX = ObjectManager.AddAnimationCurve(TXData, TimeData);
				var CurveTY = ObjectManager.AddAnimationCurve(TYData, TimeData);
				var CurveTZ = ObjectManager.AddAnimationCurve(TZData, TimeData);

				ConnectionManager.Add(new FbxConnection( "AnimCurveNode", "T", NodeT, "AnimLayer", "BaseLayer", AnimLayer, FbxRelationType.OO));
				ConnectionManager.Add(new FbxConnection( "AnimCurve", "", CurveTX, "AnimCurveNode", "T", NodeT, FbxRelationType.OP, "d|X"));
				ConnectionManager.Add(new FbxConnection( "AnimCurve", "", CurveTY, "AnimCurveNode", "T", NodeT, FbxRelationType.OP, "d|Y"));
				ConnectionManager.Add(new FbxConnection( "AnimCurve", "", CurveTZ, "AnimCurveNode", "T", NodeT, FbxRelationType.OP, "d|Z"));
			}
			else
			{
				AnimNodePosition = null;
			}


			if (MakeRotation)
			{
				var NodeR = ObjectManager.AddAnimationCurveNode(FbxAnimationCurveNodeType.Rotation, ao.Frames[0].RotationEular);
				AnimNodeRotation = NodeR;
				float[] RXData;
				float[] RYData;
				float[] RZData;
				ao.GetRotationCurveData(out RXData, out RYData, out RZData);
				var CurveRX = ObjectManager.AddAnimationCurve(RXData, TimeData);
				var CurveRY = ObjectManager.AddAnimationCurve(RYData, TimeData);
				var CurveRZ = ObjectManager.AddAnimationCurve(RZData, TimeData);

				ConnectionManager.Add(new FbxConnection( "AnimCurveNode", "R", NodeR, "AnimLayer", "BaseLayer", AnimLayer, FbxRelationType.OO));
				ConnectionManager.Add(new FbxConnection( "AnimCurve", "", CurveRX, "AnimCurveNode", "R", NodeR, FbxRelationType.OP, "d|X"));
				ConnectionManager.Add(new FbxConnection( "AnimCurve", "", CurveRY, "AnimCurveNode", "R", NodeR, FbxRelationType.OP, "d|Y"));
				ConnectionManager.Add(new FbxConnection( "AnimCurve", "", CurveRZ, "AnimCurveNode", "R", NodeR, FbxRelationType.OP, "d|Z"));
			}
			else
			{
				AnimNodeRotation = null;
			}


			if (MakeScale)
			{
				var NodeS = ObjectManager.AddAnimationCurveNode(FbxAnimationCurveNodeType.Scale, ao.Frames[0].Scale);
				AnimNodeScale = NodeS;
				float[] SXData;
				float[] SYData;
				float[] SZData;
				ao.GetScaleCurveData(out SXData, out SYData, out SZData);
				var CurveSX = ObjectManager.AddAnimationCurve(SXData, TimeData);
				var CurveSY = ObjectManager.AddAnimationCurve(SYData, TimeData);
				var CurveSZ = ObjectManager.AddAnimationCurve(SZData, TimeData);

				ConnectionManager.Add(new FbxConnection( "AnimCurveNode", "S", NodeS, "AnimLayer", "BaseLayer", AnimLayer, FbxRelationType.OO));
				ConnectionManager.Add(new FbxConnection( "AnimCurve", "", CurveSX, "AnimCurveNode", "S", NodeS, FbxRelationType.OP, "d|X"));
				ConnectionManager.Add(new FbxConnection( "AnimCurve", "", CurveSY, "AnimCurveNode", "S", NodeS, FbxRelationType.OP, "d|Y"));
				ConnectionManager.Add(new FbxConnection( "AnimCurve", "", CurveSZ, "AnimCurveNode", "S", NodeS, FbxRelationType.OP, "d|Z"));
			}
			else
			{
				AnimNodeScale = null;
			}
		}


	}
}


