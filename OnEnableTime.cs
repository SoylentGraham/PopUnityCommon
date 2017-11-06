using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnableTime : MonoBehaviour {

	public UnityEvent_float	OnEnabledTime;
	public UnityEvent_float	OnDisabledTime;

	[Header("Use timeSinceLevelLoad to match shader _Time")]
	//	https://answers.unity.com/questions/302335/built-in-shaderlab-timey-and-timetime-are-not-equa.html
	public bool				UseTimeSinceLevelLoad = true;

	void OnEnable()
	{
		OnEnabledTime.Invoke ( UseTimeSinceLevelLoad ? Time.timeSinceLevelLoad : Time.time );
	}

	void OnDisable()
	{
		OnDisabledTime.Invoke ( UseTimeSinceLevelLoad ? Time.timeSinceLevelLoad : Time.time );
	}
}
