using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaterialValue : MonoBehaviour {

	public Material	material;
	public string	Uniform;

	[Header("If two-part value, this is the 2nd uniform. Dir in Ray")]
	public string	Uniform2;

	void Start()
	{
		if (!material)
			material = GetComponent<MeshRenderer> ().sharedMaterial;
	}

	public void SetMatrix(Matrix4x4 Value)
	{
		if (material == null)
			return;
		
		material.SetMatrix (Uniform, Value);
	}
		
	public void SetRay2(Ray Value)
	{
		if (material == null)
			return;

		material.SetVector (Uniform, Value.origin);
		material.SetVector (Uniform2, Value.direction);
	}			
		

}
