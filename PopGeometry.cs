using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class UnityEvent_ListOfLine3 : UnityEngine.Events.UnityEvent <List<PopX.Line3>> {}


//	rename namespace to Pop in later refactor
namespace PopX
{

	//	gr: should this be in Pop.Geometry.Line3 ?
	[System.Serializable]
	public struct Line3
	{
		public Vector3	Start;
		public Vector3	End;

		public Line3(Vector3 _Start, Vector3 _End)
		{ 
			Start = _Start;
			End = _End;
		}

	};


	//	gr: should this be in Pop.Geometry.Sphere3 ?
	[System.Serializable]
	public struct Sphere3
	{
		public Vector3	center;
		public float	radius;
		public Vector3	radius3	{	get{	return new Vector3( radius, radius, radius );	}}

		public Sphere3(Vector3 _Center, float _Radius)
		{ 
			center = _Center;
			radius = _Radius;
		}

		//	negative - inside sphere, positive, distance from sphere.
		public float Distance(Ray ray)
		{
			Vector3 Temp;
			return Distance (ray, out Temp);
		}

		//	negative - inside sphere, positive, distance from sphere.
		public float Distance(Ray ray,out Vector3 NearestPoint)
		{
			NearestPoint = PopMath.NearestToRay3 (this.center, ray);
			var dist = Vector3.Distance (NearestPoint, this.center);
			dist -= this.radius;
			return dist;
		}

		public bool	IsPointInside(Vector3 Position)
		{
			return Distance (Position) <= 0;
		}

		public float Distance(Vector3 Position)
		{
			var Dist = Vector3.Distance (Position, this.center );
			Dist -= this.radius;
			return Dist;
		}

		public void Transform(Matrix4x4 Transform)
		{
			var EdgePos = center + new Vector3(0, 0, radius);

			//	transform
			center = Transform.MultiplyPoint(center);

			//	find new edge
			var WorldEdgePos = Transform.MultiplyPoint(EdgePos);

			radius = Vector3.Distance(WorldEdgePos, center);
		}
	};


	public static class Geometry 
	{
		//	gr: this value is NOT uInt16.MaxValue/65535 as we'd expect. I can't remember why, but it's something deep inside unity. maybe it uses some magic numbers for other things
		//		so just impose a vaguely similar limit
		const int Max16BitTriangleIndexes = 65000;
		
		#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Unshare triangle indexes of mesh")]
		public static void UnshareTrianglesOfMesh (MenuCommand menuCommand) 
		{
			var mf = menuCommand.context as MeshFilter;
			var mesh = mf.sharedMesh;

			Undo.RecordObject(mesh, "Unshare Triangles Of Mesh " + mesh.name);
			UnshareTrianglesOfMesh (ref mesh);
			Undo.FlushUndoRecordObjects ();
		}
		#endif

		#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Unshare triangle indexes to new mesh...")]
		public static void UnshareTrianglesToMesh (MenuCommand menuCommand) 
		{
			var mf = menuCommand.context as MeshFilter;
			var m = CopyMesh(mf.sharedMesh);
			m.name = mf.sharedMesh.name + " unshared";
			UnshareTrianglesOfMesh (ref m);
			SaveMesh(m, m.name, true);
		}
		#endif

		#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Weld vertexes of mesh")]
		public static void _WeldVertexesOfMesh(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var mesh = mf.sharedMesh;

			Undo.RecordObject(mesh, "Unshare Triangles Of Mesh " + mesh.name);
			WeldVertexesOfMesh(ref mesh);
			Undo.FlushUndoRecordObjects();
		}
#endif

#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Weld vertexes to new mesh...")]
		public static void _WeldVertexesOfNewMesh(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var m = CopyMesh(mf.sharedMesh);
			m.name = mf.sharedMesh.name + " welded";
			WeldVertexesOfMesh(ref m);
			SaveMesh(m, m.name, true);
		}
#endif

		#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Randomise triangle order")]
		public static void _RandomiseTriangleOrder (MenuCommand menuCommand) 
		{
			var mf = menuCommand.context as MeshFilter;

			var mesh = mf.sharedMesh;
			Undo.RecordObject(mesh, "Randomised triangle order of " + mesh.name);
			RandomiseTriangleOrder (mesh);
			Undo.FlushUndoRecordObjects ();
		}
		#endif


		#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Set mesh UV[3] to vertex index")]
		public static void _SetMeshUV5ToVertexIndex(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var mesh = mf.sharedMesh;

			Undo.RecordObject(mesh, "Set mesh UV3 to vertex index of " + mesh.name);
			SetMeshUVToVertexIndex(mesh, 3);
			Undo.FlushUndoRecordObjects();
		}
		#endif

		#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Set mesh UV[0] to triangle index")]
		public static void _SetMeshUV0ToTriangleIndex (MenuCommand menuCommand) 
		{
			var mf = menuCommand.context as MeshFilter;
			var mesh = mf.sharedMesh;

			Undo.RecordObject(mesh, "Set mesh UV0 to triangle index of " + mesh.name);
			SetMeshUVToTriangleIndex (mesh,0);
			Undo.FlushUndoRecordObjects ();
		}
		#endif

