using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(RectTransform))]
public class RectTransformUV : MonoBehaviour {

	[InspectorButton("SetPosition00")]
	public bool _SetPosition00;

	[InspectorButton("SetPosition11")]
	public bool _SetPosition11;


	void SetPosition00()
	{
		SetPositionUv (new Vector2 (0, 0));
	}

	void SetPosition11()
	{
		SetPositionUv (new Vector2 (1, 1));
	}

	public void SetPositionUv(Vector2 PositionUv)
	{
		var rt = GetComponent<RectTransform> ();
		var rtparent = rt.parent;
		var rtparentrt = rtparent.GetComponent<RectTransform> ();

		var rect = rtparentrt.rect;

		var Pos3 = new Vector3 (0, 0, rt.localPosition.z);
		Pos3.x = Mathf.Lerp (rect.xMin, rect.xMax, PositionUv.x);
		Pos3.y = 1 - Mathf.Lerp (rect.yMin, rect.yMax, PositionUv.y);

		rt.localPosition = Pos3;
	}
}

/*
#if UNITY_EDITOR
[CustomEditor(typeof(RectTransformUV))]
public class RectTransformUVEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		var RT = (RectTransformUV)target;

		EditorGUI.DrawRect( 

		myTarget.experience = EditorGUILayout.IntField("Experience", myTarget.experience);
		EditorGUILayout.LabelField("Level", myTarget.Level.ToString());
	}
}
#endif
*/
