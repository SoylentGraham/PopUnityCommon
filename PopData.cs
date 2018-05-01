using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PopX
{
	public static class Data
	{
		static public byte[] SubArray(this System.Array ParentArray, long Start, long Count)
		{
			var ChildArray = new byte[Count];
			System.Array.Copy(ParentArray, Start, ChildArray, 0, Count);
			return ChildArray;
		}

		public class NotFound : System.Exception
		{
			public NotFound()
			{
			}
		}

		//	minimal interface for reading through arrays of data (avoiding conversions)
		public interface IIndexer<T>
		{
			T this [int index] {
				get;
			}
			int Count {
				get;
			}
		};

		//	char->byte conversion without copying all the chars and converting to a byte array
		public class CharsAsBytes : IIndexer<byte>
		{
			IList<char>		Data;

			public byte this [int index] {
				get { return (byte)Data [index]; }
			}

			public int Count {
				get { return Data.Count; }
			}

			public CharsAsBytes(IList<char> Data)
			{
				this.Data = Data;
			}
		};

		//	list to indexer
		public class ListIndexer<T> : IIndexer<T>
		{
			IList<T>	Data;

			public T this [int index] {
				get { return Data [index]; }
			}

			public int Count {
				get { return Data.Count; }
			}

			public ListIndexer(IList<T> Data)
			{
				this.Data = Data;
			}
		}

		static public int	FindPattern(IList<byte> Data,IList<byte> Match,int Start=0)
		{
			return FindPattern ( Data, new ListIndexer<byte>(Match), Start);
		}

		static public int	FindPattern(IList<byte> Data,IList<char> Match,int Start=0)
		{
			return FindPattern ( Data, new CharsAsBytes (Match), Start);
		}

		//	throws Data.NotFound if not present
		static public int	FindPattern(IList<byte> Data,IIndexer<byte> Match,int Start=0)
		{
			var PrefixLen = Match.Count;
			int Position = Start;
			while (Position + PrefixLen < Data.Count) {

				var IsMatch = true;
				for (int i = 0;	i < Match.Count;	i++) {
					var d = Data [Position + i];
					var m = Match [i];
					IsMatch = IsMatch && (d == m);
				}

				if (IsMatch)
					return Position;

				Position++;
			}

			throw new Data.NotFound();
		}


	};
}
