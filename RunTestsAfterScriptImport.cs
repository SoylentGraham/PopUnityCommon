using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools;
/*	
 Automatically run unit tests after scripts are changed.
 Like how unit tests are supposed to work! (break immediately after compile, not later!)

	https://answers.unity.com/questions/775869/editor-how-to-add-checkmarks-to-menuitems.html
*/
[InitializeOnLoad]
class RunTestsAfterScriptImport : AssetPostprocessor
{
	const string MenuTitle_Enabled = "Editor/Run unit tests after script import";
	const string MenuTitle_Run = "Editor/Run unit tests";
	const string SaveName = "RunTestsAfterScriptImport";

	static bool		RunTestsEnabled = true;

	/// Called on load thanks to the InitializeOnLoad attribute
	static RunTestsAfterScriptImport()
	{
		RunTestsEnabled = EditorPrefs.GetBool(SaveName, RunTestsEnabled);

		/// Delaying until first editor tick so that the menu
		/// will be populated before setting check state, and
		/// re-apply correct action
		EditorApplication.delayCall += () => {
			SetEnabled(RunTestsEnabled);
		};
	}

	[MenuItem(MenuTitle_Enabled)]
	static void ToggleEnabled()
	{
		SetEnabled(!RunTestsEnabled);
	}

	[MenuItem(MenuTitle_Run)]
	static void RunUnitTests()
	{
		//	until I can figure out how to use UnityEditor.TestRunner.dll... just run [test] funcs
		//UnityEngine.TestTools.TestRunner.
		//	UnityEditor.TestRunner.dll
		/*
		//UnityEditor.UnityTest.Batch.RunUnitTests();
		var assembly = System.Reflection.Assembly.GetExecutingAssembly();
		var Types = new List<System.Type> (assembly.GetTypes ());
		var Methods = Types
			.SelectMany (x => x.GetMethods ())
			.Where (y => y.GetCustomAttributes ().OfType<MethodAttribute> ().Any ())
			.ToDictionary (z => z.Name);
		*/
	}

	public static void SetEnabled(bool enabled)
	{
		RunTestsEnabled = enabled;

		//	update menu
		Menu.SetChecked(MenuTitle_Enabled, RunTestsEnabled);

		//	save settings
		EditorPrefs.SetBool(MenuTitle_Run, RunTestsEnabled);
	}


	//	probably a proper way to do this (without loading the asset)
	static bool IsScript(string AssetPath)
	{
		var Extension = ".cs";
		if (AssetPath.ToLower().EndsWith(Extension))
			return true;

		return false;
	}

	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		var AnyScriptsChanged = false;

		foreach (var str in importedAssets)
		{
			AnyScriptsChanged |= IsScript(str);
		}

		foreach (var str in deletedAssets)
		{
			AnyScriptsChanged |= IsScript(str);
		}

		foreach(var str in movedAssets)
		{
			AnyScriptsChanged |= IsScript(str);
		}

		if (AnyScriptsChanged)
			RunUnitTests();
	}
}