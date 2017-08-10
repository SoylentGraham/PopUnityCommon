using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class PopRestartEditor : MonoBehaviour {

	static string	ArgForceOpengl = "-force-opengl";

#if UNITY_EDITOR
	[MenuItem("NewChromantics/Restart Editor in opengl")]
	static void RestartEditorInOpengl()
	{
		//var UnityExe = System.Environment.GetCommandLineArgs()[0];
		//var UnityExe = EditorApplication.applicationPath;
		//System.Diagnostics.Process.Start( UnityExe, ArgForceOpengl );
		EditorApplication.OpenProject("./", ArgForceOpengl);
	}
#endif
}

