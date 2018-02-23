using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//	gr: PopX being renamed to Pop later
namespace PopX
{
	public static class IO
	{
		//	file types we can write/export
		//	gr: value used as extension (todo; support multiple extensions per type, eg. jpeg and jpg)
		public enum ImageFileType
		{
			png,
			jpg,
			exr
		};

		static public string Application_ProjectPath { get { return Application.dataPath.Split(new string[] { "/Assets" }, System.StringSplitOptions.None)[0]; } }

		public static void WriteStringToFile(string Filename, string Data)
		{
			var FileHandle = System.IO.File.CreateText(Filename);
			FileHandle.Write(Data);
			FileHandle.Close();
		}


		//	throw if not project relative
		public static string GetProjectRelativePath(string Path)
		{
			//	gr: this seems to always return an empty string...
			//UnityEditor.FileUtil.GetProjectRelativePath (Path);

			if (string.IsNullOrEmpty(Path))
				return Path;

			var ProjectPath = Application_ProjectPath;

			if (!Path.StartsWith(ProjectPath))
				throw new System.Exception("Path " + Path + " is not project relative (" + ProjectPath + ")");

			Path = Path.Remove(0, ProjectPath.Length);
			return Path;
		}

#if UNITY_EDITOR
		public static void SaveFile(string DefaultName, ImageFileType Extension, System.Action<string, ImageFileType> DoSave)
		{
			var ExtensionsString = Extension.ToString();
			var DefaultDirectory = Application.dataPath;

			var Filename = UnityEditor.EditorUtility.SaveFilePanel("Save as...", DefaultDirectory, DefaultName, ExtensionsString);
			if (string.IsNullOrEmpty(Filename))
				throw new System.Exception("Save cancelled");

			/*	gr: might still want this on windows if the user can choose a different extension. osx won't allow it
			//	get the full path
			//	work out the extension used
			ImageFileType? Extension = null;
			foreach (var Ext in Extensions)
				if (Filename.EndsWith(Ext.ToString()))
					Extension = Ext;
			if (!Extension.HasValue)
				throw new System.Exception("Couldn't determine file extension selected by user");
			*/

			DoSave(Filename, Extension);
		}
		#endif
	}
}

