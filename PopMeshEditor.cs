using System.Collections;
using System.Collections.Generic;
using UnityEngine;




#if UNITY_EDITOR
using UnityEditor;
#endif

public static class PopMeshEditor {

	[MenuItem("CONTEXT/MeshFilter/Unshare triangle indexes of mesh")]
	public static void UnshareTrianglesOfMesh (MenuCommand menuCommand) 
	{
		var mf = menuCommand.context as MeshFilter;
		var m = mf.sharedMesh;
		UnshareTrianglesOfMesh (ref m);
		SaveMesh(m, m.name, true);
	}

	[MenuItem("CONTEXT/MeshFilter/Unshare triangle indexes to new mesh...")]
	public static void UnshareTrianglesToMesh (MenuCommand menuCommand) 
	{
		var mf = menuCommand.context as MeshFilter;
		var m = CopyMesh(mf.sharedMesh);
		m.name = mf.sharedMesh.name + " unshared";
		UnshareTrianglesOfMesh (ref m);
		SaveMesh(m, m.name, true);
	}


	class Vertex
	{
		public Vector3	position;
		public Vector3	normal;
		public Vector2	uv1;
		public Vector2	uv2;
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
		var OldColourfs = mesh.colors;
		var OldColour32s = mesh.colors32;

		List<Vector3> NewPositons = NullIfEmpty(OldPositions)!=null ? new List<Vector3> () : null;
		List<Vector3> NewNormals = NullIfEmpty(OldNormals)!=null ? new List<Vector3> () : null;
		List<Vector2> NewUv1s = NullIfEmpty(OldUv1s)!=null ? new List<Vector2> () : null;
		List<Vector2> NewUv2s = NullIfEmpty(OldUv2s)!=null ? new List<Vector2> () : null;
		List<Color> NewColourfs = NullIfEmpty(OldColourfs)!=null ? new List<Color> () : null;
		List<Color32> NewColour32s = NullIfEmpty(OldColour32s)!=null ? new List<Color32> () : null;
		List<int> TriangleIndexes = new List<int> ();

		bool PromptShown = false;
		System.Func<Vertex,Vertex,Vertex,bool> PushTriangle = (a, b, c) => {

			//	hit limits
			if ( TriangleIndexes.Count >= 65000 && !PromptShown )
			{
				var DialogResult = EditorUtility.DisplayDialogComplex("Error", "Hit vertex limit with " + (TriangleIndexes.Count/3) + " triangles (" + TriangleIndexes.Count + " vertexes.", "Stop Here", "Abort", "Overflow" );
				PromptShown = true;

				//	stop
				if ( DialogResult == 0 )
				{
					return false;
				}
				else if ( DialogResult == 2 )	//	continue/overflow
				{
				}
				else if ( DialogResult == 1 )	//	abort
				{
					throw new System.Exception("Aborted export");
				}
				else
				{
					throw new System.Exception("unknown dialog result " + DialogResult);
				}
			}

			if ( NewPositons != null )
			{
				NewPositons.Add( a.position );
				NewPositons.Add( b.position );
				NewPositons.Add( c.position );
			}
			if ( NewNormals != null )
			{
				NewNormals.Add( a.normal );
				NewNormals.Add( b.normal );
				NewNormals.Add( c.normal );
			}
			if ( NewUv1s != null )
			{
				NewUv1s.Add( a.uv1 );
				NewUv1s.Add( b.uv1 );
				NewUv1s.Add( c.uv1 );
			}
			if ( NewUv2s != null )
			{
				NewUv2s.Add( a.uv2 );
				NewUv2s.Add( b.uv2 );
				NewUv2s.Add( c.uv2 );
			}
			if ( NewColourfs != null )
			{
				NewColourfs.Add( a.colourf );
				NewColourfs.Add( b.colourf );
				NewColourfs.Add( c.colourf );
			}
			if ( NewColour32s != null )
			{
				NewColour32s.Add( a.colour32 );
				NewColour32s.Add( b.colour32 );
				NewColour32s.Add( c.colour32 );
			}

			TriangleIndexes.Add( TriangleIndexes.Count );
			TriangleIndexes.Add( TriangleIndexes.Count );
			TriangleIndexes.Add( TriangleIndexes.Count );

			return true;
		};

		using( var Progress = new ScopedProgressBar("Splitting mesh") )
		{
			var va = new Vertex ();
			var vb = new Vertex ();
			var vc = new Vertex ();

			for (int t = 0;	t < OldTriangles.Length;	t += 3) {

				if ( t % 100 == 0 )
					Progress.SetProgress ("Adding triangle " + (t / 3) + "/" + (OldTriangles.Length / 3), t / (float)OldTriangles.Length);

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
		if ( NewColourfs != null )	mesh.SetColors (NewColourfs);
		if ( NewColour32s != null )	mesh.SetColors (NewColour32s);
		if ( TriangleIndexes != null )	mesh.SetIndices(TriangleIndexes.ToArray(), MeshTopology.Triangles, 0);
		mesh.bounds = OldBounds;
		mesh.UploadMeshData (false);
	}

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
}
