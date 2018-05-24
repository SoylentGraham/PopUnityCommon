#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System;


/*
 * gr: this is being deprecated, for "PopBuild" a more complete build solution, but keeping this here temporarily until all funcitonality is replaced.
 * so PopBuild has been renamed to PopCommandLineBuild
 */
public class PopCommandLineBuild : MonoBehaviour {

	private static string	BuildPathArg = "-BuildPath=";
	//private static string 	PathSuffixIos = "/";
	private static string 	PathSuffixIos = null;
	private static string 	PathSuffixAndroid = ".apk";

	private static string[] GetLevels()
	{
		return (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray();
	}

	public static string	GetCommandLineBuildPath(string RequiredSuffix)
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

			if ( RequiredSuffix != null )
				if ( !BuildPath.EndsWith(RequiredSuffix) )
					return null;

			return BuildPath;
		}
		catch(System.Exception e)
		{
			Debug.Log("failed to get BuildPath arg: " + e.Message );
			return null;
		}
	}


	public static void Build(BuildTarget Target,string RequiredBuildPathSuffix)
	{
		string Path = GetCommandLineBuildPath (RequiredBuildPathSuffix);
		if ( Path==null )
		{
			string Error = "No path (ending in "+RequiredBuildPathSuffix+") supplied on commandline. use " + BuildPathArg + "\"yourpath/filename" + RequiredBuildPathSuffix + "\"";
			throw new UnityException(Error);
		}
		BuildPipeline.BuildPlayer (GetLevels(), Path, Target, BuildOptions.None);
	}

	public static void BuildAndroid()
	{
		Build (BuildTarget.Android, PathSuffixAndroid);
	}

	public static void BuildIos()
	{
		Build (BuildTarget.iOS, PathSuffixIos);
	}

}
#endif//UNITY_EDITOR