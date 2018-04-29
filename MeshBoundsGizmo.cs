using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Display the mesh bounds as a gizmo. Because there's no built in way to do so
 */
[RequireComponent(typeof(MeshFilter))]
public class MeshBoundsGizmo : MonoBehaviour {

	public Color Colour = Color.red;

	[ShowFunctionResult("GetBoundsMin")]
	public bool _Min;
	[ShowFunctionResult("GetBoundsMax")]
	public bool _Max;

	Vector3 GetBoundsMin()
	{
		var mf = GetComponent<MeshFilter>();
		var m = mf.sharedMesh;
		var b = m.bounds;
		return b.min;
	}
	Vector3 GetBoundsMax()
	{
		var mf = GetComponent<MeshFilter>();
		var m = mf.sharedMesh;
		var b = m.bounds;
		return b.max;
	}

	//	to enable the enable checkbox
	void Update()
	{
		
	}

	void OnDrawGizmos()
	{
		if (!this.enabled)
			return;
		var mf = GetComponent<MeshFilter>();
		var m = mf.sharedMesh;
		var b = m.bounds;

		Gizmos.matrix = this.transform.localToWorldMatrix;
		Gizmos.color = Colour;
		Gizmos.DrawWireCube(b.center, b.size);
		
	}

}
