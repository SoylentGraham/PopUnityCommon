using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RandomMaterialInitialisation : MonoBehaviour {

	[InspectorButton("Randomise")]
	public bool _Randomise;

	[Header("If this is false, a new instance of the material will be created")]
	public bool	RandomiseSharedMaterial = true;

	[Header("Randomly set these values based on their range")]
	public List<string>	Uniforms;

	void Start () {
	}

	#if UNITY_EDITOR
	public void Randomise()
	{
		var mr = GetComponent<MeshRenderer> ();
		var Mat = RandomiseSharedMaterial ? mr.sharedMaterial : mr.material;

		foreach (var Uniform in Uniforms) {
			RandomiseUniform (Mat, Uniform);
		}
	}
	#endif

	#if UNITY_EDITOR
	public void RandomiseUniform(Material Mat,string Uniform)
	{
		var Shader = Mat.shader;
		var PropertyIndex = Shader.PropertyToID (Uniform);
		if (PropertyIndex < 0) 
		{
			Debug.LogError ("No uniform named " + Uniform + " in " + Shader.name);
			return;
		}
		Debug.Log ("uniform " + Uniform + " in " + Shader.name + " = " + PropertyIndex);

		//float Default = UnityEditor.ShaderUtil.GetRangeLimits (Shader, PropertyIndex, 0);
		float Min = UnityEditor.ShaderUtil.GetRangeLimits (Shader, PropertyIndex, 1);
		float Max = UnityEditor.ShaderUtil.GetRangeLimits (Shader, PropertyIndex, 2);

		RandomiseUniform (Mat, Uniform, Min, Max);
	}
	#endif

	//	gr: serialise these attributes to allow randomisation at startup
	#if UNITY_EDITOR
	public void RandomiseUniform(Material Mat,string Uniform,float Min,float Max)
	{
		var Value = Random.Range (Min, Max);
		Mat.SetFloat (Uniform, Value);
	}
	#endif



}
