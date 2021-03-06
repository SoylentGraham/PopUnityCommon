using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_5_6_OR_NEWER
using UnityEngine.Video;
#endif



#if UNITY_5_6_OR_NEWER
[RequireComponent(typeof(VideoPlayer))]
#endif
public class VideoPlayerUtils : MonoBehaviour {

	public float?				InitialTime	
	{
		get
		{
			#if UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_OSX
			var StartTime = EditorInitialTime;
			#else
			var StartTime = PlayerInitialTime;
			#endif
			if (StartTime > 0)
				return (float)StartTime;
			return null;
		}
	}

	public double				PlayerInitialTime = 0;
	[Header("For debugging. Set initial time, but only in editor")]
	public double				EditorInitialTime = 0;

	public bool					DebugEvents = true;
	public bool					DebugEveryFrame = false;

	public bool					SkipDroppedFrames = false;
	public UnityEvent_String	OnError;
	[Header("gr: sometimes OnStarted happens before OnPrepared")]
	public UnityEvent			OnPrepared;
	public UnityEvent			OnStarted;
	public UnityEvent_float		OnFirstFrameReady;
	public UnityEvent			OnLoopPoint;

	[Header("Callback just before video finishes")]
	[Range(-5,0)]
	public float				NotifySecsBeforeFinish = 0;
	public UnityEvent			OnBeforeFinish;
	long?						LastFrameIndex = null;

	public float				GetClipDuration()
	{
		#if UNITY_5_6_OR_NEWER
		var Player = GetComponent<VideoPlayer> ();
		var FrameCount = Player.clip.frameCount;
		return FrameIndexToTime( FrameCount );
		#else
		throw new System.Exception("Unsupported on unity < 5.6");
		#endif
	}

	public float				FrameIndexToTime(ulong FrameIndex)
	{
		#if UNITY_5_6_OR_NEWER
		var Player = GetComponent<VideoPlayer> ();
		var FrameRate = (float)Player.clip.frameRate;
		return FrameIndex / FrameRate;
		#else
		throw new System.Exception("Unsupported on unity < 5.6");
		#endif
	}

	public ulong					TimeToFrameIndex(float Time)
	{
		#if UNITY_5_6_OR_NEWER
		var Player = GetComponent<VideoPlayer> ();
		var FrameRate = (float)Player.clip.frameRate;
		var FrameCount = Player.clip.frameCount;

		if ( Time < 0 )
			Time = 0;
		var FrameIndex = (ulong)(Time * FrameRate);

		if ( FrameIndex >= FrameCount )
			FrameIndex = FrameCount-1;
		return FrameIndex;
		#else
		throw new System.Exception("Unsupported on unity < 5.6");
		#endif
	}

	void Start () {

#if UNITY_5_6_OR_NEWER
		var Player = GetComponent<VideoPlayer> ();

		//	gr; this is stopping the video from playing... on android... if video has started?
		//Player.skipOnDrop = SkipDroppedFrames;

		var StartTime = InitialTime;
		if ( StartTime.HasValue )
		{
			if ( !Player.canSetTime )
				Debug.LogError("Couldn't not set video player start time to " + StartTime.Value );
			Player.time = StartTime.Value;
		}
		
		Player.errorReceived += (player, error) => {
			if ( DebugEvents )
				Debug.Log( this.name + " error: " + error );
			OnError.Invoke(error);
		};

		Player.started += (player) => {
			if ( DebugEvents )
				Debug.Log( this.name + " started");
			OnStarted.Invoke();
			LastFrameIndex = null;
		};

		Player.loopPointReached += (player) => {
			if ( DebugEvents )
				Debug.Log( this.name + " loop point reached (looping=" + player.isLooping + ")");
			OnLoopPoint.Invoke();
			LastFrameIndex = null;
		};

		Player.prepareCompleted += (player) =>
		{
			if ( DebugEvents )
				Debug.Log( this.name + " Prepared");
			OnPrepared.Invoke();
			LastFrameIndex = null;
		};

		Player.seekCompleted += (player) =>
		{
			if ( DebugEvents )
				Debug.Log( this.name + " seekCompleted");
		};

		Player.frameReady += (player,FrameIndex) =>
		{
			if ( LastFrameIndex == null )
			{
				var timef = (float)player.time;
				if ( DebugEvents )
					Debug.Log( this.name + " first frame ready at " + timef);
				OnFirstFrameReady.Invoke( timef );
			}
		};

		try
		{
			var ClipDuration = GetClipDuration();
			var NotifyTime = ClipDuration + NotifySecsBeforeFinish;
			var NotifyFrameNumber = (long)TimeToFrameIndex( NotifyTime );

			Player.sendFrameReadyEvents = true;
			Player.frameReady += (player,FrameIndex) =>
			{
				if ( DebugEveryFrame )
					Debug.Log( this.name + " frameReady (FrameIndex=" + FrameIndex + ")");

				try
				{
					CheckForBeforeFinishNotification( FrameIndex, NotifyFrameNumber );
				}
				catch(System.Exception e)
				{
					Debug.LogException(e);
				}
			};
			Player.frameDropped += (player) =>
			{
				if ( DebugEveryFrame )
					Debug.Log( this.name + " frameDropped (FrameIndex=" + player.frame + ")");
			
				try
				{
					CheckForBeforeFinishNotification( player.frame, NotifyFrameNumber );
				}
				catch(System.Exception e)
				{
					Debug.LogException(e);
				}

			};
		}
		catch
		{
		}

#else
		OnError.Invoke("Unsupported on unity < 5.6");
#endif

	}


	void CheckForBeforeFinishNotification(long Frame,long NotifyFrame)
	{
		if (Frame >= NotifyFrame) {
			//	only notify once!
			//	seeked straight over the notify point
			if (LastFrameIndex == null) {
				if ( DebugEvents )
					Debug.Log( this.name + " Before-Finish notification (jumped in)");
				OnBeforeFinish.Invoke ();
			} else {
				//	stepped over naturally
				if (LastFrameIndex < NotifyFrame) {
					if (DebugEvents)
						Debug.Log (this.name + " Before-Finish notification (frame elapse)");
					OnBeforeFinish.Invoke ();
				}
			}
		}

		//	update last frame
		LastFrameIndex = Frame;
	}
}
