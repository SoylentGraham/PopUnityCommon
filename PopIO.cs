using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//	gr: PopX being renamed to Pop later
namespace PopX
{
	public static class IO
	{
		static public string	Application_ProjectPath	{	get { return Application.dataPath.Split (new string[]{ "/Assets" }, System.StringSplitOptions.None) [0]; } }

		public static void WriteStringToFile (string Filename, string Data)
		{
			var FileHandle = System.IO.File.CreateText( Filename );
			FileHandle.Write(Data);
			FileHandle.Close();
		}


		//	throw if not project relative
		public static string GetProjectRelativePath(string Path)
		{
			//	gr: this seems to always return an empty string...
			//UnityEditor.FileUtil.GetProjectRelativePath (Path);

			if (string.IsNullOrEmpty (Path))
				return Path;

			var ProjectPath = Application_ProjectPath;

			if (!Path.StartsWith (ProjectPath))
				throw new System.Exception ("Path " + Path + " is not project relative (" + ProjectPath + ")");

			Path = Path.Remove (0, ProjectPath.Length);
			return Path;
		}

	}
}

