using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif


#if UNITY_EDITOR
public class CameraPreviewWindow : UnityEditor.EditorWindow
{
	public Camera mCamera;
	public RenderTexture mRenderTexture;

	// Add menu named "My Window" to the Window menu
	[MenuItem ("Camera Preview Window/Create Camera Preview Window")]
	static void Init () 
	{
		//	create a new window
		var NewWindow  = EditorWindow.CreateInstance<CameraPreviewWindow>();
		NewWindow.ShowUtility();
	}

	void UpdateRenderTexture(Rect WindowSize)
	{
		//	render camera
		if (mCamera == null) {
			mRenderTexture = null;
		} else {
			if (mRenderTexture != null && (mRenderTexture.width != WindowSize.width || mRenderTexture.height != WindowSize.height)) {
				mRenderTexture = null;
			}

			if (mRenderTexture == null) {
				int CameraRenderTextureDepth = (mCamera.targetTexture != null) ? mCamera.targetTexture.depth : 24;
				mRenderTexture = new RenderTexture ( (int)WindowSize.width, (int)WindowSize.height, CameraRenderTextureDepth);
			}

			var OldTarget = mCamera.targetTexture;
			mCamera.targetTexture = mRenderTexture;
			mCamera.Render ();
			mCamera.targetTexture = OldTarget;

		}
	}

	void SnapWindowToDisplay()
	{
		if ( mCamera == null )
			return;

		//	get disp
	}

	void OnGUI()
	{
		var Rect = this.position;
		Rect.x = 0;
		Rect.y = 0;
			
		//	draw camera selection
		{
			int SelectedCamera = 0;
			List<string> CameraNames = new List<string> ();
			CameraNames.Add ("null");

			//	add the other cameras in the scene
			foreach (Camera Cam in Camera.allCameras) {
				CameraNames.Add (Cam.name);
				if (mCamera == Cam)
					SelectedCamera = CameraNames.Count - 1;
			}

			int NewSelectedCamera = EditorGUILayout.Popup ( SelectedCamera, CameraNames.ToArray(), GUILayout.ExpandWidth (true));

			//	changed camera
			if (NewSelectedCamera != SelectedCamera) {
				if (NewSelectedCamera == 0) {
					mCamera = null;
				} else {
					mCamera = Camera.allCameras [NewSelectedCamera -1];
				}
			}
		}

		{
			if (GUILayout.Button ("Snap to display")) {
				SnapWindowToDisplay ();
			}
		}


		//	draw the content!
		{
			//	move the rect to go past all the previous controls
			var LastBottom = GUILayoutUtility.GetLastRect ().yMax;
			Rect.y += LastBottom;
			Rect.height -= LastBottom;

			UpdateRenderTexture (Rect);
			if (mRenderTexture == null) {
				GUI.Box (Rect, "No camera");
			} else {
				GUI.DrawTexture (Rect, mRenderTexture);
			}
		}
	}
}
#endif
