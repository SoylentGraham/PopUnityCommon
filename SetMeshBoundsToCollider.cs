using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMeshBoundsToCollider : MonoBehaviour {

	void Start () {
	
		var Mesh = GetComponent<MeshFilter> ().mesh;
		var Collider = GetComponent<Collider> ();
		if (Collider && Mesh) {
			Mesh.bounds = Collider.bounds;
		}

	}
}


