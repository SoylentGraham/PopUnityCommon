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
	#if UNITY_EDITOR
	string	mTitle;
	#endif

	public ScopedProgressBar(string Title)
	{
		#if UNITY_EDITOR
		mTitle = Title;
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

	//	use NotifyEveryNth (eg.=100) if you're doing large sets as the GUI update will slow down your thread, and you probably don't need to see progress for every one in 300,000 steps
	public void SetProgress(string StepName,int Step,int StepCount,int NotifyEveryNth=1)
	{
		bool Notify = (Step % Mathf.Max(1,NotifyEveryNth)) == 0;
		if (!Notify)
			return;
		StepName += " " + Step + "/" + StepCount;
		SetProgress (StepName, Step / (float)StepCount);
	}

	public void SetProgress(string StepName,float Progress)
	{
		#if UNITY_EDITOR
		if (EditorUtility.DisplayCancelableProgressBar (mTitle, StepName, Progress))
			throw new System.Exception (mTitle + " cancelled");
		#endif
	}
};

