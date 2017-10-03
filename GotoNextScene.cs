using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[System.Serializable]
public class UnityEvent_float : UnityEvent <float> {}



public class GotoNextScene : MonoBehaviour {

	public enum SceneLoadMode
	{
		LoadNextScene,
		ReloadCurrentScene,
		LoadSpecificScene
	};

	public SceneLoadMode	LoadMode = SceneLoadMode.LoadNextScene;

	[InspectorButton("GotoNextScene")]
	public bool		_GotoNextScene;

	[ShowIf("ModeIs_LoadSpecificScene")]
	public string levelName;

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

	public bool SkipOnMouseClick = false;


	bool ModeIs_LoadNextScene()
	{
		return LoadMode == SceneLoadMode.LoadNextScene;
	}

	bool ModeIs_ReloadCurrentScene()
	{
		return LoadMode == SceneLoadMode.ReloadCurrentScene;
	}

	bool ModeIs_LoadSpecificScene()
	{
		return LoadMode == SceneLoadMode.LoadSpecificScene;
	}


	public void NextScene()
	{
		var CurrentScene = SceneManager.GetActiveScene();

		switch( LoadMode )
		{
		case SceneLoadMode.LoadNextScene:
			var CurrentSceneIndex = CurrentScene.buildIndex;
			var NextSceneIndex = (CurrentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;

			SceneManager.LoadScene(NextSceneIndex, LoadSceneMode.Single);
			break;

		case SceneLoadMode.ReloadCurrentScene:
			SceneManager.LoadScene(CurrentScene.name);
			break;

		case SceneLoadMode.LoadSpecificScene:
			SceneManager.LoadScene (levelName);
			break;
		}
	}

	void OnEnable()
	{
		TriggerFromTime = Time.time;
	}

	void Update () {

		if (SkipOnMouseClick) {
			if (Input.GetMouseButtonDown (0)) {
				NextScene ();
			}
		}

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