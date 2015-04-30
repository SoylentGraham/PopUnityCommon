using UnityEngine;
using System.Collections;


/*
	There's a bug in unity 5.0.1f1 where using Graphics.Blit() crashes in Update()(where I NEED to do blits) if there is no current camera set.
	Possibly due to specific texture types...
	Call this beforehand.
*/
public class CheckCurrentCamera : MonoBehaviour {

	static public bool CheckCurrentCamera()
	{
		if (!Camera.current)
			Camera.SetupCurrent (Camera.main);
		return Camera.current != null;
	}
}
