using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PopX
{
	public class Json
	{
		//	works on byte[] and string, but can't do a generics with that in c#, so accessor for now. 
		//	Which is probably ultra slow
		static public int GetJsonLength(System.Func<int, char> GetChar)
		{
			//	pull json off the front
			if (GetChar(0) != '{')
				throw new System.Exception("Data is not json. Starts with " + (char)GetChar(0));

			var OpeningBrace = '{';
			var ClosingBrace = '}';
			int BraceCount = 1;
			int i = 1;
			while (BraceCount > 0)
			{
				try
				{
					var Char = GetChar(i);
					if (Char == OpeningBrace)
						BraceCount++;
					if (Char == ClosingBrace)
						BraceCount--;
					i++;
				}
				catch //	OOB
				{
					throw new System.Exception("Json braces not balanced");
				}
			}
			return i;
		}

		static public int GetJsonLength(string Data)
		{
			return GetJsonLength((i) => { return Data[i]; });
		}

		static public int GetJsonLength(byte[] Data)
		{
			return GetJsonLength((i) => { return (char)Data[i]; });
		}
	}
}
