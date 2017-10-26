using UnityEngine;
using System.Collections;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PopX
{
	public static class BakeUv
	{
		const string ShaderAssetName = "PopBakeUv_WorldPosition";

		//	gr; need to prompt this option to user somehow... I figure, make it big and user can resize later
		public static int BakeTextureWidth = 4096;
		public static int BakeTextureHeight = 4096;


		#if UNITY_EDITOR
		[MenuItem("CONTEXT/MeshFilter/Bake mesh uv to texture with world space...")]
		public static void UnshareTrianglesOfMesh (MenuCommand menuCommand) 
		{
			//	gr: wrap this in a try/catch, if it fails, pop up object/file selection to pick shader

			var ShaderGuids = AssetDatabase.FindAssets ("t:shader " + ShaderAssetName);
			if (ShaderGuids.Length != 1)
				throw new System.Exception ("Failed to find shader to bake with: " + ShaderAssetName + " (found " + ShaderGuids.Length + " matches");

			var ShaderGuid = ShaderGuids [0];
			var ShaderPath = AssetDatabase.GUIDToAssetPath (ShaderGuid);
			var Shader = AssetDatabase.LoadAssetAtPath<Shader> (ShaderPath);


			var mf = menuCommand.context as MeshFilter;
			var mesh = mf.sharedMesh;
			var transform = mf.transform;

			using ( var Progress = new ScopedProgressBar("Baking Mesh UV") )
			{
				System.Action<string,int,int> OnTriangleBakeProgress = (string StepDescription, int Step, int StepMax) => {
					Progress.SetProgress( StepDescription, Step, StepMax );
				};

				//	make output...
				//	float format so we're not restricted to 0-1
				var PositionMap = new RenderTexture ( BakeTextureWidth, BakeTextureHeight, 0, RenderTextureFormat.ARGBFloat);
				PositionMap.name = mesh.name + " baked uv map";

				Graphics.Blit( Texture2D.blackTexture, PositionMap );

				var DrawTriangleMat = new Material (Shader);

				Bake( mesh, transform, PositionMap, DrawTriangleMat, OnTriangleBakeProgress);

				//	prompt to save to file
				AssetWriter.SaveAsset (PositionMap);
			}
		}
		#endif

		//	returns false if skipped due to tiny size
		static bool SetTriangle(Material DrawTriangleMat,Mesh mesh,int MeshTriangleIndex,int ShaderTriangleIndex,Texture BakedTextureMap)
		{
			List<Vector2> uvs = new List<Vector2> ();
			mesh.GetUVs(0,uvs);

			if (uvs.Count == 0)
				throw new System.Exception ("Mesh " + mesh.name + " missing uv0 map");

			var Indexes = mesh.GetIndices (0);
			var TriangleUvs = new Vector2[] {
				uvs [Indexes[(MeshTriangleIndex * 3) + 0]],
				uvs [Indexes[(MeshTriangleIndex * 3) + 1]],
				uvs [Indexes[(MeshTriangleIndex * 3) + 2]]
			};

			//	evaluate size of triangle
			var TriangleArea = PopMath.GetTriangleArea( TriangleUvs[0], TriangleUvs[1], TriangleUvs[2] );
			TriangleArea *= BakedTextureMap.width * BakedTextureMap.height;

			if (TriangleArea < 1) {
				Debug.Log ("Triangle skipped; area in pixels: " + TriangleArea);
				return false;
			}


			for ( int v=0;	v<3;	v++ )
			{
				var uv = TriangleUvs[v];

				string Uniform = "Triangle_Uv_" + ShaderTriangleIndex + "_" + v;
				DrawTriangleMat.SetVector(Uniform, new Vector4( uv.x, uv.y, v, 0) );
				//Debug.Log ("SetTriangle( " + Uniform + " ) = (" + uv.x + " " + uv.y + " )");
			}

			DrawTriangleMat.SetInt ("TriangleCount", ShaderTriangleIndex + 1); 

			return true;
		}

		static void SetTriangleMeta(Material DrawTriangleMat,Mesh mesh,int MeshTriangleIndex,int ShaderTriangleIndex,Transform transform)
		{
			var Verts = mesh.vertices;
			var Indexes = mesh.GetIndices (0);
			for ( int v=0;	v<3;	v++ )
			{
				//	get world pos
				var LocalPos = Verts[Indexes[(MeshTriangleIndex*3) + v]];
				var WorldPos = transform ? transform.TransformPoint (LocalPos) : LocalPos;

				string Uniform = "Triangle_Pos_" + ShaderTriangleIndex + "_" + v;
				DrawTriangleMat.SetVector(Uniform, new Vector4( WorldPos.x, WorldPos.y, WorldPos.z, 0) );
			}

		}




		static public void Bake(Mesh BakeMesh,Transform BakeTransform,RenderTexture BakedTextureMap,Material DrawTriangleMat,System.Action<string,int,int> OnProgress)
		{
			if ( OnProgress == null )
			{
				OnProgress = (x,y,z) => {
				};
			}
			var LastTexture = new RenderTexture(BakedTextureMap.width, BakedTextureMap.height, 0, BakedTextureMap.format);
			var TempTexture = new RenderTexture(BakedTextureMap.width, BakedTextureMap.height, 0, BakedTextureMap.format);

			//	clear 
			Graphics.Blit( Texture2D.blackTexture, LastTexture );

			var Indexes = BakeMesh.GetIndices(0);
			int TriCount = Indexes.Length / 3;
			int Skipped = 0;

			for (int t = 0;	t <TriCount;	t++) 
			{
				try
				{
					OnProgress ("Blitting " + t + " of " + TriCount + " triangles. (" + Skipped + " skipped)", t, TriCount);
					if (!SetTriangle (DrawTriangleMat, BakeMesh, t, 0, BakedTextureMap))
					{
						Skipped++;
						continue;
					}
					SetTriangleMeta (DrawTriangleMat, BakeMesh, t, 0, BakeTransform);
					Graphics.Blit (LastTexture, TempTexture, DrawTriangleMat);
					Graphics.Blit (TempTexture, LastTexture);
				}
				catch 
				{
					//	save last bit of work
					if (t > 0) {
						Debug.LogWarning ("Writing last triangle bake");
						Graphics.Blit (LastTexture, BakedTextureMap);
					}
					throw;
				}
			}
			Graphics.Blit (LastTexture, BakedTextureMap);
		}
	}
}

