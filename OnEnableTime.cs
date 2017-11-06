using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnableTime : MonoBehaviour {

	public UnityEvent_float	OnEnabledTime;
	public UnityEvent_float	OnDisabledTime;

	void OnEnable()
	{
		OnEnabledTime.Invoke (Time.time);
	}

	void OnDisable()
	{
		OnDisabledTime.Invoke (Time.time);
	}
}
