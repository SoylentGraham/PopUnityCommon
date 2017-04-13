using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

	public void NextScene()
	{
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
				if ( TimeSinceStart> TriggerAfterSecs) {
					NextScene ();
			}
		}
	}
}
