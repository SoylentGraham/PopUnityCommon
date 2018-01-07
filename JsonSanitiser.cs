using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


namespace PopX
{
	public class JsonSanitiser
	{
/*
	 This function looks through json for anonymous double arrays that unity can't handle, and lets you inject an object with a name in 
	 front of it so you can use JsonUtility and unity's serialising in the same way

	problem:
	"points": [ [0,0,0], [1,1,1], [2,2,2] ],
	goal:
	"points": [ {	"myvector":[0,0,0]	},	{	"myvector":[1,1,1]	},	{	"myvector":[2,2,2]	}	],

	//	can then deserialise as normal
	class TVector
	{
	public float[]	myvector;
	};

	class TPoints
	{
	public TVector[]	points;
	}

	//	to do the above
	JsonSanitiser.DoubleArrayToMember( Json, (ParentName) => {	return ParentName == "points" ? "myvector" : null;	} );
*/
		public static string	DoubleArrayToMember(string Json,System.Func<string,string> ReplaceHeriachyDoubleArrayWithMemberName)
		{
			//	find all double arrays

			//	match (with whitepspace)
			//	name semicolon [ [ 
			var ParenthesisPattern = "\\[([\\s]*)\\[";
			var DoubleArrayPattern = PopX.Json.NamePattern + PopX.Json.SemiColonPattern + ParenthesisPattern;
			var RegExpression = new Regex(DoubleArrayPattern);

			int Iterations = 0;	//	stop infinite loops
			int StringIndex = 0;
			do {
				var match = RegExpression.Match (Json, StringIndex);
				if ( match == null )
					break;
				if ( match.Groups.Count < 2 )
					break;

				var WholeMatch = match.Groups[0].Captures[0];
				var Name = match.Groups[1].Captures[0];
				try
				{
					//	if we don't parse anything, step over. if we do, re-parse from here so we can do recursive sub anonymous arrays
					if ( !ParseCapture( Name, ref Json, ReplaceHeriachyDoubleArrayWithMemberName ) )
						StringIndex = match.Index + match.Length;
				}
				catch (System.Exception e)
				{
					Debug.LogException(e);
					break;
				}
			} 
			while(Iterations++ < 5000);

			return Json;
		}




		static string ExtractNextChunk(char StartChar,char EndChar,string Haystack,ref int NeedleStart,ref int NeedleEnd)
		{
			//	now grab the entire next section by balancing square brackets.
			var BracketCount = 1;
			//	todo: count when we're inside sections where we need to ignore opening and closing brackets... eg. /* hello :] */ and "Bye :[ "
			//var BraceCount = 0;
			//var QuoteCount = 0;
			//var CommentCount = 0;
			var SearchStart = Haystack.IndexOf(StartChar,NeedleStart);
			if (SearchStart == -1)
				return null;
			NeedleStart = SearchStart;
			var StringIndex = NeedleStart+1;
			while (BracketCount > 0) 
			{
				if (StringIndex >= Haystack.Length)
					throw new System.Exception ("Unbalanced array" + StartChar+EndChar + " in json");

				var ThisChar = Haystack [StringIndex];
				if ( ThisChar == StartChar )
					BracketCount++;
				if ( ThisChar == EndChar )
					BracketCount--;

				StringIndex++;
			}

			//	we now have the whole double array. trim off the outer [] 
			NeedleEnd = StringIndex;
			var DoubleArrayString = Haystack.Substring (NeedleStart, NeedleEnd - NeedleStart );
			return DoubleArrayString;
		}

		struct TChunk
		{
			public string	Chunk;
			public int		Start;
			public int		End;
		};


		//	return if changed
		static bool ParseCapture(Capture NameMatch,ref string Json,System.Func<string,string> ReplaceHeriachyDoubleArrayWithMemberName)
		{
			//	todo: have a full heirachy name, not just this element
			var HeirachyName = NameMatch.ToString().Trim( new char[]{'"'} );

			var InjectMemberName = ReplaceHeriachyDoubleArrayWithMemberName.Invoke (HeirachyName);
			if (InjectMemberName == null)
				return false;

			//	grab the whole double array string after semi colon
			int NameEnd = NameMatch.Index + NameMatch.Length;
			int ArrayStart = Json.IndexOf('[', NameEnd );
			if (ArrayStart == -1)
				throw new System.Exception ("Can't find start of array");

			int DoubleArrayStart = ArrayStart;
			int DoubleArrayEnd = ArrayStart;
			var DoubleArray = ExtractNextChunk ('[', ']', Json, ref DoubleArrayStart, ref DoubleArrayEnd );

			//	get each array
			var ArrayElements = new List<TChunk>();
			int ChunkStart = 1;
			while (true) {
				var NewChunk = new TChunk ();
				NewChunk.Start = ChunkStart;
				NewChunk.End = ChunkStart;
				NewChunk.Chunk = ExtractNextChunk ('[', ']', DoubleArray, ref NewChunk.Start, ref NewChunk.End);
				if (NewChunk.Chunk == null)
					break;

				ChunkStart = NewChunk.End;

				//	correct for final usage
				NewChunk.Start += ArrayStart;
				NewChunk.End += ArrayStart;
				ArrayElements.Add (NewChunk);
			}

			//Debug.Log ("Found " + ArrayElements.Count + " elements;");
			//foreach (var Element in ArrayElements)
			//	Debug.Log (Element.Chunk);

			//	inject object & member name for every element
			//	do back to front so we don't need to correct indexes for elements
			ArrayElements.Reverse ();
			//	{	"myvector":
			var InjectPrefix = " { \"" + InjectMemberName + "\" : ";
			var InjectSuffix = " } ";
			foreach (var Element in ArrayElements) {
				Json = Json.Insert (Element.End, InjectSuffix);
				Json = Json.Insert (Element.Start, InjectPrefix);
			}

			return true;
		}


		/*
		public static void	Test_JsonCleanDoubleArrays()
		{
			var Json = "{ \"points\": [ [0,0,0], [1,1,1], [2,2,2] ] }";

			Json = JsonCleanDoubleArrays (Json, (Heirachy) => {
				Debug.Log(Heirachy);
				return null;
			}
			);
		}

		public static void	Test_JsonCleanDoubleArrays2()
		{
			var Json = "{ \"points\": [ [0,0,0], [1,1,1], [2,2,2] ],  \"morepoints\": [ [3,4,5], [6,7,8], [9,10,11] ] }";

			Json = JsonCleanDoubleArrays (Json, (Heirachy) => {
				Debug.Log(Heirachy);
				return "float3";
			}
			);

			Debug.Log ("Injected json: " + Json);
		}
*/
	}
}
