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
}
