using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;



namespace PopX
{
	public class Json
	{
		public const string	NamePattern = "([\"]{1}[A-Za-z0-9]+[\"]{1})";
		public const string	SemiColonPattern = "([\\s]*):([\\s]*)";
		//	https://stackoverflow.com/a/32155765/355753
		public const string	ValueStringPattern = "([\\s]*)\"([^\\\\\"]*)\"([\\s]*)";
		public const string	ValueBoolPattern = "([\\s]*)(true|false){1}([\\s]*)";

		public static string GetNamePattern(string Name)
		{
			return "([\"]{1}" + Name + "[\"]{1})";
		}

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

		static public void	Replace(ref string Json,string Key,string Value)
		{
			var StringElementPattern = GetNamePattern (Key) + PopX.Json.SemiColonPattern + ValueStringPattern;
			var RegExpression = new Regex (StringElementPattern);

			//	todo: json escaping!
			var Replacement = '"' + Key + ':' + '"' + Value + '"';

			//	harder to debug, but simpler implementation
			Json = RegExpression.Replace (Json, Replacement);
		}

	}
}
