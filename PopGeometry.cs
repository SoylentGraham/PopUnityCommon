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
		[MenuItem("CONTEXT/MeshFilter/Set mesh UV0 to triangle index")]
		public static void _SetMeshUV0ToTriangleIndex (MenuCommand menuCommand) 
		{
			var mf = menuCommand.context as MeshFilter;
			var mesh = mf.sharedMesh;

			Undo.RecordObject(mesh, "Set mesh UV0 to triangle index of " + mesh.name);
			SetMeshUV0ToTriangleIndex (mesh);
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
			if (TriangleIndexes.Length != m.vertexCount) {
				var DialogResult = EditorUtility.DisplayDialog ("Error", "Assigning triangle indexes to attributes probably won't work as expected for meshes sharing vertexes.", "Continue", "Cancel");
				if (!DialogResult)
					throw new System.Exception ("Aborted assignment of UV triangle indexes");
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



		public static Mesh CopyMesh(Mesh OldMesh)
		{
			var NewMesh = new Mesh();
			NewMesh.name = OldMesh.name;
			NewMesh.vertices = NullIfEmpty (OldMesh.vertices);
			NewMesh.triangles = NullIfEmpty (OldMesh.triangles);
			NewMesh.uv = NullIfEmpty (OldMesh.uv);
			NewMesh.uv2 = NullIfEmpty (OldMesh.uv2);
			NewMesh.uv3 = NullIfEmpty (OldMesh.uv3);
			NewMesh.normals = NullIfEmpty (OldMesh.normals);
			NewMesh.colors = NullIfEmpty (OldMesh.colors);
			NewMesh.tangents = NullIfEmpty (OldMesh.tangents);
			NewMesh.bounds = OldMesh.bounds;
			return NewMesh;
		}

		public static void UnshareTrianglesOfMesh(ref Mesh mesh)
		{
			var OldTriangles = mesh.triangles;
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

				//	hit limits
				if (TriangleIndexes.Count >= 65000 && !PromptShown) {
					#if UNITY_EDITOR
					var DialogResult = EditorUtility.DisplayDialogComplex ("Error", "Hit vertex limit with " + (TriangleIndexes.Count / 3) + " triangles (" + TriangleIndexes.Count + " vertexes.", "Stop Here", "Abort", "Overflow");
					#else
					var DialogResult = 1;
					#endif
					PromptShown = true;

					//	stop
					if (DialogResult == 0) {
						return false;
					} else if (DialogResult == 2) {	//	continue/overflow
					} else if (DialogResult == 1) {	//	abort
						throw new System.Exception ("Aborted export");
					} else {
						throw new System.Exception ("unknown dialog result " + DialogResult);
					}
				}

				if (NewPositons != null) {
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

			mesh.Clear ();
			if ( NewPositons != null )	mesh.SetVertices (NewPositons);
			if ( NewNormals != null )	mesh.SetNormals (NewNormals);
			if ( NewUv1s != null )	mesh.SetUVs (0,NewUv1s);
			if ( NewUv2s != null )	mesh.SetUVs (1,NewUv2s);
			if ( NewUv3s != null )	mesh.SetUVs (1,NewUv3s);
			if ( NewColourfs != null )	mesh.SetColors (NewColourfs);
			if ( NewColour32s != null )	mesh.SetColors (NewColour32s);
			if ( TriangleIndexes != null )	mesh.SetIndices(TriangleIndexes.ToArray(), MeshTopology.Triangles, 0);
			mesh.bounds = OldBounds;
			mesh.UploadMeshData (false);
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


		public static void SetMeshUV0ToTriangleIndex (Mesh mesh) 
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
				m.uv = Uvs;
				m.UploadMeshData (true);
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


