using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetMeshBoundsToCollider : MonoBehaviour {


	[InspectorButton("SetMeshFilterMeshBoundsToCollider")]
	public bool _SetMeshFilterMeshBoundsToCollider;


	void Start () {
	
		SetMeshFilterMeshBoundsToCollider ();
	}


	public void SetMeshFilterMeshBoundsToCollider()
	{
		var mf = GetComponent<MeshFilter> ();
		if (mf) {
			var Mesh = mf.mesh;
			SetMeshBoundsToMeshFilterMesh (Mesh);
		}

	}

	public void SetMeshBoundsToMeshFilterMesh(Mesh mesh)
	{
		var Collider = GetComponent<Collider> ();
		if (Collider && mesh) {
			mesh.bounds = Collider.bounds;
		}

	}
}


