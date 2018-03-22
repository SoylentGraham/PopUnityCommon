using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UnityEvent_Texture : UnityEvent <Texture> {}

public class AnimCurveTo1DTexture : MonoBehaviour {

	[InspectorButton("RegenerateAll")]
	public bool				_RegenerateAll;

	public Texture2D		Target;
	[Range(1,4096)]
	[ShowIfAttribute("TargetIsNull")]
	public int				TargetWidth = 256;
	[ShowIfAttribute("TargetIsNull")]
	public TextureFormat	TargetFormat = TextureFormat.RGBAFloat;

	[OnChanged(null,"Generate1DTexture")]
	public AnimationCurve	Curve;

	[InspectorButton("Generate1DTexture")]
	public bool				_Generate1DTexture;

	public UnityEvent_Texture	OnGenerated;


	void RegenerateAll()
	{
		var acs = GameObject.FindObjectsOfType<AnimCurveTo1DTexture> ();
		foreach (var ac in acs) {
			try {
				ac.Generate1DTexture ();
			} catch {
			}
		}
	}
			
		

	void OnEnable () 
	{
		//	invoke event, but only generate texture if required
		OnGenerated.Invoke( GetTexture() );
	}

	bool TargetIsNull()
	{
		return Target == null;
	}

	public void Generate1DTexture()
	{
		if (!this.enabled)
			return;
		
		if (Target == null) {
			Debug.Log (this.name + " allocating new AnimCurve 1D texture", this);
			Target = new Texture2D (TargetWidth, 16, TargetFormat, false);
			Target.filterMode = FilterMode.Point;
			Target.wrapMode = TextureWrapMode.Clamp;
		}
		
		Debug.Log (this.name + " generating new AnimCurve 1D texture", this);
		var w = Target.width;

		var Colour = new Color (0, 0, 0);
		float Range = Mathf.Max (1, w - 1);
		var CurveTimeMin = Curve.keys [0].time;
		var CurveTimeMax = Curve.keys [Curve.length - 1].time;

		var Pixels = Target.GetPixels ();

		for (int x = 0;	x < w;	x++) {
			var xf = x / Range;
			var Time = Mathf.Lerp (CurveTimeMin, CurveTimeMax, xf);
			var Value = Curve.Evaluate (Time);

			//	todo; scale value to min/max of curve? maybe neccessary for non-float textures

			Colour.r = Value; 
			Colour.g = Value; 
			Colour.b = Value; 
			
			for (int y = 0;	y < Target.height;	y++) {
				Pixels [x + y * w] = Colour;
			}
		}
			
		Target.SetPixels (Pixels);
		Target.Apply ();
		OnGenerated.Invoke (Target);
	}

	//	only alloc texture if dirty/not generated
	public Texture GetTexture()
	{
		if (Target == null)
			Generate1DTexture ();
		return Target;
	}

}
