using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//	gr: I had a script in PopUnityCommon somewhere for this...
[System.Serializable]
public class UnityEvent_int : UnityEngine.Events.UnityEvent <int> {}



[RequireComponent(typeof(Camera))]
public class OnCameraPreRenderCallback : MonoBehaviour {

	public UnityEvent_int	OnPreRenderEye;
	public int				LeftValue = 1;
	public int				RightValue = 0;
	Camera					ThisCamera;

	void Start()
	{
		ThisCamera = GetComponent<Camera> ();
	}


	void OnPreRender () 
	{
		var Left = (ThisCamera.stereoTargetEye == StereoTargetEyeMask.Left);
		OnPreRenderEye.Invoke (Left ? LeftValue : RightValue);
	}
}
