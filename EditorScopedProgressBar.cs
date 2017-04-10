using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*
To make this go out of scope without having to call YourProgress.Dispose() or GC.Collect, use the Using keyword

	using (var Progress = new ScopedProgressBar())
	{
	     ...
	}
*/
public class ScopedProgressBar : System.IDisposable
{
	string	mTitle;

	public ScopedProgressBar(string Title)
	{
		mTitle = Title;
		#if UNITY_EDITOR
		EditorUtility.DisplayProgressBar (mTitle, "...", 0.0f);
		#endif
	}

	~ScopedProgressBar()
	{
		//	can't use destructor, not on the unity main thread. have to use disposable interface.
		//Dispose ();
	}

	public void Dispose()
	{
		#if UNITY_EDITOR
		EditorUtility.ClearProgressBar();
		#endif
	}

	public void SetProgress(string StepName,float Progress)
	{
		#if UNITY_EDITOR
		if (EditorUtility.DisplayCancelableProgressBar (mTitle, StepName, Progress))
			throw new System.Exception (mTitle + " cancelled");
		#endif
	}
};

