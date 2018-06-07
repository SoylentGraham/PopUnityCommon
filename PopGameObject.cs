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

	public static T FindObjectOfTypeInParents<T>(this GameObject go)
	{
		var parent = go.transform.parent;
		if (parent == null)
			return default (T);

		var Match = parent.GetComponent<T> ();
		if (Match != null)
			return Match;

		return parent.gameObject.FindObjectOfTypeInParents<T> ();
	}

	public static T[] FindObjectsOfTypeWithName<T>(string MatchName) where T : Component
	{
		System.Func<T,bool> Match = (x) =>
		{
			return x.name == MatchName;
		};
		return FindObjectsOfTypeMatching<T> (Match);
	}

	public static T[] FindObjectsOfTypeMatching<T>(System.Func<T,bool> Match) where T : Object
	{
		var MatchObjects = new List<T> ();
		MatchObjects.AddRange (GameObject.FindObjectsOfType<T> ());

		for (int i = MatchObjects.Count - 1;	i >= 0;	i--) {
			var mo = MatchObjects [i];
			if (Match (mo))
				continue;

			MatchObjects.RemoveAt (i);
		}

		return MatchObjects.ToArray ();
	}



	public static void ForEachChild(this GameObject go,System.Action<GameObject> Lambda)
	{
		var t = go.transform;
		for (var c = 0; c < t.childCount;	c++ )
		{
			var Child = t.GetChild(c);
			Lambda.Invoke(Child.gameObject);
		}
	}

	//	if you return false from this lambda, the search will abort early
	public static void ForEachChild(this GameObject go, System.Func<GameObject,bool> Lambda)
	{
		var t = go.transform;
		for (var c = 0; c < t.childCount; c++)
		{
			var Child = t.GetChild(c);
			if (!Lambda.Invoke(Child.gameObject))
				break;
		}
	}
		
	public static T GetChildMatching<T>(this GameObject go,System.Func<T,bool> Match) where T : Object
	{
		var t = go.transform;
		for (var c = 0; c < t.childCount; c++)
		{
			var Child = t.GetChild(c);
			var ChildComp = Child.GetComponent<T>();
			if ( !ChildComp )
				continue;
			if ( Match( ChildComp) )
				return ChildComp;
		}
		return null;
	}

	public static GameObject GetChildWithName(this GameObject go,string Name)
	{
		System.Func<GameObject,bool> Match = (Child) =>
		{
			if (Child.name != Name)
				return false;
			return true;
		};

		return go.GetChildMatching<GameObject>( Match );

	}
}

