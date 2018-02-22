using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FastParse : MonoBehaviour {

	//	fast alterantive to parse.Floats
	//	needs more error checking
	static public float	Float(string FloatStr)
	{
		if (FloatStr == null || FloatStr.Length == 0)
			throw new System.Exception ("Empty string");
		
		float Major = 0;
		float Minor = 0;
		int Pos = 0;
		float Modifier = 1.0f;

		if (FloatStr [0] == '-') {
			Modifier = -1.0f;
			Pos++;
		}

		//	parse major
		while (Pos < FloatStr.Length) {
			if (FloatStr [Pos] == '.')
			{
				Pos++;
				break;
			}

			//	throw if non-number
			var CharNumber = FloatStr [Pos] - '0';
			if (CharNumber < 0 || CharNumber > 9)
				throw new System.Exception ("Nan string");

			Major *= 10;
			Major += CharNumber;
			Pos++;
		}
		
		//	parse minor
		float MinorScale = 1.0f / 10.0f;
		while (Pos < FloatStr.Length) {
			if ( FloatStr[Pos] == 'f' )
			{
				Pos++;
				continue;
			}

			//	hacky handling of exponential
			if (FloatStr [Pos] == 'e' )
				if ( FloatStr [Pos+1] == '-')
				break;

			//	throw if non-number
			var CharNumber = FloatStr [Pos] - '0';
			if (CharNumber < 0 || CharNumber > 9)
				throw new System.Exception ("Nan string");
			
			Minor += CharNumber * MinorScale;
			MinorScale /= 10.0f;
			Pos++;
		}
		
		return Modifier * (Major + Minor);
	}

	static public List<float>	Floats(string FloatString)
	{
		var FloatStrings = FloatString.Split (new char[]{ ' ',',' }, System.StringSplitOptions.RemoveEmptyEntries );
		if (FloatStrings == null || FloatStrings.Length == 0)
			FloatStrings = new string[1]{ FloatString };

		var Floats = new List<float> ();

		//	let errors throw
		foreach (var s in FloatStrings) {
			var f = Float (s);
			Floats.Add (f);
		}
		return Floats;
	}
		
}
