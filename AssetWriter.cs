using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetWriter : MonoBehaviour {

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
}
