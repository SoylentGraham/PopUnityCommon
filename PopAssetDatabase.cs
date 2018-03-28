using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class PopAssetDatabase
{
	public static string GetTypeAssetTypeName(System.Type Type)
	{
#if UNITY_EDITOR
		//	some special cases
		if (Type == typeof(SceneAsset))
			return "scene";
#endif
		return Type.Name;
	}
	
	//	gr: maybe should allow this in production, but find assets from somewhere else? resources?
#if UNITY_EDITOR
	public static List<T> GetAssets<T>(string Filter=null) where T : UnityEngine.Object
	{
		var Assets = new List<T>();

		//	get all guids
		var Typename = GetTypeAssetTypeName(typeof(T));
		var Guids = AssetDatabase.FindAssets("t:" + Typename + " " + Filter );
		foreach ( var Guid in Guids )
		{
			var Path = AssetDatabase.GUIDToAssetPath(Guid);
			var Asset = AssetDatabase.LoadAssetAtPath<T>(Path);
			Assets.Add(Asset);
		}

		return Assets;
	}
#endif
}


