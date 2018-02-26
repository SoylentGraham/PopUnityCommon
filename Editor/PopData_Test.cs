using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NUnit.Framework;


namespace PopX
{
	public class Data_UnitTest
	{
		[Test]
		public void FindPattern_Match()
		{
			var Needle = "cat";
			var Haystack = "Are we out of cat food again?";
			var HaystackBytes = System.Text.Encoding.ASCII.GetBytes (Haystack);
			var ExceptedNeedlePos = Haystack.IndexOf (Needle);

			//	test match
			var MatchPos = Data.FindPattern ( HaystackBytes, Needle.ToCharArray ());
			Assert.AreEqual (ExceptedNeedlePos, MatchPos);
		}

		[Test]
		public void FindPattern_NotFound()
		{
			var Haystack = "Are we out of cat food again?";
			var HaystackBytes = System.Text.Encoding.ASCII.GetBytes (Haystack);

			//	test not found, should throw
			try
			{
				Data.FindPattern(HaystackBytes, "dog".ToCharArray() );
				throw new System.Exception("Failed to NOT-FIND pattern");
			}
			catch(PopX.Data.NotFound)
			{
				//	expected behaviour
			}
		}
	}
}

