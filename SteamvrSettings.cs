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
	//	manifest not present on windows (I don't think?) but doesn't need configuration for hmd-less mode to work it seems
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
	const string	ManifestFilename = "driver.vrdrivermanifest";
#else
	const string	ManifestFilename = null;
#endif
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
	[MenuItem("NewChromantics/SteamVR/Set RequireHmd False")]
	public static void Set_RequireHmd_False()
	{
		ChangeSettings(null, RequireHmdKey, false);
	}
#endif

#if UNITY_EDITOR
	[MenuItem("NewChromantics/SteamVR/Set RequireHmd True")]
	public static void Set_RequireHmd_True()
	{
		ChangeSettings(null, RequireHmdKey, true);
	}
#endif
	
#if UNITY_EDITOR
	[MenuItem("NewChromantics/SteamVR/Show " + SettingsFilename)]
	public static void ShowSettingsFile()
	{
		EditorUtility.RevealInFinder( SettingsPath );
	}
#endif


#if UNITY_EDITOR
	[MenuItem("NewChromantics/SteamVR/Show " + NullDriver + " driver settings")]
	public static void ShowDriverSettingsFile()
	{
		EditorUtility.RevealInFinder( GetDriverFolder(NullDriver));
	}
#endif

	//	returns if changed
#if UNITY_EDITOR
	[MenuItem("NewChromantics/SteamVR/enable " + NullDriver +" driver")]
#endif
	public static bool EnableNullDriver()
	{
		//	gr: for OSX. March 7th 2018
		//	enable the driver and make it always activate
		//	general settings dont NEED requireHmd nor forcedDriver, just activateMultipleDrivers
		var Changed = false;
		Changed |= ChangeSettings(NullDriver,DriverEnableKey,true);
		if ( ManifestFilename != null)
			Changed |= ChangeManifest(NullDriver, AlwaysActivateKey, true);
		Changed |= ChangeSettings(null, ActivateMultipleDriversKey, true);

#if UNITY_EDITOR_OSX //|| UNITY_STANDALONE_OSX
		if (Changed)
		{
			EditorUtility.DisplayDialog("Settings may not yet apply", "After changing settings, the steamvr runtime may need to be restarted.\n\nClosing Unity and/or steamvr may not be enough.\n\nOpen activity monitor and kill the process called vrserver", "ok");
		}
#endif

		return Changed;
	}

	public static bool ChangeSettings(string DriverName, string Key, bool Value)
	{
		var Filename = GetDriverSettingsPath(DriverName);
		return ChangeFile(Filename, Key, Value,DriverName);
	}

	public static bool ChangeManifest(string DriverName, string Key, bool Value)
	{
		var Filename = GetDriverManifestPath(DriverName);
		return ChangeFile(Filename, Key, Value,DriverName);
	}

	public static bool ChangeFile(string Filename,string Key,bool Value,string DriverName)
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
				return false;
			}
			System.IO.File.WriteAllText(Filename, NewContents);
			Debug.Log("Updated " + DriverName + " settings.");
			return true;
		}
		catch (System.Exception e)
		{
			Debug.LogError("Error replacing " + Key + " in " + DriverName + " settings...");
			Debug.LogException(e);
			throw;
		}
	}


}
