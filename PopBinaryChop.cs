using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PopX
{
	public static class BinaryChop
	{
		public enum CompareDirection
		{
			Before,
			Inside,
			After
		};

		public static void Search<T>(int FirstIndex, int LastIndex, System.Func<int, T> GetElementAt, System.Func<T, CompareDirection> Compare, out int? NearestPrevIndex, out int? MatchIndex)
		{
			//	dealing with 1million sized arrays, so this number needs to be big
			int Tries = 1000;
			while (Tries-- > 0)
			{
				if (FirstIndex > LastIndex)
					throw new System.Exception("Binary chop error, FirstIndex(" + FirstIndex + ") > LastIndex(" + LastIndex + ")");

				var FirstElement = GetElementAt(FirstIndex);
				var FirstDirection = Compare(FirstElement);
				if (FirstDirection == CompareDirection.Inside)
				{
					MatchIndex = FirstIndex;
					NearestPrevIndex = FirstIndex;
					return;
				}
				//	im before first
				if (FirstDirection == CompareDirection.Before)
				{
					MatchIndex = null;
					//	gr: is this -1 right?
					NearestPrevIndex = FirstIndex - 1;
					return;
				}

				//	in case we've exhausted the list
				//	im after first
				if (FirstIndex == LastIndex)
				{
					MatchIndex = null;
					NearestPrevIndex = FirstIndex;
					return;
				}

				var LastElement = GetElementAt(LastIndex);
				var LastDirection = Compare(LastElement);
				if (LastDirection == CompareDirection.Inside)
				{
					MatchIndex = LastIndex;
					NearestPrevIndex = LastIndex;
					return;
				}
				//	im after last
				if (LastDirection == CompareDirection.After)
				{
					MatchIndex = null;
					NearestPrevIndex = LastIndex;
					return;
				}

				//	when no range left, we can get stuck with the midpoint, but we can bail out here
				if (FirstIndex+1 == LastIndex)
				{
					MatchIndex = null;
					NearestPrevIndex = FirstIndex;
					return;
				}

				if ( Tries < 10 )
				{
					Debug.Log("Binary chop last chance...");
				}

				//	gr: can we get stuck here by not moving in either direction?
				//	gr: yes (dealt with above)
				var RangeRemaining = LastIndex - FirstIndex;
				var MidIndex = FirstIndex + (RangeRemaining / 2);
				var MidElement = GetElementAt(MidIndex);
				var MidDirection = Compare(MidElement);
				if (MidDirection == CompareDirection.Inside)
				{
					MatchIndex = MidIndex;
					NearestPrevIndex = MidIndex;
					return;
				}

				//	in first half or second half
				if (MidDirection == CompareDirection.Before)
				{
					LastIndex = MidIndex;
					//todo: FirstIndex++;
				}
				else
				{
					FirstIndex = MidIndex;
					//todo: LastIndex--;
				}
				
				if (FirstIndex > LastIndex)
					throw new System.Exception("Binary chop mid-jump error, FirstIndex(" + FirstIndex + ") > LastIndex(" + LastIndex + ")");
			}

			throw new System.Exception("Binary chop bailed from loop");
		}

	}
}
