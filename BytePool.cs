using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BytePool
{
	//	for ease of use, there's a global pool
	static private BytePool	_Global;
	static public BytePool	Global
	{
		get {
			if ( _Global == null )
				_Global = new BytePool();
			return _Global;
		}
	}

	public int		MaxPoolSize = 10;
	public int		TotalAllocatedBytes		{	get	{	return FreeBytes + UsedBytes;	}	}
	public int		TotalAllocatedArrays	{	get	{	return FreeArrays.Count + UsedArrays.Count;	}	}
	public int		FreeBytes				{	get	{	return CountBytes( FreeArrays );	}	}
	public int		UsedBytes				{	get	{	return CountBytes( UsedArrays );	}	}
	public bool		IsFull					{	get {	return UsedArrays.Count >= MaxPoolSize; } }
		
	static public int	CountBytes(List<byte[]> Arrays)
	{
		var Total = 0;
		foreach ( var Array in Arrays )
			Total += Array.Length;
		return Total;
	}


	List<byte[]>	FreeArrays = new List<byte[]>();
	List<byte[]>	UsedArrays = new List<byte[]>();

	void TrimArrays()
	{
		FreeArrays.Clear();
	}

	public byte[]	Alloc(byte[] CopyThisArray,bool AllowBiggerSize=false)
	{
		var NewArray = Alloc( CopyThisArray.Length, AllowBiggerSize );
		CopyThisArray.CopyTo( NewArray, 0 );
		return NewArray;
	}

	public byte[]	Alloc(int Size,bool AllowBiggerSize=false)
	{
		lock(FreeArrays)
		{
			//	find free array that fits
			for ( int i=0;	i<FreeArrays.Count;	i++ )
			{
				var Array = FreeArrays[i];

				if ( Array.Length < Size )
					continue;

				if ( !AllowBiggerSize && Array.Length > Size )
					continue;
			
				//	this fits!
				FreeArrays.RemoveAt(i);
				UsedArrays.Add(Array);
				return Array;
			}

			//	not allowed to allocate more
			if ( TotalAllocatedArrays >= MaxPoolSize )
				TrimArrays();
			if ( TotalAllocatedArrays >= MaxPoolSize )
				throw new System.Exception("Byte pool full " + TotalAllocatedArrays + "/" + MaxPoolSize );

			//	allocate a new array
			var NewArray = new byte[Size];
			UsedArrays.Add( NewArray );
			return NewArray;
		}
	}

	public void		Release(byte[] ReleasedBuffer)
	{
		lock(FreeArrays)
		{
			if (!UsedArrays.Remove (ReleasedBuffer)) {
				Debug.LogError ("Tried to remove ByteBuffer (" + ReleasedBuffer.Length + ") from pool that does not belong");
				return;
			}

			//	move out of used arrays
			//	gr: more error checking plz!
			FreeArrays.Add(ReleasedBuffer);
		}
	}	
};

