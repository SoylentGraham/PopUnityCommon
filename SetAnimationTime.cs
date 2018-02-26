//	gr: actually the simpler solution.
#define PAUSE_VIA_ENABLE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class SetAnimationTime : MonoBehaviour {

	#if !PAUSE_VIA_ENABLE
	float?	PausedNormTime = null;
	int?	PausedState = null;
	#endif

	public void SetTime(float TimeSecs)
	{
		try
		{
			//	gr: if we were "paused", the state is probably garbage.
			Resume();
		}
		catch {
		}

		var Anim = GetComponent<Animator> ();
		var State = Anim.GetCurrentAnimatorStateInfo (0);
		var TimeNorm = TimeSecs / State.length;
		var StateHash = State.shortNameHash;

		Debug.Log ("Setting animator time " + TimeSecs + "/" + State.length + " -> Play(" + TimeNorm + ")");

		Anim.speed = 1;
		Anim.Play ( StateHash, 0, TimeNorm);

		#if !PAUSE_VIA_ENABLE
		{
			//	"unpause"
			PausedNormTime = null;
			PausedState = null;
		}
		#endif
	}

	public void Resume()
	{
		var Anim = GetComponent<Animator> ();

		#if PAUSE_VIA_ENABLE
		{
			Anim.enabled = true;
		}
		#else
		{
			if (!PausedNormTime.HasValue || !PausedState.HasValue)
				throw new System.Exception ("Trying to resume Animator which wasn't paused");

			//var Anim = GetComponent<Animator> ();

			var TimeNorm = PausedNormTime.Value;
			var StateHash = PausedState.Value;

			PausedNormTime = null;
			PausedState = null;

			Debug.Log ("Resuming animator at normalised time " + TimeNorm + ")");


			Anim.speed = 1;
			Anim.Play ( StateHash, 0, TimeNorm);
		}
		#endif
	}

	public void Pause()
	{
		var Anim = GetComponent<Animator> ();
	
		#if PAUSE_VIA_ENABLE
		{
			Anim.enabled = false;
		}
		#else
		{
			var State = Anim.GetCurrentAnimatorStateInfo (0);
			var TimeSecs = State.normalizedTime * State.length;

			PausedNormTime = State.normalizedTime;
			PausedState = State.shortNameHash;

			Debug.Log ("Pausing animator at " + TimeSecs);
			Anim.speed = 0;
		}
		#endif
	}
}
