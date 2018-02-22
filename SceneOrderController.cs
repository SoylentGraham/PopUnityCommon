using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneOrderController : MonoBehaviour {

	public List<string> ScenesNames;

	[InspectorButton("NextScene")]
	public bool	_NextScene;

	void Awake() {
		DontDestroyOnLoad(this);
	}

	public void NextScene()
	{
		//	pop first one and goto it
		string NextScene = null;
		while (ScenesNames.Count > 0) {
			NextScene = ScenesNames [0];
			ScenesNames.RemoveAt (0);
			if (NextScene != null)
				break;
			Debug.Log ("Entry in scene list is null");
		}

		if (NextScene == null) {
			Debug.LogError ("No scene to move onto.");
			return;
		}

		Debug.Log ("Loading " + NextScene);
		UnityEngine.SceneManagement.SceneManager.LoadScene (NextScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
	}
}