		#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Set mesh UV1 to triangle barycentric coords")]
		public static void SetMeshUV1ToTriangleBarycentricCoords (MenuCommand menuCommand) 
		{
			var mf = menuCommand.context as MeshFilter;
			var m = mf.sharedMesh;

			var TriangleIndexes = m.triangles;
			if (TriangleIndexes.Length != m.vertexCount)
			{
				var DialogResult = EditorUtility.DisplayDialog("Error", "Assigning triangle indexes to attributes probably won't work as expected for meshes sharing vertexes.", "Continue", "Cancel");
				if (!DialogResult)
					throw new System.Exception("Aborted assignment of UV triangle indexes");
			}

			using (var Progress = new ScopedProgressBar ("Setting UVs")) {
				var Uvs = new Vector3[m.vertexCount];
				{
					var barya = new Vector3 (1,0,0);
					var baryb = new Vector3 (0,1,0);
					var baryc = new Vector3 (0,0,1);
					for (int t = 0;	t < TriangleIndexes.Length;	t += 3) {

						Progress.SetProgress ("Setting UV of triangle", t / 3, TriangleIndexes.Length / 3, 100);

						Uvs [TriangleIndexes [t + 0]] = barya;
						Uvs [TriangleIndexes [t + 1]] = baryb;
						Uvs [TriangleIndexes [t + 2]] = baryc;
					}
				}
				m.SetUVs( 1, new List<Vector3>(Uvs) );
				m.UploadMeshData (true);
				AssetDatabase.SaveAssets ();
			}
		}
		#endif


		#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Set mesh UV2 to random per-triangle")]
		public static void SetMeshUV2ToRandomPerTriangle (MenuCommand menuCommand) 
		{
			var mf = menuCommand.context as MeshFilter;
			var m = mf.sharedMesh;

			var TriangleIndexes = m.triangles;
			if (TriangleIndexes.Length != m.vertexCount) {
				var DialogResult = EditorUtility.DisplayDialog ("Error", "Assigning triangle indexes to attributes probably won't work as expected for meshes sharing vertexes.", "Continue", "Cancel");
				if (!DialogResult)
					throw new System.Exception ("Aborted assignment of UV triangle indexes");
			}

			using (var Progress = new ScopedProgressBar ("Setting UVs")) {
				var Uvs = new Vector3[m.vertexCount];
				{
					var random3 = new Vector3 (0,0,0);
					for (int t = 0;	t < TriangleIndexes.Length;	t += 3) {

						Progress.SetProgress ("Setting UV of triangle", t / 3, TriangleIndexes.Length / 3, 100);

						float tTime = t / (float)TriangleIndexes.Length;
						random3.x = Mathf.PerlinNoise (tTime, 0.0f);
						random3.y = Mathf.PerlinNoise (0.0f, tTime);
						random3.z = Random.Range (0.0f, 1.0f);

						Uvs [TriangleIndexes [t + 0]] = random3;
						Uvs [TriangleIndexes [t + 1]] = random3;
						Uvs [TriangleIndexes [t + 2]] = random3;
					}
				}
				m.SetUVs( 2, new List<Vector3>(Uvs) );
				m.UploadMeshData (true);
				AssetDatabase.SaveAssets ();
			}
		}
#endif


#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Set mesh bounds to Box Collider")]
		public static void SetMeshBoundsToBoxCollider(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var m = mf.sharedMesh;
			var bc = mf.GetComponent<BoxCollider>();

			m.bounds = bc.bounds;

			//	save the asset, if this is not asset-backed, it will prompt to create a new asset and return that
			m = AssetWriter.SaveAsset(m);
			mf.sharedMesh = m;
		}
#endif

#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Set Box Collider to Mesh Bounds", true)]
		public static bool SetBoxColliderToMeshBounds_Verify(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var bc = mf.GetComponent<BoxCollider>();
			return bc != null;
		}

