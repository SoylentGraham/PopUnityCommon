#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System;


public class PopBuild : MonoBehaviour {

	public static string[]	Levels = {};
	private static string	BuildPathArg = "-BuildPath=";

	public static string	GetCommandLineBuildPath()
	{
		string[] Args = Environment.GetCommandLineArgs ();
		try
		{
			string BuildPath = Args.Where( Element => Element.StartsWith(BuildPathArg)).Single();
			//	strip predicate
			BuildPath = BuildPath.Replace(BuildPathArg, "");
			BuildPath = BuildPath.Replace("\"", "" );
			BuildPath = BuildPath.Trim();
			Debug.Log("Build path found as: " + BuildPath );
			if ( !BuildPath.EndsWith(".apk") )
				return null;
			return BuildPath;
		}
		catch(System.Exception e)
		{
			Debug.Log("failed to get BuildPath arg: " + e.Message );
			return null;
		}
	}

	public static void BuildAndroid()
	{
		string Path = GetCommandLineBuildPath ();
		if ( Path==null )
		{
			string Error = "No path (ending in .apk) supplied on commandline. use " + BuildPathArg + "\"yourpath/filename.apk\"";
			throw new UnityException(Error);
		}

		BuildPipeline.BuildPlayer (Levels, Path, BuildTarget.Android, BuildOptions.None);
	}

}
#endif//UNITY_EDITOR