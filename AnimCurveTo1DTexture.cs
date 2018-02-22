using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UnityEvent_Texture : UnityEvent <Texture> {}


public class AnimCurveTo1DTexture : MonoBehaviour {

	public Texture2D		Target;
	[Range(1,4096)]
	[ShowIfAttribute("TargetIsNull")]
	public int				TargetWidth = 256;
	[ShowIfAttribute("TargetIsNull")]
	public TextureFormat	TargetFormat = TextureFormat.RGBAFloat;

	public AnimationCurve	Curve;

	[InspectorButton("Generate1DTexture")]
	public bool				_Generate1DTexture;

	public UnityEvent_Texture	OnGenerated;

	void Start () {
		Generate1DTexture ();
	}

	bool TargetIsNull()
	{
		return Target == null;
	}

	public void Generate1DTexture()
	{
		if (Target == null) {
			Target = new Texture2D (TargetWidth, 16, TargetFormat, false);
			Target.filterMode = FilterMode.Point;
		}
		
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
}
