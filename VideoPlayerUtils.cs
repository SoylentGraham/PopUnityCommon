using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_5_6_OR_NEWER
using UnityEngine.Video;
#endif

[System.Serializable]
public class UnityEvent_String : UnityEvent <string> {}



#if UNITY_5_6_OR_NEWER
[RequireComponent(typeof(VideoPlayer))]
#endif
public class VideoPlayerUtils : MonoBehaviour {

	public bool					DebugEvents = true;

	public bool					SkipDroppedFrames = false;
	public UnityEvent_String	OnError;
	public UnityEvent			OnStarted;
	public UnityEvent			OnLoopPoint;

	void Start () {

#if UNITY_5_6_OR_NEWER
		var Player = GetComponent<VideoPlayer> ();

		Player.skipOnDrop = SkipDroppedFrames;

		Player.errorReceived += (player, error) => {
			if ( DebugEvents )
				Debug.Log( this.name + " error: " + error );
			OnError.Invoke(error);
		};

		Player.started += (player) => {
			if ( DebugEvents )
				Debug.Log( this.name + " started");
			OnStarted.Invoke();
		};

		Player.loopPointReached += (player) => {
			if ( DebugEvents )
				Debug.Log( this.name + " loop point reached (looping=" + player.isLooping + ")");
			OnLoopPoint.Invoke();
		};
			
#else
		OnError.Invoke("Unsupported on unity < 5.6");
#endif
	}

}
