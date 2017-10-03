using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnableEvent : MonoBehaviour {

	public UnityEngine.Events.UnityEvent	OnEnabled;
	public UnityEngine.Events.UnityEvent	OnDisabled;

	void OnEnable()
	{
		OnEnabled.Invoke ();
	}

	void OnDisable()
	{
		OnDisabled.Invoke ();
	}

}
