using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaterialValue : MonoBehaviour {

	public Material	material;
	public bool		GlobalUniform = false;
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
		if (GlobalUniform) 
		{
			Shader.SetGlobalMatrix (Uniform, Value);
		}
		else if ( material )
		{
			material.SetMatrix (Uniform, Value);
		}
	}


	public void SetTexture(Texture Value)
	{
		if (GlobalUniform)
		{
			Shader.SetGlobalTexture (Uniform, Value);
		}
		else if ( material )
		{
			material.SetTexture (Uniform, Value);
		}
	}

	public void SetRay2(Ray Value)
	{
		if (GlobalUniform) 
		{
			Shader.SetGlobalVector (Uniform, Value.origin);
			Shader.SetGlobalVector (Uniform2, Value.direction);
		} 
		else if ( material )
		{
			material.SetVector (Uniform, Value.origin);
			material.SetVector (Uniform2, Value.direction);
		}
	}			
		

}