		[MenuItem("CONTEXT/MeshFilter/Set Box Collider to Mesh Bounds")]
		public static void SetBoxColliderToMeshBounds(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var m = mf.sharedMesh;
			var bc = mf.GetComponent<BoxCollider>();

			bc.size = m.bounds.size;
			bc.center = m.bounds.center;
		}
#endif

#if UNITY_EDITOR
		[MenuItem("CONTEXT/BoxCollider/Set Box Collider to Mesh Bounds",true)]
		public static bool SetBoxColliderToMeshBounds2_Verify(MenuCommand menuCommand)
		{
			var bc = menuCommand.context as BoxCollider;
			var mf = bc.GetComponent<MeshFilter>();
			return mf != null;
		}
		[MenuItem("CONTEXT/BoxCollider/Set Box Collider to Mesh Bounds")]
		public static void SetBoxColliderToMeshBounds2(MenuCommand menuCommand)
		{
			var bc = menuCommand.context as BoxCollider;
			var mf = bc.GetComponent<MeshFilter>();
			var m = mf.sharedMesh;

			bc.size = m.bounds.size;
			bc.center = m.bounds.center;
		}
#endif



#if UNITY_EDITOR
		[MenuItem("Assets/Mesh/Save as new asset...", true)]
		public static bool MeshSaveAsNewAsset_Verify()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			return Meshes.Length > 0;
		}
		[MenuItem("Assets/Mesh/Save as new asset...")]
		public static void MeshSaveAsNewAsset()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			foreach (var Mesh in Meshes)
			{
				AssetWriter.SaveAsNewAsset(Mesh);
			}
		}
		[MenuItem("CONTEXT/MeshFilter/Save as new asset...")]
		public static void MeshSaveAsNewAsset(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var m = mf.sharedMesh;
			AssetWriter.SaveAsNewAsset(m);
		}
#endif

#if UNITY_EDITOR
		[MenuItem("Assets/Mesh/Center mesh on bounds", true)]
		public static bool MeshCenterPivotViaBounds_Verify()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			return Meshes.Length > 0;
		}
		[MenuItem("Assets/Mesh/Center pivot via bounds")]
		public static void MeshCenterPivotViaBounds()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			foreach (var Mesh in Meshes)
			{
				Mesh.CenterOnBounds();
				AssetWriter.SaveAsset(Mesh);
			}
		}
		[MenuItem("CONTEXT/MeshFilter/Center mesh pivot via bounds")]
		public static void MeshCenterPivotViaBounds(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var m = mf.sharedMesh;
			m.CenterOnBounds();
			AssetWriter.SaveAsset(m);
		}
#endif

#if UNITY_EDITOR
		[MenuItem("Assets/Mesh/Recalculate bounds", true)]
		public static bool MeshRecalculateBounds_Verify()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			return Meshes.Length > 0;
		}
		[MenuItem("Assets/Mesh/Recalculate bounds")]
		public static void MeshRecalculateBounds()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			foreach (var Mesh in Meshes)
			{
				Mesh.RecalculateBounds();
				AssetWriter.SaveAsset(Mesh);
			}
		}
		[MenuItem("CONTEXT/MeshFilter/Recalculate mesh bounds")]
		public static void MeshRecalculateBounds(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var m = mf.sharedMesh;
			m.RecalculateBounds();
			AssetWriter.SaveAsset(m);
		}
#endif

#if UNITY_EDITOR
		const string MergeSubmeshMenuLabel = "Merge Submeshes, uv[1].x=index";
		[MenuItem("Assets/Mesh/"+MergeSubmeshMenuLabel, true)]
		public static bool MergeSubmeshMenu_Verify()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			return Meshes.Length > 0;
		}
		[MenuItem("Assets/Mesh/"+MergeSubmeshMenuLabel)]
		public static void MergeSubmeshMenu()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			foreach (var mesh in Meshes)
			{
				//	gr: undo is nice, but SOOO slow on meshes
				//Undo.RecordObject(mesh, MergeSubmeshMenuLabel + " of " + mesh.name);
				MergeSubmeshes(mesh, 1);
				//Undo.FlushUndoRecordObjects();
				AssetWriter.SaveAsset(mesh);
			}
		}
		[MenuItem("CONTEXT/MeshFilter/"+MergeSubmeshMenuLabel)]
		public static void MergeSubmeshMenu(MenuCommand menuCommand)
		{
			var mf = menuCommand.context as MeshFilter;
			var mesh = mf.sharedMesh;

			//	gr: undo is nice, but SOOO slow on meshes
			//Undo.RecordObject(mesh, MergeSubmeshMenuLabel + " of " + mesh.name);
			MergeSubmeshes(mesh, 1);
			//Undo.FlushUndoRecordObjects();
			AssetWriter.SaveAsset(mesh);
		}


		[MenuItem("Assets/Mesh/Merge Submeshes 0,1,2,3,4,5,12", true)]
		public static bool MergeSubmesh_0_1_2_3_4_5_12_Menu_Verify()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			return Meshes.Length > 0;
		}
		[MenuItem("Assets/Mesh/Merge Submeshes 0,1,2,3,4,5,12")]
		public static void MergeSubmesh_0_1_2_3_4_5_12_Menu()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			foreach (var mesh in Meshes)
			{
				var NewMesh = ExtractSubmeshesAsMesh(mesh, new int[] { 0, 1, 2, 3, 4, 5, 12 });
				AssetWriter.SaveAsset(NewMesh);
			}
		}

		[MenuItem("Assets/Mesh/Merge Submeshes 6,7,8,9,10,11", true)]
		public static bool MergeSubmesh_6_7_8_9_10_11_Menu_Verify()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			return Meshes.Length > 0;
		}
		[MenuItem("Assets/Mesh/Merge Submeshes 6,7,8,9,10,11")]
		public static void MergeSubmesh_6_7_8_9_10_11_Menu()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			foreach (var mesh in Meshes)
			{
				var NewMesh = ExtractSubmeshesAsMesh(mesh, new int[] { 6, 7, 8, 9, 10, 11 });
				AssetWriter.SaveAsset(NewMesh);
			}
		}


		[MenuItem("Assets/Mesh/Merge Submeshes 2,12,10,5,7,11,4", true)]
		public static bool MergeSubmesh_2_12_10_5_7_11_4_Menu_Verify()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			return Meshes.Length > 0;
		}
		[MenuItem("Assets/Mesh/Merge Submeshes 2,12,10,5,7,11,4")]
		public static void MergeSubmesh_2_12_10_5_7_11_4_Menu()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			foreach (var mesh in Meshes)
			{
				var NewMesh = ExtractSubmeshesAsMesh(mesh, new int[] { 2, 12, 10, 5, 7, 11, 4 });
				AssetWriter.SaveAsset(NewMesh);
			}
		}


		[MenuItem("Assets/Mesh/Merge Submeshes 8,6,9,3,0,1", true)]
		public static bool MergeSubmesh_8_6_9_3_0_1_Menu_Verify()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			return Meshes.Length > 0;
		}
		[MenuItem("Assets/Mesh/Merge Submeshes 8,6,9,3,0,1")]
		public static void MergeSubmesh_8_6_9_3_0_1_Menu()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			foreach (var mesh in Meshes)
			{
				var NewMesh = ExtractSubmeshesAsMesh(mesh, new int[] { 8, 6, 9, 3, 0, 1 });
				AssetWriter.SaveAsset(NewMesh);
			}
		}



		[MenuItem("Assets/Mesh/Split Submeshes...", true)]
		public static bool SplitSubmeshes_Verify()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			return Meshes.Length > 0;
		}
		[MenuItem("Assets/Mesh/Split Submeshes...")]
		public static void SplitSubmeshes()
		{
			var Meshes = Selection.GetFiltered<Mesh>(SelectionMode.Assets);
			foreach (var mesh in Meshes)
			{
				for (int s = 0; s < mesh.subMeshCount; s++)
				{
					var NewMesh = ExtractSubmeshesAsMesh(mesh, new int[] { s });
					AssetWriter.SaveAsset(NewMesh);
				}
			}
		}
