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
		Folder
	};
	PathType	pathType;
	string		fileType = "";

	public FilePathAttribute(PathType _pathType)
	{
		this.pathType = _pathType;
	}
	public FilePathAttribute(string _fileType,PathType _pathType=PathType.File)
	{
		this.fileType = _fileType;
		this.pathType = _pathType;
	}

	public bool PathExists(string CurrentFilename)
	{
		try
		{
			var Path = System.IO.Path.GetFullPath(CurrentFilename);

			switch ( this.pathType )
			{
			case PathType.File:
			case PathType.FileInProject:
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

		if (string.IsNullOrEmpty (Filename))
			Filename = "Filename";

		if ( pathType == PathType.File )
			return EditorUtility.SaveFilePanel(PanelTitle, Dir, Filename, fileType);

		if ( pathType == PathType.FileInProject )
			return EditorUtility.SaveFilePanelInProject(PanelTitle, Filename, fileType, "" );

		if (pathType == PathType.Folder)
			return EditorUtility.SaveFolderPanel (PanelTitle, Dir, Filename);

		throw new System.Exception ("Unhandled path type " + pathType);
	}
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
		float BrowseWidthPercent = 0.12f;
		float ShowWidthPercent = ShowShowButton ? 0.10f : 0;

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
			if ( !string.IsNullOrEmpty(NewPath)) {
				property.stringValue = NewPath;
			}
		}

		if (ShowShowButton) {
			if (GUI.Button (ShowRect, "Show")) {
				try {
					var CurrentPath = property.stringValue;
					CurrentPath = System.IO.Path.GetFullPath (CurrentPath);
					EditorUtility.RevealInFinder (CurrentPath);
				} catch {
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


