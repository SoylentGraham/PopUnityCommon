using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/*
public class Vector2x4 : FixedArray<Vector2,of4>
{
};
*/


public interface SizePolicy
{
	int	Size{	get;}
}

public class of4 : SizePolicy
{
	public int	Size{	get {	return 4;}}
}

//	gr: cannot serialise a template!
[System.Serializable]
public class FixedArray<Type,SizePolicyType> where SizePolicyType : SizePolicy, new()
{
	public Type		this [int Index] {
		get {
			return Elements [Index];
		}
		set {
			Elements [Index] = value;
		}
	}

	public int		Length	{	get { return Size.Size; } }
	public int		Count	{	get { return Size.Size; } }

	SizePolicyType	Size = new SizePolicyType();

	//[System.Serializable]
	Type[]		_Elements;
	Type[]		Elements {
		get {

			if (_Elements == null)
				_Elements = new Type[Length];
			return _Elements;
		}
	}

};