#endif
		public static void MergeSubmeshes(Mesh mesh,int IndexesUvChannel)
		{
			//	get a merged triangle list
			var NewTriangles = new List<int>();
			var SubmeshIndexes = new Vector2[mesh.vertexCount];

			for (int sm = 0; sm < mesh.subMeshCount;	sm++ )
			{
				var OldTriangleCount = NewTriangles.Count;
				var smtris = mesh.GetTriangles(sm, true);
				NewTriangles.AddRange(smtris);

				//	set the submesh index of the new batch
				for (int t = OldTriangleCount; t < NewTriangles.Count; t++)
				{
					var vi = NewTriangles[t];
					SubmeshIndexes[vi].x = sm;
				}
			}

			//	remove old triangles
			mesh.subMeshCount = 1;
			mesh.SetUVs( IndexesUvChannel, new List<Vector2>(SubmeshIndexes) );
			mesh.SetTriangles( NewTriangles, 0 );
			mesh.UploadMeshData(false);
		}

		public static Mesh ExtractSubmeshesAsMesh(Mesh mesh,int[] SubmeshesToMerge)
		{
			//	copy verts, uvs etc, but not triangle sets
			var MergedMesh = CopyMesh(mesh,false);

			//	just in case we overflow, always set to 32bit format
			MergedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
			/*
			MergedMesh.subMeshCount = SubmeshesToMerge.Length;
			for (int i = 0; i < SubmeshesToMerge.Length;	i++)
			{
				var sm = SubmeshesToMerge[i];
				var smtriangles = mesh.GetTriangles(sm, true);
				MergedMesh.name += "_" + sm;
				MergedMesh.SetTriangles(smtriangles, i);
			}
			*/

			var MergedTriangles = new List<int>();
			foreach (var sm in SubmeshesToMerge)
			{
				var smtriangles = mesh.GetTriangles(sm,true);
				MergedTriangles.AddRange(smtriangles);
				MergedMesh.name += "_" + sm;
			}
			MergedMesh.subMeshCount = 1;
			MergedMesh.SetTriangles(MergedTriangles, 0);

			return MergedMesh;
		}


		class Vertex
		{
			public Vector3	position;
			public Vector3	normal;
			public Vector2	uv1;
			public Vector2	uv2;
			public Vector2	uv3;
			public Color	colourf;
			public Color32	colour32;
		};


		static void SafeSet<T>(ref T Value,T[] Array,int ArrayIndex)
		{
			if ( Array == null )
				return;
			if (Array.Length == 0)
				return;
			Value = Array [ArrayIndex];
		}

		static T[] NullIfEmpty<T>(T[] Array)
		{
			if (Array == null)
				return null;
			if (Array.Length == 0)
				return null;
			return Array;
		}

		//	center the mesh so the center of the bounds is 0,0,0 (for when your mesh bounds may be specifically offset!)
		public static void CenterOnBounds(this Mesh mesh)
		{
			//	get delta to center it
			var MeshBounds = mesh.bounds;
			var Delta = -MeshBounds.center;
			var Positions = mesh.vertices;
			for (int i = 0; i < Positions.Length; i++)
				Positions[i] += Delta;
			mesh.vertices = Positions;
			//	this should be 000 now
			MeshBounds.center += Delta;
			mesh.bounds = MeshBounds;
		}

		public static Mesh CopyMesh(Mesh OldMesh,bool CopyTriangles=true)
		{
			var NewMesh = new Mesh();
			NewMesh.name = OldMesh.name;
			NewMesh.vertices = NullIfEmpty (OldMesh.vertices);
			NewMesh.uv = NullIfEmpty (OldMesh.uv);
			NewMesh.uv2 = NullIfEmpty (OldMesh.uv2);
			NewMesh.uv3 = NullIfEmpty (OldMesh.uv3);
			NewMesh.normals = NullIfEmpty (OldMesh.normals);
			NewMesh.colors = NullIfEmpty (OldMesh.colors);
			NewMesh.tangents = NullIfEmpty (OldMesh.tangents);
			NewMesh.bounds = OldMesh.bounds;

			//	gr: VERY important. submeshes with > 65k triangles causes my mac to reboot if the mesh gets uploaded without 32bit indexes set.
			//	probably should set this in the right conditions irregardless of old mesh settings
			NewMesh.indexFormat = OldMesh.indexFormat;

			if (CopyTriangles)
			{
				NewMesh.subMeshCount = OldMesh.subMeshCount;
				for (int sm = 0; sm < OldMesh.subMeshCount; sm++)
				{
					var smtriangles = OldMesh.GetTriangles(sm,true);
					NewMesh.SetTriangles(smtriangles, sm);
				}
			}

			return NewMesh;
		}



		public static void WeldVertexesOfMesh(ref Mesh mesh)
		{
			//	get a tolerance relative to the size of the bounds
			//	todo: prompt user for variables!
			float DistanceTolerance = 0.0001f;
			var BoundsMin = Mathf.Min(mesh.bounds.size.x, Mathf.Min(mesh.bounds.size.y, mesh.bounds.size.z));
			DistanceTolerance *= BoundsMin;

			var TriangleIndexes = mesh.triangles;
			var Positions = new List<Vector3>(mesh.vertices);
			var Normals = mesh.normals!=null ? new List<Vector3>(mesh.normals) : null;

			//	gr: this is vector2... need to handle bigger vector sizes some how!
			var Uv_ = mesh.uv!= null && mesh.uv.Length > 0 ? new List<Vector2>(mesh.uv) : null;
			var Uv2 = mesh.uv2!= null && mesh.uv2.Length>0 ? new List<Vector2>(mesh.uv2) : null;
			var Uv3 = mesh.uv3!= null && mesh.uv3.Length > 0 ? new List<Vector2>(mesh.uv3) : null;
			var Uv4 = mesh.uv4!= null && mesh.uv4.Length > 0 ? new List<Vector2>(mesh.uv4) : null;

			System.Action<int> RemoveVertex = (VertexIndex) =>
			{
				Positions.RemoveAt(VertexIndex);
				if (Normals != null) Normals.RemoveAt(VertexIndex);
				if (Uv_ != null) Uv_.RemoveAt(VertexIndex);
				if (Uv2 != null) Uv2.RemoveAt(VertexIndex);
				if (Uv3 != null) Uv3.RemoveAt(VertexIndex);
				if (Uv4 != null) Uv4.RemoveAt(VertexIndex);
				if (Normals != null) Normals.RemoveAt(VertexIndex);
			};

			System.Func<Vector3,int, int?> FindMatchingVertexIgnoring = (Position,IgnoreIndex) =>
			{
				for (int i = 0; i < Positions.Count; i++)
				{
					if (i == IgnoreIndex)
						continue;
					var Distance = Vector3.Distance(Positions[i], Position);

					//	match with ANY, or match with best? lets say any and just increase tolerance if we need to
					if (Distance <= DistanceTolerance)
						return i;
				}
				return null;
			};

			using (var Progress = new ScopedProgressBar("Welding vertexes", true))
			{
				var WeldCount = 0;
				var OrigCount = Positions.Count;

				//	merge vertexes downwards to make index changes easy
				for (int v = Positions.Count - 1; v >= 1; v--)
				{
					{
						var p = Positions.Count - v;
						var Name = "Welding " + p + "/" + OrigCount + " (" + WeldCount + " welded)";
						Progress.SetProgress(Name, p, OrigCount, 100);
					}				

					//	find one to merge with
					var MergeIndex = FindMatchingVertexIgnoring(Positions[v], v);
					if (!MergeIndex.HasValue)
						continue;
						WeldCount++;

					var m = MergeIndex.Value;
					//	remove all references to v, change all references to v to m, and drop index of anything further down the array
					RemoveVertex(v);
					for (int t = 0; t < TriangleIndexes.Length; t++)
					{
						if (TriangleIndexes[t] == v)
							TriangleIndexes[t] = m;
						else if (TriangleIndexes[t] > v)
							TriangleIndexes[t]--;
					}
				}
			}

			//	all done!
			mesh.vertices = Positions.ToArray();
			mesh.triangles = TriangleIndexes;
			mesh.normals = Normals.ToArray();
			mesh.uv = Uv_.ToArray();
			mesh.uv2 = Uv2.ToArray();
			mesh.uv3 = Uv3.ToArray();
			mesh.uv4 = Uv4.ToArray();

			mesh.UploadMeshData(false);
		}

		public static void UnshareTrianglesOfMesh(Mesh mesh, System.Action<MeshContents> MakeNewMesh, bool Allow32BitIndexes)
		{
			for (int sm = 0; sm < mesh.subMeshCount;	sm++ )
			{
				UnshareTrianglesOfMesh(mesh, sm, MakeNewMesh, Allow32BitIndexes);
			}
		}
			
		public static void UnshareTrianglesOfMesh(Mesh mesh, int Submesh, System.Action<MeshContents> MakeNewMesh, bool Allow32BitIndexes)
		{
#if !UNITY_2017_3_OR_NEWER
			Allow32BitIndexes = false;
#endif
			var OldTriangles = mesh.GetTriangles(Submesh);
			var OldPositions = mesh.vertices;
			var OldNormals = mesh.normals;
			var OldUv1s = mesh.uv;
			var OldUv2s = mesh.uv2;
			var OldUv3s = mesh.uv3;
			var OldColourfs = mesh.colors;
			var OldColour32s = mesh.colors32;

			List<Vector3> NewPositons = NullIfEmpty(OldPositions)!=null ? new List<Vector3> () : null;
			List<Vector3> NewNormals = NullIfEmpty(OldNormals)!=null ? new List<Vector3> () : null;
			List<Vector2> NewUv1s = NullIfEmpty(OldUv1s)!=null ? new List<Vector2> () : null;
			List<Vector2> NewUv2s = NullIfEmpty(OldUv2s)!=null ? new List<Vector2> () : null;
			List<Vector2> NewUv3s = NullIfEmpty(OldUv3s)!=null ? new List<Vector2> () : null;
			List<Color> NewColourfs = NullIfEmpty(OldColourfs)!=null ? new List<Color> () : null;
			List<Color32> NewColour32s = NullIfEmpty(OldColour32s)!=null ? new List<Color32> () : null;
			List<int> TriangleIndexes = new List<int> ();

			bool PromptShown = false;
			System.Func<Vertex,Vertex,Vertex,bool> PushTriangle = (a, b, c) => {

				if (!Allow32BitIndexes)
				{
					//	hit limits
					if (TriangleIndexes.Count >= Max16BitTriangleIndexes && !PromptShown)
					{
#if UNITY_EDITOR
						var DialogResult = EditorUtility.DisplayDialogComplex("Error", "Hit vertex limit with " + (TriangleIndexes.Count / 3) + " triangles (" + TriangleIndexes.Count + " vertexes.", "Stop Here", "Abort", "Overflow");
#else
					var DialogResult = 1;
#endif
						PromptShown = true;

						//	stop
						if (DialogResult == 0)
						{
							return false;
						}
						else if (DialogResult == 2)
						{   //	continue/overflow
						}
						else if (DialogResult == 1)
						{   //	abort
							throw new System.Exception("Aborted export");
						}
						else
						{
							throw new System.Exception("unknown dialog result " + DialogResult);
						}
					}
				}

				if ( NewPositons != null )
				{
					NewPositons.Add (a.position);
					NewPositons.Add (b.position);
					NewPositons.Add (c.position);
				}
				if (NewNormals != null) {
					NewNormals.Add (a.normal);
					NewNormals.Add (b.normal);
					NewNormals.Add (c.normal);
				}
				if (NewUv1s != null) {
					NewUv1s.Add (a.uv1);
					NewUv1s.Add (b.uv1);
					NewUv1s.Add (c.uv1);
				}
				if (NewUv2s != null) {
					NewUv2s.Add (a.uv2);
					NewUv2s.Add (b.uv2);
					NewUv2s.Add (c.uv2);
				}
				if (NewUv3s != null) {
					NewUv3s.Add (a.uv3);
					NewUv3s.Add (b.uv3);
					NewUv3s.Add (c.uv3);
				}
				if (NewColourfs != null) {
					NewColourfs.Add (a.colourf);
					NewColourfs.Add (b.colourf);
					NewColourfs.Add (c.colourf);
				}
				if (NewColour32s != null) {
					NewColour32s.Add (a.colour32);
					NewColour32s.Add (b.colour32);
					NewColour32s.Add (c.colour32);
				}

				TriangleIndexes.Add (TriangleIndexes.Count);
				TriangleIndexes.Add (TriangleIndexes.Count);
				TriangleIndexes.Add (TriangleIndexes.Count);

				return true;
			};

			using( var Progress = new ScopedProgressBar("Splitting mesh") )
			{
				var va = new Vertex ();
				var vb = new Vertex ();
				var vc = new Vertex ();

				for (int t = 0;	t < OldTriangles.Length;	t += 3) {

					Progress.SetProgress ("Adding triangle", t/3, OldTriangles.Length/3, 100 );

					var ia = OldTriangles [t];
					var ib = OldTriangles [t+1];
					var ic = OldTriangles [t+2];

					SafeSet (ref va.position, OldPositions, ia);
					SafeSet (ref vb.position, OldPositions, ib);
					SafeSet (ref vc.position, OldPositions, ic);

					SafeSet (ref va.normal, OldNormals, ia);
					SafeSet (ref vb.normal, OldNormals, ib);
					SafeSet (ref vc.normal, OldNormals, ic);

					SafeSet (ref va.uv1, OldUv1s, ia);
					SafeSet (ref vb.uv1, OldUv1s, ib);
					SafeSet (ref vc.uv1, OldUv1s, ic);

					SafeSet (ref va.uv2, OldUv2s, ia);
					SafeSet (ref vb.uv2, OldUv2s, ib);
					SafeSet (ref vc.uv2, OldUv2s, ic);

					SafeSet (ref va.uv3, OldUv3s, ia);
					SafeSet (ref vb.uv3, OldUv3s, ib);
					SafeSet (ref vc.uv3, OldUv3s, ic);

					SafeSet (ref va.colourf, OldColourfs, ia);
					SafeSet (ref vb.colourf, OldColourfs, ib);
					SafeSet (ref vc.colourf, OldColourfs, ic);

					SafeSet (ref va.colour32, OldColour32s, ia);
					SafeSet (ref vb.colour32, OldColour32s, ib);
					SafeSet (ref vc.colour32, OldColour32s, ic);

					if ( !PushTriangle (va, vb, vc) )
						break;
				}
			}

			var OldBounds = mesh.bounds;

			var NewMeshContents = new MeshContents();
			NewMeshContents.NewPositons = NewPositons;
			NewMeshContents.NewNormals = NewNormals;
			NewMeshContents.NewUv1s = NewUv1s;
			NewMeshContents.NewUv2s = NewUv2s;
			NewMeshContents.NewUv3s = NewUv3s;
			NewMeshContents.NewColourfs = NewColourfs;
			NewMeshContents.NewColour32s = NewColour32s;
			NewMeshContents.TriangleIndexes = TriangleIndexes;
			NewMeshContents.OldBounds = OldBounds;

			MakeNewMesh.Invoke(NewMeshContents);
		}

		public static void UnshareTrianglesOfMesh(ref Mesh mesh)
		{
			var Allow32BitIndexes = true;
			MeshContents NewContents = null;
			UnshareTrianglesOfMesh(mesh, (contents) =>
			{
				NewContents = contents;
			}, Allow32BitIndexes);

			mesh.Clear();

			if (NewContents.TriangleIndexes.Count >= Max16BitTriangleIndexes)
			{
#if UNITY_2017_3_OR_NEWER
				mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
#else
				Debug.LogWarning("New mesh has >16bit triangle indexes ("+NewContents.TriangleIndexes.Count+" >= " + Max16BitTriangleIndexes + " ), won't render correctly");
#endif
			}

			if (NewContents.NewPositons != null) mesh.SetVertices(NewContents.NewPositons);
			if (NewContents.NewNormals != null) mesh.SetNormals(NewContents.NewNormals);
			if (NewContents.NewUv1s != null) mesh.SetUVs(0, NewContents.NewUv1s);
			if (NewContents.NewUv2s != null) mesh.SetUVs(1, NewContents.NewUv2s);
			if (NewContents.NewUv3s != null) mesh.SetUVs(1, NewContents.NewUv3s);
			if (NewContents.NewColourfs != null) mesh.SetColors(NewContents.NewColourfs);
			if (NewContents.NewColour32s != null) mesh.SetColors(NewContents.NewColour32s);
			if (NewContents.TriangleIndexes != null) mesh.SetIndices(NewContents.TriangleIndexes.ToArray(), MeshTopology.Triangles, 0);
			mesh.bounds = NewContents.OldBounds;
			mesh.UploadMeshData(false);

		}



		public static void RandomiseTriangleOrder(Mesh mesh)
		{
			var OldTriangles = new List<int> (mesh.triangles);
			var NewTriangles = new List<int> ();

			using( var Progress = new ScopedProgressBar("Randomising triangle order") )
			{
				var OriginalTriangleCount = OldTriangles.Count;

				//	move a random set of indexes to the new list
				while (OldTriangles.Count > 0) {
					var TriangleIndex = Random.Range (0, OldTriangles.Count);
					TriangleIndex -= TriangleIndex % 3;
					if (TriangleIndex % 3 != 0)
						throw new System.Exception ("Picked triangle index not at triangle start");
					NewTriangles.Add (OldTriangles [TriangleIndex + 0]);
					NewTriangles.Add (OldTriangles [TriangleIndex + 1]);
					NewTriangles.Add (OldTriangles [TriangleIndex + 2]);
					OldTriangles.RemoveRange (TriangleIndex, 3);
					Progress.SetProgress ("Shuffling", NewTriangles.Count, OriginalTriangleCount, 100);
				}
			}

			mesh.triangles = NewTriangles.ToArray ();
		}


		public static void SetMeshUVToTriangleIndex (Mesh mesh,int UvChannel) 
		{
			var m = mesh;

			var TriangleIndexes = m.triangles;
			#if UNITY_EDITOR
			if (TriangleIndexes.Length != m.vertexCount) {
				var DialogResult = EditorUtility.DisplayDialog ("Error", "Assigning triangle indexes to attributes probably won't work as expected for meshes sharing vertexes.", "Continue", "Cancel");
				if (!DialogResult)
					throw new System.Exception ("Aborted assignment of UV triangle indexes");
			}
			#endif

			using (var Progress = new ScopedProgressBar ("Setting UVs")) {
				var Uvs = new Vector2[m.vertexCount];
				{
					var v2 = new Vector2 ();	//	avoid allocs
					for (int t = 0;	t < TriangleIndexes.Length;	t += 3) {

						if (t % 100 == 0)
							Progress.SetProgress ("Setting UV of triangle", t / 3, TriangleIndexes.Length / 3);

						for (int i = 0;	i < 3;	i++) {
							var iv = TriangleIndexes [t + i];
							v2.x = t / 3;
							v2.y = i;
							Uvs [iv] = v2;
						}
					}
				}
				m.SetUVs(UvChannel, new List<Vector2>(Uvs));
				m.UploadMeshData (false);
				#if UNTIY_EDITOR
				AssetDatabase.SaveAssets ();
				#endif
			}
		}


		public static void SetMeshUVToVertexIndex(Mesh mesh, int UvChannel)
		{
			var m = mesh;

			using (var Progress = new ScopedProgressBar("Setting UVs"))
			{
				var Uvs = new Vector2[m.vertexCount];
				{
					var v2 = new Vector2(); //	avoid allocs
					for (int v = 0; v < m.vertexCount; v++)
					{
						if (v % 100 == 0)
							Progress.SetProgress("Setting UV of triangle", v, m.vertexCount);

						v2.x = v;
						v2.y = 0;
						Uvs[v] = v2;
					}
				}
				m.SetUVs(UvChannel, new List<Vector2>(Uvs));
				m.UploadMeshData(false);
#if UNTIY_EDITOR
				AssetDatabase.SaveAssets ();
#endif
			}
		}


		#if UNITY_EDITOR
		public static void SaveMesh (Mesh mesh, string name, bool makeNewInstance)
		{
			string path = AssetDatabase.GetAssetPath (mesh);

			if (string.IsNullOrEmpty (path))
				makeNewInstance = true;

			if (makeNewInstance) {
				path = EditorUtility.SaveFilePanel ("Save Separate Mesh Asset", "Assets/", name, "asset");
				if (string.IsNullOrEmpty (path))
					throw new System.Exception ("Cancelled new mesh path");

				path = FileUtil.GetProjectRelativePath (path);
			}

			Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;

			AssetDatabase.CreateAsset(meshToSave, path);
			AssetDatabase.SaveAssets();
		}
		#endif


		public static Mesh	GetPrimitiveMesh(PrimitiveType type)
		{
			return GameObject.CreatePrimitive( type ).GetComponent<MeshFilter>().sharedMesh;
		}


		public static Material	GetMaterial(GameObject Object,bool SharedMaterial)
		{
			try {
				var Renderer = Object.GetComponent<MeshRenderer> ();
				var mat = SharedMaterial ? Renderer.sharedMaterial : Renderer.material;
				return mat;
			} catch {
			}

			try {
				var Renderer = Object.GetComponent<SpriteRenderer> ();
				var mat = SharedMaterial ? Renderer.sharedMaterial : Renderer.material;
				return mat;
			} catch {
			}

			return null;
		}

		public static RenderTextureFormat Format2DToRenderTextureFormat(TextureFormat Format)
		{
			switch (Format) {
			case TextureFormat.RGBA32:
			case TextureFormat.ARGB32:
				return RenderTextureFormat.ARGB32;
			case TextureFormat.RGB24:
				return RenderTextureFormat.ARGBFloat;
			case TextureFormat.RFloat:
				return RenderTextureFormat.RFloat;
			case TextureFormat.RGBAFloat:
				return RenderTextureFormat.ARGBFloat;
			}

			throw new System.Exception ("Failed to convert " + Format + " to RenderTextureFormat");
		}

	}
}


