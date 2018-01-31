using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PopX
{
	public static class Exception
	{
		public static void ThrowIf(bool Condition, string Error)
		{
			if (Condition)
				throw new System.Exception(Error);
		}

		public static void Assert(bool Condition, string Error)
		{
			ThrowIf(!Condition, Error);
		}
	}
}

