using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif



[System.AttributeUsage(System.AttributeTargets.Field)]
public class FilePathAttribute : PropertyAttribute
{
	public enum PathType
	{
		File,
		FileInProject,
		FileInStreamingAssets,
		Folder
	};
	public enum DialogType
	{
		Open,
		Save,
	};
#if UNITY_EDITOR
	PathType	pathType;
	DialogType	dialogType = DialogType.Save;
	string		fileType = "";
#endif

	public FilePathAttribute(PathType _pathType)
	{
		#if UNITY_EDITOR
		this.pathType = _pathType;
		#endif
	}
	public FilePathAttribute(string _fileType, PathType _pathType = PathType.File, DialogType dialogType = DialogType.Save)
	{
		#if UNITY_EDITOR
		this.fileType = _fileType;
		this.dialogType = dialogType;
		this.pathType = _pathType;
		#endif
	}

	#if UNITY_EDITOR
	public bool PathExists(string CurrentFilename)
	{
		try
		{
			var Path = System.IO.Path.GetFullPath(CurrentFilename);

			switch ( this.pathType )
			{
			case PathType.File:
			case PathType.FileInProject:
			case PathType.FileInStreamingAssets:
				return System.IO.File.Exists(Path);

			case PathType.Folder:
				return System.IO.Directory.Exists(Path);
			}
		}
		catch
		{
		}
		return false;
	}
#endif

	#if UNITY_EDITOR
	public string BrowseForPath(string PanelTitle,string CurrentFilename)
	{
		var Dir = "";
		try
		{
			Dir = System.IO.Path.GetFullPath (CurrentFilename);
		}
		catch{
		}

		var Filename = CurrentFilename;
		try
		{
			Filename = System.IO.Path.GetFileName (CurrentFilename);
		}
		catch{
		}

		System.Func<string, string, string> OpenFilePanelInProject = (Title, FileType) =>
		{
			if (string.IsNullOrEmpty(Dir))
				Dir = PopX.IO.Application_ProjectPath;
			var Path = EditorUtility.OpenFilePanel(Title, Dir, FileType);
			return PopX.IO.GetProjectRelativePath(Path);
		};

		System.Func<string, string, string> OpenFilePanelInStreamingAssets = (Title, FileType) =>
		{
			if (string.IsNullOrEmpty(Dir))
				Dir = Application.streamingAssetsPath;
			var Path = EditorUtility.OpenFilePanel(Title, Dir, FileType);
			return PopX.IO.GetStreamingAssetsRelativePath(Path);
		};


		if (string.IsNullOrEmpty (Filename))
			Filename = "Filename";

		if (pathType == PathType.File && dialogType == DialogType.Save)
			return EditorUtility.SaveFilePanel(PanelTitle, Dir, Filename, fileType);

		if (pathType == PathType.File && dialogType == DialogType.Open)
			return EditorUtility.OpenFilePanel(PanelTitle, Dir, fileType);

		if (pathType == PathType.FileInProject && dialogType == DialogType.Save)
			return EditorUtility.SaveFilePanelInProject(PanelTitle, Filename, fileType, "");
		
		if (pathType == PathType.FileInProject && dialogType == DialogType.Open)
			return OpenFilePanelInProject(PanelTitle, fileType);

		if (pathType == PathType.FileInStreamingAssets && dialogType == DialogType.Open)
			return OpenFilePanelInStreamingAssets(PanelTitle, fileType);

		if (pathType == PathType.Folder && dialogType == DialogType.Save)
			return EditorUtility.SaveFolderPanel(PanelTitle, Dir, Filename);

		if (pathType == PathType.Folder && dialogType == DialogType.Open)
			return EditorUtility.OpenFolderPanel(PanelTitle, Dir, Filename);

		throw new System.Exception ("Unhandled path/dialog type " + pathType + "/" + dialogType);
	}
	#endif
}



#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(FilePathAttribute))]
public class FilePathAttributePropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var Attrib = (FilePathAttribute)attribute;
		var TargetObject = property.serializedObject.targetObject;

		bool ShowShowButton = Attrib.PathExists (property.stringValue);

		//	calc sizes
		float BrowseWidthPercent = 0.16f;
		float ShowWidthPercent = ShowShowButton ? 0.16f : 0;

		var BrowseWidth = position.width * BrowseWidthPercent;
		var ShowWidth = position.width * ShowWidthPercent;
		var FieldWidth = position.width * (1 - BrowseWidthPercent - ShowWidthPercent);

		Rect FieldRect = new Rect (new Vector2(position.xMin,position.yMin), new Vector2 (FieldWidth, position.height));
		Rect ShowRect = new Rect ( new Vector2(FieldRect.xMax,position.yMin), new Vector2 (ShowWidth, position.height));
		Rect BrowseRect = new Rect ( new Vector2(ShowRect.xMax,position.yMin), new Vector2 (BrowseWidth, position.height));

		//	draw browse button
		if (GUI.Button (BrowseRect, "Browse..."))
		{
			var CurrentPath = property.stringValue;
			var NewPath = Attrib.BrowseForPath ( TargetObject.name, CurrentPath );

			//	empty = cancelled
			if ( !string.IsNullOrEmpty(NewPath))
			{
				property.stringValue = NewPath;
				property.serializedObject.ApplyModifiedProperties();
			}
			GUIUtility.ExitGUI();
		}

		if (ShowShowButton) {
			if (GUI.Button (ShowRect, "Show"))
			{
				try
				{
					var CurrentPath = property.stringValue;
					CurrentPath = System.IO.Path.GetFullPath (CurrentPath);
					EditorUtility.RevealInFinder (CurrentPath);
				}
				catch
				{
				}
			}
		}



		//	draw normal field
		EditorGUI.PropertyField (FieldRect, property, label, true);
	}
	/*
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		var Attrib = (ShowIfAttribute)attribute;
		var TargetObject = property.serializedObject.targetObject;

		if ( IsVisible(TargetObject,Attrib)) {
			//base.OnGUI (position, prop, label);
			return base.GetPropertyHeight ( property,  label);
		}
		return 0;
	}
*/
}
#endif


