using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class SetTimelineTime : MonoBehaviour {

	PlayableDirector	TimelineDirector	{	get { return GetComponent<PlayableDirector> (); } }

	public void SetTime(float Seconds)
	{
		var Timeline = TimelineDirector;
		Timeline.time = Seconds;
	}
}
