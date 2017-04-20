using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

[System.Serializable]
public class UnityEvent_float : UnityEvent <float> {}




public class GotoNextScene : MonoBehaviour {

	[InspectorButton("GotoNextScene")]
	public bool		_GotoNextScene;

	[Header("If not zero, go to next scene after X secs from OnEnable")]
	[Range(0,180)]
	public float	TriggerAfterSecs = 0;
	public bool		DoTimedTrigger
	{
		get
		{
			return TriggerAfterSecs > 0;
		}
	}
	private float	TriggerFromTime = 0;

	[Header("sends 0-1 of countdown")]
	public UnityEvent_float	OnCountdown;

	public void NextScene()
	{
		//	see if there's a scene controller
		var sc = GameObject.FindObjectOfType<SceneOrderController>();
		if (sc != null) {
			sc.NextScene ();
			return;
		}
		
		var CurrentScene = SceneManager.GetActiveScene();
		var CurrentSceneIndex = CurrentScene.buildIndex;
		var NextSceneIndex = (CurrentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;

		SceneManager.LoadScene(NextSceneIndex, LoadSceneMode.Single);
	}

	void OnEnable()
	{
		TriggerFromTime = Time.time;
	}

	void Update () {

		if (DoTimedTrigger) {
			var TimeSinceStart = Time.time - TriggerFromTime;
			var CountdownFactor = Mathf.Clamp01 (TimeSinceStart / TriggerAfterSecs);
			//Debug.Log (CountdownFactor);
			OnCountdown.Invoke (CountdownFactor);
				if ( TimeSinceStart> TriggerAfterSecs) {
					NextScene ();
			}
		}
	}
}
