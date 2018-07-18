using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PopPlayerPrefs
{
	const string NullValue = "<null>";  //	if we serialise null, we write this. If we deseralise it instead of json, return null

	public static void SetObject<T>(string Key, T Value)
	{
		var Json = (Value == null) ? NullValue : JsonUtility.ToJson(Value);
		PlayerPrefs.SetString(Key, Json);
	}

	public static T GetObject<T>(string Key)
	{
		var Json = PlayerPrefs.GetString(Key);
		if (Json.Equals(NullValue))
			return default(T);

		var Obj = JsonUtility.FromJson<T>(Json);
		return Obj;
	}


}
