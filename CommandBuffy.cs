using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Events;

[System.Serializable]
public class UnityEvent_OnCaptureTextureCreated : UnityEvent <Texture> {}


public class CommandBuffy : MonoBehaviour {

	public RenderTexture		Target;
	[ShowIfAttribute("TargetIsNull")]
	public RenderTextureFormat	TargetFormat = RenderTextureFormat.ARGBFloat;

	[Range(1,4096)]
	[ShowIfAttribute("TargetIsNull")]
	public int 					TargetWidth = 2048;

	[Range(1,4096)]
	[ShowIfAttribute("TargetIsNull")]
	public int 					TargetHeight = 2048;

	[Header("Called if we create a target so you can assign to a material etc, or just called in start")]
	public UnityEvent_OnCaptureTextureCreated	OnCaptureTextureCreated;

	[Header("When to capture from the screen")]
	public CameraEvent			CaptureAfterEvent = CameraEvent.AfterSkybox;

	[ShowIfAttribute("CaptureEventRequiresDepthMode")]
	public bool					SetCameraDepthMode = false;

	public BuiltinRenderTextureType	CaptureSource = BuiltinRenderTextureType.CurrentActive;

	public bool					AssignToAllCameras = true;

	[ShowIfAttribute("IsAssignToSpecificCameras")]
	public List<Camera>			AssignToCameraList;


	Dictionary<Camera,CommandBuffer>	CameraCommands;


	bool CaptureEventRequiresDepthMode()
	{
		switch ( CaptureAfterEvent )
		{
		case CameraEvent.AfterDepthNormalsTexture:
		case CameraEvent.AfterDepthTexture:
		case CameraEvent.BeforeDepthNormalsTexture:
		case CameraEvent.BeforeDepthTexture:
			return true;
		default:
			return false;
		}
	}


	bool TargetIsNull()
	{
		return Target == null;
	}

	bool IsAssignToSpecificCameras()
	{
		return !AssignToAllCameras;
	}

	void Start () 
	{
		if (Target == null) {
			Target = new RenderTexture (TargetWidth, TargetHeight, 0, TargetFormat);
		}
		OnCaptureTextureCreated.Invoke (Target);
	}

	void OnEnable()
	{
		var CameraList = AssignToAllCameras ? Camera.allCameras : AssignToCameraList.ToArray ();
		if (CameraList != null) {
			foreach (var Cam in CameraList) {
				CreateCommand (Cam);
			}
		}
	}

	void OnDisable()
	{
		if (CameraCommands != null) {
			foreach (var Cam in CameraCommands.Keys) {
				ReleaseCommand (Cam);
			}
		}
	}

	void CreateCommand(Camera Cam)
	{
		//	if command already exists, replace it
		ReleaseCommand(Cam);

		//	make new command
		var Command = new CommandBuffer();
		Command.name = this.name;

		var DepthNormalsEvent = (CaptureAfterEvent == CameraEvent.AfterDepthNormalsTexture) || (CaptureAfterEvent == CameraEvent.BeforeDepthNormalsTexture);
		var DepthEvent = (CaptureAfterEvent == CameraEvent.AfterDepthTexture) || (CaptureAfterEvent == CameraEvent.BeforeDepthTexture);
	
		if (DepthEvent && Cam.depthTextureMode == DepthTextureMode.None) {
			if (SetCameraDepthMode)
				Cam.depthTextureMode = DepthTextureMode.Depth;
			else
				Debug.LogWarning ("Trying to capture " + CaptureSource + " but camera depth mode is " + Cam.depthTextureMode);
		}

		if (DepthNormalsEvent && Cam.depthTextureMode != DepthTextureMode.DepthNormals) {
			if (SetCameraDepthMode)
				Cam.depthTextureMode = DepthTextureMode.DepthNormals;
			else
				Debug.LogWarning ("Trying to capture " + CaptureSource + " but camera depth mode is " + Cam.depthTextureMode);
		}

		Debug.Log ("Camera " + Cam.name + " depth mode is " + Cam.depthTextureMode);

		//	get target id
		var TargetId = new RenderTargetIdentifier(Target);
		var SourceId = new RenderTargetIdentifier (CaptureSource);

		Command.Blit (SourceId, TargetId);

		Cam.AddCommandBuffer (CaptureAfterEvent,Command);

		if ( CameraCommands == null )
			CameraCommands = new Dictionary<Camera,CommandBuffer> ();
		
		CameraCommands.Add (Cam, Command);
	}

	void ReleaseCommand(Camera Cam)
	{
		if (CameraCommands == null)
			return;
	
		if (!CameraCommands.ContainsKey (Cam))
			return;

		Cam.RemoveCommandBuffer (CaptureAfterEvent,CameraCommands [Cam]);
		CameraCommands [Cam].Release ();
		CameraCommands.Remove (Cam);
	}
	
}
