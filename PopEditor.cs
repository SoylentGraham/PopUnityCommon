using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PopX
{
	public static class Editor
	{
		static string Application_ProjectPath { get { return PopX.IO.Application_ProjectPath; } }

#if UNITY_EDITOR
		//	https://docs.unity3d.com/Manual/CommandLineArguments.html
		//	gr: is this glcore now? opengl still works on windows, even after this changed...
		static string ArgForceOpengl = "-force-opengl";

		//	not documented yet; https://forum.unity.com/threads/hair-tool-1-1-0.446431/page-5#post-3058637
		static string ArgForceMetal = "-force-metal";
#endif

#if UNITY_EDITOR
		[MenuItem("NewChromantics/Editor/Restart Editor")]
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
		[MenuItem("NewChromantics/Editor/Restart Editor in Opengl")]
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
		[MenuItem("NewChromantics/Editor/Restart Editor in Metal")]
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

#if UNITY_EDITOR
		[MenuItem("NewChromantics/Editor/Enable Gizmos in all game views")]
		public static void EnableGizmos_Menu()
		{
			EnableGizmos(true);
		}
#endif

#if UNITY_EDITOR
		[MenuItem("NewChromantics/Editor/Disable Gizmos in all game views")]
		public static void DisableGizmos_Menu()
		{
			EnableGizmos(false);
		}
#endif

#if UNITY_EDITOR
		public static List<EditorWindow> GetAllEditorWindows(System.Type FilterType=null)
		{
			if (FilterType == null)
				FilterType = typeof(EditorWindow);
					
			var EditorWindowObjects = Resources.FindObjectsOfTypeAll(FilterType);
			var EditorWindows = new List<EditorWindow>();
			foreach ( var ewo in EditorWindowObjects )
			{
				var ew = ewo as EditorWindow;
				EditorWindows.Add(ew);
			}
			return EditorWindows;
		}
#endif


#if UNITY_EDITOR
		public static List<EditorWindow> GetAllInspectorWindows()
		{
			return GetAllEditorWindows("UnityEditor.InspectorWindow");
		}
#endif

#if UNITY_EDITOR
		public static List<EditorWindow> GetAllEditorWindows(string EditorWindowTypeName)
		{
			var EditorAsm = typeof(Editor).Assembly;
			var WindowType = EditorAsm.GetType(EditorWindowTypeName);

			return GetAllEditorWindows(WindowType);
		}
#endif

		//	from https://answers.unity.com/questions/851470/how-to-hide-gizmos-by-script.html
		//	^^ outdated, but still might be useful reference.
		public static void EnableGizmos(bool Enable)
		{
#if UNITY_EDITOR
			var GameViewTypename = "UnityEditor.GameView";
			var GizmoPropertyName = "m_Gizmos";
			System.Type GameViewType = null;
			//var asm = Assembly.GetAssembly(typeof(Editor));
			foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
			{
				var t = asm.GetType(GameViewTypename, false, true);
				if (t == null)
					continue;
				GameViewType = t;
				break;
			}

			//	find the gizmo variable
			//	https://github.com/Unity-Technologies/UnityCsReference/blob/83cceb769a97e24025616acc7503e9c21891f0f1/Editor/Mono/GameView/GameView.cs#L61
			var Binding = BindingFlags.Instance | BindingFlags.NonPublic;
			//var GizmoProperty = GameViewType.GetProperty(GizmoPropertyName, Binding);
			var GizmoField = GameViewType.GetField(GizmoPropertyName, Binding);

			var GameViewWindows = GetAllEditorWindows(GameViewType);

			foreach (var gvw in GameViewWindows)
			{
				GizmoField.SetValue(gvw, Enable);
				gvw.Repaint();
			}
#endif
		}


#if UNITY_EDITOR
		[MenuItem("NewChromantics/Editor/Reset Player Prefs")]
		public static void ResetPlayerPrefs()
		{
			PlayerPrefs.DeleteAll();
		}
#endif


	}

}

