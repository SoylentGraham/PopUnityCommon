using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//	gr: PopX being renamed to Pop later
namespace PopX
{
	public static class IO
	{
		//	file types we can write/export
		//	gr: value used as extension (todo; support multiple extensions per type, eg. jpeg and jpg)
		//	gr: allow jpeg quality settings... these may need to turn into a TYPE with options.
		public enum ImageFileType
		{
			png,
			jpg,
			exr
		};

		static public string GetImageFormatExtension(ImageFileType FileType) { return FileType.ToString(); 	}

		static public string Application_ProjectPath { get { return Application.dataPath.Split(new string[] { "/Assets" }, System.StringSplitOptions.None)[0]; } }

		public static void WriteStringToFile(string Filename, string Data)
		{
			var FileHandle = System.IO.File.CreateText(Filename);
			FileHandle.Write(Data);
			FileHandle.Close();
		}

		public static System.Action<string> GetFileWriteLineFunction(string Filename)
		{
			var Stream = new System.IO.StreamWriter(Filename);

			//	gr: as we're holding onto the stream, it won't flush/close automatically (are we leaking a handle?)
			//	unless we add another func to close/dispose the stream, we can auto flush and it'll just write immediately out
			Stream.AutoFlush = true;

			System.Action<string> WriteLine = (Line) =>
			{
				//	add line feed if it's not there
				//	todo: or clip line feed and use WriteLine?
				var LineFeed = "\n";
				if (Line == null)
					Line = "";
				if (!Line.EndsWith(LineFeed))
					Line += LineFeed;
				Stream.Write(Line);
			};
			return WriteLine;
		}

#if UNITY_EDITOR
		public static System.Action<string> GetFileWriteLineFunction(out string Filename,string FileDescription, string DefaultFilename, string FileExtension)
		{
			//	get filename
			var InitialDir = "Assets/";
			Filename = UnityEditor.EditorUtility.SaveFilePanel(FileDescription, InitialDir, DefaultFilename, FileExtension);
			if (string.IsNullOrEmpty(Filename))
				throw new System.Exception("Cancelled file save");

			return GetFileWriteLineFunction(Filename);
		}
#endif

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
		public static void SaveFile(string DefaultName, ImageFileType Extension, System.Action<ImageFileType,string> DoSave)
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

			DoSave(Extension,Filename);
		}
		#endif


#if UNITY_EDITOR
		public static void SaveFile(string DefaultName, Texture2D texture,ImageFileType Extension)
		{
			var DoSave = PopX.IO.GetFileWriteImageFunction(Extension);

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

			DoSave(Filename,texture);
		}
#endif

		//	cannot extend System.IO.Path
		public static string Path_Combine(IEnumerable<string> Paths)
		{
			string FullPath = null;
			foreach ( var Folder in Paths )
			{
				//	the test in here means we will return null if Paths is null/empty
				if (FullPath == null)
					FullPath = Folder;
				else
					FullPath = System.IO.Path.Combine(FullPath, Folder);
			}
			return FullPath;
		}

		public static System.Func<Texture2D,byte[]> GetEncodeImageFunction(ImageFileType Filetype)
		{
			switch ( Filetype )
			{
				case ImageFileType.exr: return (t) => { return t.EncodeToEXR(); };
				case ImageFileType.jpg: return (t) => { return t.EncodeToJPG(); };
				case ImageFileType.png: return (t) => { return t.EncodeToPNG(); };
				default:	throw new System.Exception("Unhandled file type " + Filetype);
			}
		}

		public static System.Action<string, Texture2D> GetFileWriteImageFunction(ImageFileType Filetype)
		{
			var EncodeFunc = GetEncodeImageFunction(Filetype);
			System.Action<string, Texture2D> WriteFunc = (Filename, Texture) =>
			{
				var Bytes = EncodeFunc(Texture);
				System.IO.File.WriteAllBytes( Filename, Bytes );
			};
			return WriteFunc;
		}



		#if UNITY_EDITOR
		static public string GetSelectedPath(out string Filename)
		{
			//	https://answers.unity.com/questions/472808/how-to-get-the-current-selected-folder-of-project.html
			//	selection may be null, an object, or the current folder that was right clicked in
			var Selected = Selection.activeObject;
			var SelectedPath = Selected ? AssetDatabase.GetAssetPath(Selected.GetInstanceID()) : null;
			if (string.IsNullOrEmpty(SelectedPath))
			{
				Filename = null;
				return "Assets";
			}

			//	just a directory
			if (System.IO.Directory.Exists(SelectedPath))
			{
				Filename = null;
				return SelectedPath;
			}

			//	file, split filename and folder
			var Folder = System.IO.Path.GetPathRoot(SelectedPath);
			Filename = System.IO.Path.GetFileName(SelectedPath);
			return Folder;
		}
#endif

		static public string Path_MakePath(string Folder,string Filename,string Extension)
		{
			if (Extension[0] != '.')
				Extension = "." + Extension;
			
			var Path = System.IO.Path.Combine(Folder, Filename) + Extension;
			return Path;
		}

		//	compliment System.IO.Path.ChangeExtension
		//	named to System.IO.Path.GetFileNameWithoutExtension
		static public string Path_ChangeFilenameWithoutExtension(string Path,string NewFilename)
		{
			var OldFilename = System.IO.Path.GetFileNameWithoutExtension(Path);
			var Extension = System.IO.Path.GetExtension(Path);
			var Folder = System.IO.Path.GetDirectoryName(Path);

			//	unit/sanity test!
			var OldPath = Path_MakePath(Folder, OldFilename, Extension );
			if (OldPath != Path)
				throw new System.Exception("Error with Path_ChangeFilenameWithoutExtension could not recreate OLD path before creating new");
			var NewPath = Path_MakePath(Folder, NewFilename, Extension);
			return NewPath;
		}

#if UNITY_EDITOR
		//	from the current selected/right click place, get a new path & filename
		static public string GetAssetFolderUnusedFilename(string DefaultFilename, string Extension)
		{
			string Filename;
			var Folder = GetSelectedPath(out Filename);

			if (Filename == null)
				Filename = DefaultFilename + "." + Extension;

			//	if filename's extension is different (eg. Cow.png, not Cow.asset) then change it
			Filename = System.IO.Path.ChangeExtension(Filename, Extension);

			var Path = System.IO.Path.Combine(Folder, Filename);

			//	don't overwrite existing filename
			int Count = 1;
			while (System.IO.File.Exists(Path))
			{
				var NewFilename = System.IO.Path.GetFileNameWithoutExtension(Filename);
				Count++;
				NewFilename += " " + Count;
				Path = Path_ChangeFilenameWithoutExtension(Path, NewFilename);
			}

			return Path;
		}
#endif
	}
}

