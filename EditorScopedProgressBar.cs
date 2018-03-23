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
	bool ShowProgressBar = true;
	int NotifyCounter = 0;
	#endif

	//	gr: added a simple bool to disable rendering of the progress bar so caller can quickly turn on&off and still use using()
	public ScopedProgressBar(string Title,bool ShowProgressBar=true)
	{
		#if UNITY_EDITOR
		this.ShowProgressBar = ShowProgressBar;
		this.mTitle = Title;
		if ( ShowProgressBar )
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
		if (!ShowProgressBar)
			return;
		EditorUtility.ClearProgressBar();
		#endif
	}

	//	use NotifyEveryNth (eg.=100) if you're doing large sets as the GUI update will slow down your thread, and you probably don't need to see progress for every one in 300,000 steps
	public void SetProgress(string StepName,int Step,int StepCount,int NotifyEveryNth=1)
	{
		//	gr: we now have a notify counter for steps which aren't linear
		//		eg. parsing chunks of a file that skip lines
		NotifyCounter++;
		bool Notify = (NotifyCounter % Mathf.Max(1,NotifyEveryNth)) == 0;
		Notify |= (Step % Mathf.Max(1,NotifyEveryNth)) == 0;

		if (!Notify)
			return;
		StepName += " " + Step + "/" + StepCount;
		SetProgress (StepName, Step / (float)StepCount);
	}

	public void SetProgress(string StepName,float Progress)
	{
		#if UNITY_EDITOR
		if (!ShowProgressBar)
			return;
		if (EditorUtility.DisplayCancelableProgressBar (mTitle, StepName, Progress))
			throw new System.Exception (mTitle + " cancelled");
		#endif
	}
};

