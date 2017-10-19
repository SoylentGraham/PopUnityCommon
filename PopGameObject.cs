using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public static class PopGameObject
{
	public static T[] FindObjectsOfTypeIncludingDisabled<T>(this GameObject go)
	{
		return go.GetComponentsInChildren<T> (true);
	}

	public static T[] FindObjectsOfTypeIncludingDisabled<T>()
	{
		var ActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
		var RootObjects = ActiveScene.GetRootGameObjects ();
		var MatchObjects = new List<T> ();

		foreach (var ro in RootObjects) {
			var Matches = ro.FindObjectsOfTypeIncludingDisabled<T> ();
			MatchObjects.AddRange (Matches);
		}

		return MatchObjects.ToArray ();
	}

	public static T FindObjectOfTypeIncludingDisabled<T>(this GameObject go)
	{
		return go.GetComponentInChildren<T> (true);
	}

	public static T FindObjectOfTypeIncludingDisabled<T>()
	{
		var ActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
		var RootObjects = ActiveScene.GetRootGameObjects ();

		foreach (var ro in RootObjects) {
			var Match = ro.FindObjectOfTypeIncludingDisabled<T> ();
			if (Match != null)
				return Match;
		}

		return default(T);
	}

}

