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
		
		//	parse major
		while (Pos < FloatStr.Length) {
			if (FloatStr [Pos] == '.')
			{
				Pos++;
				break;
			}
			Major *= 10;
			Major += FloatStr [Pos] - '0';
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
			Minor += (FloatStr [Pos] - '0') * MinorScale;
			MinorScale /= 10.0f;
			Pos++;
		}
		
		return Major + Minor;
	}
}
