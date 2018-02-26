using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.UI;


[System.Serializable]
public class UnityEvent_OnProgress : UnityEvent <float> {}



public class PopTaskProgressBar : MonoBehaviour {

	public UnityEvent_OnProgress	OnProgress;
	public UnityEvent				OnComplete;

	[Header("UI progress bar")]
	public Slider					ProgressSlider;
	public Button					CancelButton;
	public Text						DescriptionText;

	string	ProgressName;
	int		CurrentStep;
	bool	Cancelled;
	uint	StepCount;


	public void InitProgress(string ProgressName,uint StepCount)
	{
		this.ProgressName = ProgressName;
		CurrentStep = -1;
		Cancelled = false;
		this.StepCount = StepCount;

		
		if (ProgressSlider != null)
		{
			ProgressSlider.maxValue = StepCount;
			ProgressSlider.minValue = 0;
			ProgressSlider.value = 0;
			ProgressSlider.wholeNumbers = true;
		}

		if ( DescriptionText != null )
		{
			DescriptionText.text = ProgressName;
		}

		if ( CancelButton != null )
		{
			UnityAction Cancel = () => 
			{
				Cancelled = true;
			};
			CancelButton.onClick.AddListener( Cancel );
		}

	}

	

	public void UpdateProgress(string Description)
	{
		CurrentStep++;

		float Progress = CurrentStep / (float)StepCount;

		try
		{
			OnProgress.Invoke(Progress);
		}
		catch (System.Exception e)
		{
			Debug.LogException( e );
		}

		//	update editorui
#if UNITY_EDITOR
		if ( EditorUtility.DisplayCancelableProgressBar( ProgressName, Description, Progress ) )
		{
			Cancelled = true;
		}
#endif
		//	update ui
		if ( ProgressSlider != null )
		{
			ProgressSlider.value = CurrentStep;			
		}
		if ( DescriptionText != null )
		{
			DescriptionText.text = Description;
		}

		if (Cancelled)
		{
			FinishProgress(false);
			throw new System.Exception( ProgressName + " cancelled");
		}
	}


	public void FinishProgress(bool Success=true)
	{
#if UNITY_EDITOR
		EditorUtility.ClearProgressBar();
#endif
		if ( DescriptionText != null )
		{
			if ( Success )
				DescriptionText.text = ProgressName + " complete";
			else
				DescriptionText.text = ProgressName + " failed";
		}
		if ( ProgressSlider != null )
		{
			if ( Success )
				ProgressSlider.value = ProgressSlider.maxValue;			
		}

		try
		{
			if ( Success )
				OnComplete.Invoke();
		}
		catch (System.Exception e)
		{
			Debug.LogException( e );
		}

	}



	}
