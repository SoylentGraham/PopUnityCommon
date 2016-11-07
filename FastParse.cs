using UnityEngine;
using System.Collections;

public class FastParse : MonoBehaviour {

	//	fast alterantive to parse.Floats
	//	needs more error checking
	static public float	Float(string FloatStr)
	{
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
}
