using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PopX
{
	//	gr: using System breaks too many references :/
	public static class Sys
	{
		public class UserCancelledException : global::System.Exception
		{
			public UserCancelledException(string message)
				: base(message) { }
		};
	};
}


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
	//	[on OSX at least] SaveFilePanelInProject doesn't remember your last dir... lets try
	static string LastSavedDirectory = null;
	#endif

	#if UNITY_EDITOR
	public static T SaveAsset<T>(T Asset) where T : Object
	{
		if (Asset == null)
			return null;

		var Path = UnityEditor.AssetDatabase.GetAssetPath (Asset);
		if (string.IsNullOrEmpty (Path)) {

			var Filename = Asset.name;

			if (!string.IsNullOrEmpty (LastSavedDirectory))
				Filename = LastSavedDirectory + Filename;

			Path = UnityEditor.EditorUtility.SaveFilePanelInProject ("Save new asset...", Filename, "asset", "Save asset");
			if (string.IsNullOrEmpty (Path))
				throw new PopX.Sys.UserCancelledException ("Save aborted");

			//	save last path
			try
			{
				LastSavedDirectory = PopX.IO.GetProjectRelativePath(Path);
			}
			catch {
				LastSavedDirectory = null;
			}
		}

		var ExistingAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(Path);

		if ( ExistingAsset == null )
		{
			var LastActiveRenderTexture = RenderTexture.active;

			//	gr: cant save a render texture if it's active!
			if (Asset is RenderTexture)
				RenderTexture.active = null;
			
			UnityEditor.AssetDatabase.CreateAsset( Asset, Path );

			RenderTexture.active = LastActiveRenderTexture;

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
