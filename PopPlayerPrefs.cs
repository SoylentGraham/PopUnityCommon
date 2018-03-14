using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PopPlayerPrefs
{
	const string NullValue = "<null>";  //	if we serialise null, we write this. If we deseralise it instead of json, return null

	public static void SetObject<T>(string Key, T Value) where T : Object
	{
		var Json = (Value == null) ? NullValue : JsonUtility.ToJson(Value);
		PlayerPrefs.SetString(Key, Json);
	}

	public static T GetObject<T>(string Key) where T : Object
	{
		var Json = PlayerPrefs.GetString(Key);
		if (Json.Equals(NullValue))
			return (T)null;

		var Obj = JsonUtility.FromJson<T>(Json);
		return Obj;
	}


}
