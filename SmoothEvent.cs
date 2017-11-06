using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SmoothEvent : MonoBehaviour {

	[Header("Called every frame")]
	public UnityEvent_float		OnUpdate;
	public UnityEvent_float		OnUpdateInverse;

	public UnityEvent			OnFinished;
	public bool					DisableOnFinished = true;

	[Header("Curve will be stretched to this duration")]
	[Range(0.01f,20.0f)]
	public float				Duration = 5;
	public AnimationCurve		Curve;

	public bool					OnDisableJumpToEnd = false;

	public bool					ClampOutput01 = false;

	float				StartTime = 0;


	void OnEnable()
	{
		StartTime = Time.time;
		Update ();
	}

	void OnDisable()
	{
		if ( OnDisableJumpToEnd )
		{
			StartTime = Time.time - Duration;
			Update();
		}
	}


	void Update () {

		float TimePassed = Time.time - StartTime;


		if (TimePassed > Duration) {
			OnFinished.Invoke ();
			TimePassed = Duration;

			if (DisableOnFinished)
				this.enabled = false;
		}

		float Value = 0;
		if (Curve.keys.Length > 1) {
			var t = TimePassed / Duration;
			var CurveDuration = Curve.keys [Curve.length - 1].time;
			t = t * CurveDuration;
			Value = Curve.Evaluate (t);
		} else {
			Value = TimePassed / Duration;
		}

		if (ClampOutput01)
			Value = Mathf.Clamp01 (Value);

		OnUpdate.Invoke (Value);
		OnUpdateInverse.Invoke (1 - Value);
	
	}
}
