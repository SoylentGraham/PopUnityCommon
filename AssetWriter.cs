using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetWriter : MonoBehaviour {


	#if UNITY_EDITOR
	public static void DeleteAsset(string Name)
	{
		var Path = "Assets/" + Name + ".asset";
		UnityEditor.AssetDatabase.DeleteAsset (Path);
	}
	#endif
		

	#if UNITY_EDITOR
	public static T WriteAsset<T>(string Name,T Asset) where T : Object
	{
		if (Asset == null)
			return null;

		var Path = "Assets/" + Name + ".asset";

		var ExistingAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(Path);

		if ( ExistingAsset == null )
		{
			UnityEditor.AssetDatabase.CreateAsset( Asset, Path );
		}
		else
		{
			UnityEditor.EditorUtility.CopySerialized(Asset, ExistingAsset);
		}

		return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(Path);
	}
	#endif



	#if UNITY_EDITOR
	public static T SaveAsset<T>(T Asset) where T : Object
	{
		if (Asset == null)
			return null;

		var Path = UnityEditor.AssetDatabase.GetAssetPath (Asset);
		if (string.IsNullOrEmpty (Path)) {
			Path = UnityEditor.EditorUtility.SaveFilePanelInProject ("Save new asset...", Asset.name, "asset", "Save asset");
			if (string.IsNullOrEmpty (Path))
				throw new System.Exception ("Save aborted");

			//	gr: returns null
			//Path = UnityEditor.FileUtil.GetProjectRelativePath (Path);
		}

		var ExistingAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(Path);

		if ( ExistingAsset == null )
		{
			UnityEditor.AssetDatabase.CreateAsset( Asset, Path );
			UnityEditor.AssetDatabase.SaveAssets();
		}
		else
		{
			UnityEditor.EditorUtility.CopySerialized(Asset, ExistingAsset);
		}

		return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(Path);
	}
	#endif



	#if UNITY_EDITOR
	public static T SaveAsNewAsset<T>(T ExistingAsset) where T : Object, new()
	{
		if (ExistingAsset == null)
			return null;

		//	duplicate this way; https://github.com/pharan/Unity-MeshSaver/blob/master/MeshSaver/Editor/MeshSaverEditor.cs
		//	copyserialised was creating a garbage mesh (same number of points, but messed up)
		var NewAsset = Object.Instantiate(ExistingAsset) as Mesh;
		NewAsset.name = ExistingAsset.name + "_copy";

		var Path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save new asset...", NewAsset.name, "asset", "Save asset");
		if (string.IsNullOrEmpty(Path))
			throw new System.Exception("Save aborted");

		UnityEditor.AssetDatabase.CreateAsset(NewAsset, Path);
		UnityEditor.AssetDatabase.SaveAssets();

		return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(Path);
	}
#endif
}
