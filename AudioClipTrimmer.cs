using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System;
#endif

public class AudioClipTrimmer : EditorWindow {

	float Left = 0;
	float Right = 1;

	[MenuItem ("Window/Audio Clip Trimmer")]
	public static void  ShowWindow () {
        EditorWindow.GetWindow(typeof(AudioClipTrimmer));
    }
	AudioClip GetAudioClip()
	{ 
		var Clips = UnityEditor.Selection.GetFiltered<AudioClip>( SelectionMode.Assets );
		return Clips[0];
	}

	//	from http://answers.unity3d.com/questions/993241/how-to-play-specific-part-of-the-audio.html
	private AudioClip MakeSubclip(AudioClip clip, float start, float stop)
	{
		/* Create a new audio clip */
		int frequency = clip.frequency;
		float timeLength = stop - start;
		int samplesLength = (int)(frequency * timeLength);
		AudioClip newClip = AudioClip.Create(clip.name + "-sub", samplesLength, 1, frequency, false);
		/* Create a temporary buffer for the samples */
		float[] data = new float[samplesLength];
		/* Get the data from the original clip */
		clip.GetData(data, (int)(frequency * start));
		/* Transfer the data to the new clip */
		newClip.SetData(data, 0);
		/* Return the sub clip */
		return newClip;
	}

	public static void PlayClip(AudioClip clip, float startTime, bool loop)
	{
		int startSample = (int)(startTime * clip.frequency);
  
		Assembly assembly = typeof(AudioImporter).Assembly;
		Type audioUtilType = assembly.GetType("UnityEditor.AudioUtil");
  
		Type[] typeParams = { typeof(AudioClip), typeof(int), typeof(bool) };
		object[] objParams = { clip, startSample, loop };
  
		MethodInfo method = audioUtilType.GetMethod("PlayClip", typeParams);
		method.Invoke(null, BindingFlags.Static | BindingFlags.Public, null, objParams, null);
	}

	void ShowInspector(AudioClip Clip)
	{
		var Preview = AssetPreview.GetAssetPreview(Clip);

		//GUILayout.Label( Preview, GUILayout.ExpandWidth(true) );
		//Rect.y += EditorGUILayout.GetControlRect( null).height;

		Left = GUILayout.HorizontalSlider( Left, 0, 1, GUILayout.ExpandWidth(true) );
		Left = Mathf.Min( Left, Right );
		//Rect.y += EditorGUILayout.GetControlRect(null).height * 2;

		Right = GUILayout.HorizontalSlider( Right, 0, 1, GUILayout.ExpandWidth(true) );
		Right = Mathf.Max( Left, Right );
		//Rect.y += EditorGUILayout.GetControlRect(null).height * 2;

		if ( GUILayout.Button("Preview", GUILayout.ExpandWidth(false) ) )
		{
			var SubClip = MakeSubclip( Clip, Left*Clip.length, Right * Clip.length );
			PlayClip( SubClip, Left, false );
		}
		//Rect.y += EditorGUILayout.GetControlRect(null).height * 2;

		if ( GUILayout.Button("Save As...", GUILayout.ExpandWidth(false) ) )
		{	
			var SubClip = MakeSubclip( Clip, Left*Clip.length, Right * Clip.length );
			AssetWriter.WriteAsset	( Clip.name + " SubClip", SubClip );	
		}


		{
			GUIStyle style = new GUIStyle();
			style.fixedHeight = 100.0f;
			var Options = new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) };
			var GroupRect = GUILayoutUtility.GetRect(new GUIContent(),style, Options );
			GUI.BeginGroup( GroupRect );
			GroupRect.x = 0;
			GroupRect.y = 0;
			EditorGUILayout.BeginVertical ();
			GUI.DrawTextureWithTexCoords( GroupRect, Preview, new Rect( Left, 0, Right-Left, 1 ) );
			EditorGUILayout.EndVertical ();
			GUI.EndGroup();
		}
	}

	void ShowHelpInspector(string Message)
	{
		EditorGUILayout.HelpBox("Select an audio clip",MessageType.Info);
	}

	public void OnGUI()
    {
		try
		{
			var Clip = GetAudioClip();
			ShowInspector(Clip);
		}
		catch (System.Exception e)
		{
			ShowHelpInspector(e.Message);
		}       
		
    }
}

