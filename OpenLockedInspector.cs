using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class OpenLockedInspector
{
	#if UNITY_EDITOR
	[MenuItem("GameObject/Open Locked Inspector", false, 49)]
	public static void DoOpenLockedInspector()
	{
		var InspectorType = typeof(Editor).Assembly.GetType("UnityEditor.InspectorWindow");
		//var Inspector = EditorWindow.GetWindow(InspectorType) as EditorWindow;
		var NewInspector = EditorWindow.CreateInstance (InspectorType) as EditorWindow;
		NewInspector.Show ();

		//	todo: explicitly set object

		//	try and lock it
		//	https://github.com/MattRix/UnityDecompiled/blob/master/UnityEditor/UnityEditor/InspectorWindow.cs
		try
		{	
			var IsLockedProp = InspectorType.GetProperty("isLocked", typeof(bool) );
			if ( !IsLockedProp.CanWrite )
				throw new System.Exception("UnityEditor.InspectorWindow.isLocked cannot be written");
			IsLockedProp.SetValue( NewInspector, true, null );
		}
		catch(System.Exception e) {
			Debug.LogError ("Failed to lock new inspector: " + e.Message);
		}

	}
	#endif
}
