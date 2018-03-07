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
	const string	SettingsFilename = "default.vrsettings";
	const string	ManifestFilename = "driver.vrdrivermanifest";
	const string 	NullDriver = "null";

	//	general settings
	const string 	RequireHmdKey = "requireHmd";
	const string	ForcedDriverKey = "forcedDriver";
	const string	ActivateMultipleDriversKey = "activateMultipleDrivers";

	//	driver manifest
	const string	AlwaysActivateKey = "alwaysActivate";
	//	driver settings
	const string	DriverEnableKey = "enable";


	static string	SettingsPath				{	get	{	return Path.Combine(GetSettingsFolder(null), SettingsFilename );	}	}
	static string	SettingsFolder				{	get	{	return GetSettingsFolder(null);	}	}

	static public string	GetDriverSettingsPath(string DriverName)
	{
		return Path.Combine(GetSettingsFolder(DriverName), SettingsFilename);
	}

	static public string GetDriverManifestPath(string DriverName)
	{
		return Path.Combine(GetDriverFolder(DriverName), ManifestFilename);
	}

	static string	GetDriverFolder(string DriverName)
	{
		if (string.IsNullOrEmpty(DriverName))
			throw new System.Exception("GetDriverFolder expects a drivername");
			
		var Parts = new List<string>();
		Parts.Add(SteamRuntimeFolder);
		Parts.Add("drivers");
		Parts.Add(DriverName);
		return PopX.IO.Path_Combine(Parts);
	}

	static string GetSettingsFolder(string DriverName)
	{
		var Parts = new List<string>();
		Parts.Add(SteamRuntimeFolder);
		if (!string.IsNullOrEmpty(DriverName))
		{
			Parts.Add("drivers");
			Parts.Add(DriverName);
		}
		Parts.Add("resources");
		Parts.Add("settings");
		return PopX.IO.Path_Combine(Parts);
	}

	//	todo, work this out from env vars etc
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	static string SteamRuntimeFolder { get { return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "Library/Application Support/Steam/steamapps/common/SteamVR/SteamVR.app/Contents/MacOS/runtime/"); } }
#else
	const string SteamRuntimeFolder = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\SteamVR\\";
#endif


#if UNITY_EDITOR
	[MenuItem("SteamVR/Set RequireHmd False")]
	public static void Set_RequireHmd_False()
	{
		ChangeSettings(null, RequireHmdKey, false);
	}
#endif

#if UNITY_EDITOR
	[MenuItem("SteamVR/Set RequireHmd True")]
	public static void Set_RequireHmd_True()
	{
		ChangeSettings(null, RequireHmdKey, true);
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
	[MenuItem("SteamVR/Show " + NullDriver + " driver settings")]
	public static void ShowDriverSettingsFile()
	{
		EditorUtility.RevealInFinder( GetDriverFolder(NullDriver));
	}
#endif

#if UNITY_EDITOR
	[MenuItem("SteamVR/enable " + NullDriver +" driver")]
	public static void EnableNullDriver()
	{
		//	gr: for OSX. March 7th 2018
		//	enable the driver and make it always activate
		//	general settings dont NEED requireHmd nor forcedDriver, just activateMultipleDrivers
		ChangeSettings(NullDriver,DriverEnableKey,true);
		ChangeManifest(NullDriver, AlwaysActivateKey, true);
		ChangeSettings(null, ActivateMultipleDriversKey, true);
	}
#endif

	public static void ChangeSettings(string DriverName, string Key, bool Value)
	{
		var Filename = GetDriverSettingsPath(DriverName);
		ChangeFile(Filename, Key, Value,DriverName);
	}

	public static void ChangeManifest(string DriverName, string Key, bool Value)
	{
		var Filename = GetDriverManifestPath(DriverName);
		ChangeFile(Filename, Key, Value,DriverName);
	}

	public static void ChangeFile(string Filename,string Key,bool Value,string DriverName)
	{
		if (string.IsNullOrEmpty(DriverName))
			DriverName = "null";
		
		try
		{
			var Contents = File.ReadAllText(Filename);
			var NewContents = Contents;
			PopX.Json.Replace(ref NewContents, Key, Value);
			if (NewContents == Contents)
			{
				Debug.Log("No changes made to " + DriverName + " settings (" + Key + "=" + Value + ")");
				return;
			}
			System.IO.File.WriteAllText(Filename, NewContents);
			Debug.Log("Updated " + DriverName + " settings.");
		}
		catch (System.Exception e)
		{
			Debug.LogError("Error replacing " + Key + " in " + DriverName + " settings...");
			Debug.LogException(e);
		}	
	}


}
