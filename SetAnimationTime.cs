//	gr: actually the simpler solution.
#define PAUSE_VIA_ENABLE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SetAnimationTime : MonoBehaviour {

	public bool ShowDebug = false;
	#if !PAUSE_VIA_ENABLE
	float?	PausedNormTime = null;
	int?	PausedState = null;
	#endif

	Animator Anim 
	{
		get
		{
			var Comp = GetComponent<Animator>();
			if (Comp != null)
				return Comp;

			return GetComponentInChildren<Animator>();
		}
	}


	public void SetNormalisedTime(float TimeNorm)
	{
		try
		{
			//	gr: if we were "paused", the state is probably garbage.
			Resume();
		}
		catch(System.Exception)
		{
		}

		var State = Anim.GetCurrentAnimatorStateInfo(0);
		var StateHash = State.shortNameHash;

		var TimeSecs = TimeNorm * State.length;

		if ( ShowDebug )
			Debug.Log("Setting animator time " + TimeSecs + "/" + State.length + " -> Play(" + TimeNorm + ")");

		Anim.speed = 1;
		Anim.Play(StateHash, 0, TimeNorm);

#if !PAUSE_VIA_ENABLE
		{
			//	"unpause"
			PausedNormTime = null;
			PausedState = null;
		}
#endif
	}


	public void SetTime(float TimeSecs)
	{
		try
		{
			//	gr: if we were "paused", the state is probably garbage.
			Resume();
		}
		catch (System.Exception){
		}

		var Anim = this.Anim;
		var State = Anim.GetCurrentAnimatorStateInfo (0);
		var TimeNorm = TimeSecs / State.length;

		SetNormalisedTime(TimeNorm);
	}

	public void Resume()
	{
		var Anim = this.Anim;

		#if PAUSE_VIA_ENABLE
		{
			Anim.enabled = true;
		}
		#else
		{
			if (!PausedNormTime.HasValue || !PausedState.HasValue)
				throw new System.Exception ("Trying to resume Animator which wasn't paused");

			//var Anim = this.Anim;

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
		var Anim = this.Anim;
	
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
