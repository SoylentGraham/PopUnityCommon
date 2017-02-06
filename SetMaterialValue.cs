using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMaterialValue : MonoBehaviour {

	public Material	_material;

	public Material material
	{
		get
		{
			if ( !_material )
			{
				var mr = GetComponent<MeshRenderer> ();
				if ( mr )
				{
					_material = mr.sharedMaterial;
				}
			}
			return _material;
		}
	}

	public bool		GlobalUniform = false;
	public string	Uniform;

	[Header("If two-part value, this is the 2nd uniform. Dir in Ray")]
	public string	Uniform2;

	void Start()
	{
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
		
	public void SetVector4(Vector4 Value)
	{
		if (GlobalUniform) 
		{
			Shader.SetGlobalVector (Uniform, Value);
		}
		else if ( material )
		{
			material.SetVector (Uniform, Value);
		}
	}


	public void SetFloat(float Value)
	{
		if (GlobalUniform) 
		{
			Shader.SetGlobalFloat (Uniform, Value);
		}
		else if ( material )
		{
			material.SetFloat (Uniform, Value);
		}
	}


	public void SetColor(Color Value)
	{
		if (GlobalUniform) 
		{
			Shader.SetGlobalColor (Uniform, Value);
		}
		else if ( material )
		{
			material.SetColor (Uniform, Value);
		}
	}

}
