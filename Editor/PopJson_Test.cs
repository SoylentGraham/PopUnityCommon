using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NUnit.Framework;


namespace PopX
{
	public class Json_UnitTest
	{
		[System.Serializable]
		public class StructHello
		{
			public string	Hello;

			public StructHello(string _HelloValue)	
			{
				Hello = _HelloValue;	
			}
		};

		[System.Serializable]
		public class StructHelloCat : StructHello
		{
			public string	Cat;

			public StructHelloCat(string _HelloValue,string _CatValue) : base(_HelloValue)
			{
				Cat = _CatValue;	
			}
		};

		[Test]
		public void Replace_Test()
		{
			//	use a struct to avoid whitespace hassle. We just need to know if it de/serialises
			var Struct = new StructHello ("World");
			var NewValue = "Universe";
			var Json = JsonUtility.ToJson (Struct);
			PopX.Json.Replace (ref Json, "Hello", NewValue);
			JsonUtility.FromJsonOverwrite (Json, Struct);

			Assert.AreEqual ( NewValue,Struct.Hello);
		}

		[Test]
		public void Append_ToEmptyJson()
		{
			var Json = "{}";
			var Value = "World";
			PopX.Json.Append (ref Json, "Hello",Value);

			//	read new json (will throw json syntax errors for us)
			var Struct = JsonUtility.FromJson<StructHello>(Json);

			Assert.AreEqual (Value,Struct.Hello);
		}

		[Test]
		public void Append_ToShallowJson()
		{
			var Json = JsonUtility.ToJson (new StructHello ("World"));
			var CatValue = "Dog";
			PopX.Json.Append (ref Json, "Cat", CatValue );

			//	read new json (will throw json syntax errors for us)
			var Struct = JsonUtility.FromJson<StructHelloCat>(Json);

			Assert.AreEqual (CatValue,Struct.Cat);
		}
	}
}

