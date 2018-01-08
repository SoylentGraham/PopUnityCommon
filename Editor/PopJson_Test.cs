using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NUnit.Framework;


namespace PopX
{
	public class Json_UnitTest
	{
		[System.Serializable]
		public class TestA
		{
			public string	One;
			public TestA(string _One)	{	One = _One;	}
		};

		[Test]
		public void Replace_Test()
		{
			//	use a struct to avoid whitespace hassle. We just need to know if it de/serialises
			var Struct = new TestA ("one");
			var NewValue = "Hello";
			var Json = JsonUtility.ToJson (Struct);
			PopX.Json.Replace (ref Json, "One", NewValue);
			JsonUtility.FromJsonOverwrite (Json, Struct);

			Assert.AreEqual (Struct.One, NewValue);
		}
	}
}

