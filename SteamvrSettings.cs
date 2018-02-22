using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SteamVrSettings : MonoBehaviour
{

	const string 	SettingsFilename = "default.vrsettings";
	const string	RequireHmdKey = "requireHmd";
	static string	SettingsPath	{	get	{	return SettingsFolder + SettingsFilename;	}	}

	//	todo, work this out from env vars etc
#if UNITY_EDITOR_OSX
	static string	SettingsFolder	{	get	{	return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Library/Application Support/Steam/steamapps/common/SteamVR/SteamVR.app/Contents/MacOS/runtime/resources/settings/" );	}	}
#else
	const string SettingsFolder = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SteamVR\\resources\\settings\\";
#endif


#if UNITY_EDITOR
	[MenuItem("SteamVR/Set RequireHmd False")]
	public static void Set_RequireHmd_False()
	{
		SetVrSettings( RequireHmdKey, "false" );
	}
#endif

#if UNITY_EDITOR
	[MenuItem("SteamVR/Set RequireHmd True")]
	public static void Set_RequireHmd_True()
	{
		SetVrSettings( RequireHmdKey, "true" );
	}
#endif
	
#if UNITY_EDITOR
	[MenuItem("SteamVR/Show " + SettingsFilename)]
	public static void ShowSettingsFile()
	{
		EditorUtility.RevealInFinder( SettingsPath );
	}
#endif

#if UNITY_EDITOR
	public static void SetVrSettings(string Key,string NewValue)
	{

		//	setup regular expression
		string EscapedKey =  "\"" + Key + "\" : ";
		string MatchPattern = EscapedKey + "(true|false),";
		//	gr: I thought it would just repalce the match, but in c# the whole match is replaced :)
		//string Replacement = NewCategory;
		string Replacement = EscapedKey + NewValue + ",";
		Regex RegExpression = new Regex(MatchPattern);

		string NoChangeFilenames = null;

		var FilePaths = new string[1] { SettingsPath };

		foreach (var Path in FilePaths) {
			try
			{
				var Contents = File.ReadAllText( Path );
				var NewContents = RegExpression.Replace( Contents, Replacement );
				if ( Contents != NewContents )
				{
					System.IO.File.WriteAllText( Path, NewContents );
					Debug.Log("Updated " + Path );
				}
				else
				{
					NoChangeFilenames += Path + " ";
					//Debug.Log("skipped " + Path );
				}
			}
			catch(System.Exception e) {
				Debug.LogError ("Error replacing " + Key + " in " + Path);
				Debug.LogException (e);
			}
		}

		if (!string.IsNullOrEmpty (NoChangeFilenames))
			Debug.Log ("Didn't change " + NoChangeFilenames);

	}
#endif


}
