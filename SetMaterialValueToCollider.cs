using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class UnityEvent_Vector4 : UnityEngine.Events.UnityEvent <Vector4> {}



[ExecuteInEditMode]
public class SetMaterialValueToCollider : MonoBehaviour {

	public UnityEvent_Vector4	OnColliderChanged;

	void Start () {

		var Sphere = GetComponent<SphereCollider> ();
		if (Sphere) {
			var Sphere4 = new Vector4 (Sphere.center.x, Sphere.center.y, Sphere.center.z, Sphere.radius);
			OnColliderChanged.Invoke (Sphere4);
		}
	}

	#if UNITY_EDITOR
	void Update()
	{
		Start();
	}
	#endif
	
}
