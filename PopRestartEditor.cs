using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class PopRestartEditor : MonoBehaviour {

	static string	Application_ProjectPath	{	get {	return FilePathAttribute.Application_ProjectPath;}}

#if UNITY_EDITOR
	//	https://docs.unity3d.com/Manual/CommandLineArguments.html
	//	gr: is this glcore now? opengl still works on windows, even after this changed...
	static string	ArgForceOpengl = "-force-opengl";

	//	not documented yet; https://forum.unity.com/threads/hair-tool-1-1-0.446431/page-5#post-3058637
	static string	ArgForceMetal = "-force-metal";
#endif

#if UNITY_EDITOR
	[MenuItem("Editor/Restart Editor")]
	static void RestartEditor()
	{
		//var UnityExe = System.Environment.GetCommandLineArgs()[0];
		//var UnityExe = EditorApplication.applicationPath;
		//System.Diagnostics.Process.Start( UnityExe, ArgForceOpengl );
		var ProjectPath = Application_ProjectPath;
		//var ProjectPath = "./";	//	shows open-project dialog
		EditorApplication.OpenProject(ProjectPath);
	}
	#endif

#if UNITY_EDITOR
	[MenuItem("Editor/Restart Editor in Opengl")]
	static void RestartEditorInOpengl()
	{
		//var UnityExe = System.Environment.GetCommandLineArgs()[0];
		//var UnityExe = EditorApplication.applicationPath;
		//System.Diagnostics.Process.Start( UnityExe, ArgForceOpengl );
		var ProjectPath = Application_ProjectPath;
		//var ProjectPath = "./";	//	shows open-project dialog
		EditorApplication.OpenProject(ProjectPath, ArgForceOpengl);
	}
#endif


#if UNITY_EDITOR && UNITY_EDITOR_OSX
	[MenuItem("Editor/Restart Editor in Metal")]
	static void RestartEditorInMetal()
	{
		//var UnityExe = System.Environment.GetCommandLineArgs()[0];
		//var UnityExe = EditorApplication.applicationPath;
		//System.Diagnostics.Process.Start( UnityExe, ArgForceOpengl );
		var ProjectPath = Application_ProjectPath;
		//var ProjectPath = "./";	//	shows open-project dialog
		EditorApplication.OpenProject(ProjectPath, ArgForceMetal);
	}
#endif


}

