using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class PopFitCollider : MonoBehaviour {
	
	[MenuItem("NewChromantics/Collider/Fit collider to Children")]
	static void FitColliderToChildren() {
		foreach (GameObject rootGameObject in Selection.gameObjects) {
			if (!(rootGameObject.GetComponent<Collider>() ))
				continue;


			Renderer[] ChildRenderers = rootGameObject.GetComponentsInChildren<Renderer>();
			Renderer[] ThisRenderers = rootGameObject.GetComponents<Renderer>();
			var Renderers = new Renderer[ChildRenderers.Length + ThisRenderers.Length];
			ChildRenderers.CopyTo(Renderers, 0);
			ThisRenderers.CopyTo(Renderers, Renderers.Length);

			bool hasBounds = false;
			Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

			foreach( Renderer r in Renderers )
			{
				if (hasBounds) {
					bounds.Encapsulate(r.bounds);
				}
				else {
					bounds = r.bounds;
					hasBounds = true;
				}
			}

			//	set all collider components
			if ( rootGameObject.GetComponent<Collider>() is BoxCollider )
			{
				BoxCollider collider = (BoxCollider)rootGameObject.GetComponent<Collider>();
				collider.center = bounds.center - rootGameObject.transform.position;
				collider.size = bounds.size;
			}

			if ( rootGameObject.GetComponent<Collider>() is CapsuleCollider )
			{
				CapsuleCollider collider = (CapsuleCollider)rootGameObject.GetComponent<CapsuleCollider>();
				collider.center = bounds.center - rootGameObject.transform.position;
				Debug.Log(bounds);
				collider.height = (bounds.max.y - bounds.min.y) * 1.0f;
				collider.radius = (bounds.max.x - bounds.min.x) * 0.5f;

			}

		}
	}

}