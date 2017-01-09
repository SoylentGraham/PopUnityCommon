using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*

	Like a dictionary, but keys aren't hashmapped.
	For something I was doing, this system was much much faster than the accessors of dictionary
	(maybe due to the string hashing), but most benefit was the fact i could cache
	the index for a key and access directly

*/
public class StackedDictionary<KEY,VALUE> where KEY : class
{
	List<VALUE>		_Values = new List<VALUE>();
	List<KEY>		_Keys = new List<KEY>();

	public int			Count	{	get { return _Keys.Count; } }
	public List<KEY>	Keys	{	get { return _Keys; } }


	public int	GetKeyIndex(KEY Key)
	{
		for (int i = 0;	i < Keys.Count;	i++)
			if (Keys [i] == Key)
				return i;
		return -1;
	}

	public VALUE this[int KeyIndex]
	{
		get	{	return _Values[KeyIndex];	}
		set	{	_Values[KeyIndex] = value;	}
	}

	public VALUE this[KEY Key]
	{
		get	{	return _Values[GetKeyIndex(Key)];	}
		set	{	_Values[GetKeyIndex(Key)] = value;	}
	}

	public bool ContainsKey(KEY Key)
	{
		return GetKeyIndex (Key) != -1;
	}

	public void Add(KEY Key,VALUE Value)
	{
		_Values.Add (Value);
		_Keys.Add (Key);
	}
};


