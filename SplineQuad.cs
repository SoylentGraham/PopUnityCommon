using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[System.Serializable]
public class UnityEvent_ListOfVector3 : UnityEngine.Events.UnityEvent <List<Vector3>> {}

[System.Serializable]
public class UnityEvent_ArrayOfVector3 : UnityEngine.Events.UnityEvent <Vector3[]> {}


[ExecuteInEditMode]
public class SplineQuad : MonoBehaviour {


	[Range(0.0001f,1.0f)]
	public float			MinDistanceToPushPosition = 0.1f;

	const int MAX_SPLINE_POINTS = 20;	//	see shader
	public string			WorldPositionsUniform = "WorldPositions";
	public string			WorldPositionsCountUniform = "WorldPositionsCount";

	[Header("If null, THIS is used")]
	public Transform		_TheTrackObject;
	public Transform		TrackObject	{	get{	return _TheTrackObject!=null ? _TheTrackObject : this.transform;	}}
	public Material			_SplineMaterial;
	public Material			SplineMaterial	{	get{	return _SplineMaterial ? _SplineMaterial : GetSplineMaterial ();	}}
	public bool				_UseSharedMaterial = true;
	public bool				UseSharedMaterial	{	get{	return (Application.isEditor && !Application.isPlaying) ? true : _UseSharedMaterial;	}}
	public bool				AutoTrack = true;

	List<Vector4>			Positions;
	Vector4					TrackedPosition		{	get	{	return TrackObject.position;	}}

	[Range(0.0f,10.0f)]
	public float 			Debug_Radius = 0.5f;

	public UnityEvent_ArrayOfVector3	OnPositionsChanged;

	[InspectorButton("RecordPosition")]
	public bool _RecordPosition;

	public void				RecordPosition()
	{
		PushPosition( TrackedPosition );
	}

	public void UpdateRootValue()
	{
		var WorldPosition = TrackedPosition;

		Pop.AllocIfNull (ref Positions);
		if (Positions.Count == 0)
			Positions.Add (WorldPosition);

		Positions [0] = WorldPosition;
	}

	public void				PushPosition(Vector4 WorldPosition)
	{
		Pop.AllocIfNull (ref Positions);

		//	only add if significant
		if (Positions.Count > 1) {
			var DistanceToLastPos = Vector3.Distance (WorldPosition, Positions [1] );
			if (DistanceToLastPos < MinDistanceToPushPosition) {
				return;
			}
		}

		Positions.Insert(0,WorldPosition);
		if (Positions.Count > MAX_SPLINE_POINTS)
			Positions.RemoveRange (MAX_SPLINE_POINTS, Positions.Count - MAX_SPLINE_POINTS);

		var Positions4 = new Vector3[Positions.Count];
		for (int i = 0;	i < Positions.Count;	i++)
			Positions4 [i] = Positions [i];

		OnPositionsChanged.Invoke (Positions4);
	}

	Material GetSplineMaterial()
	{
		try
		{
			var mr = GetComponent<MeshRenderer>();
			var mat = UseSharedMaterial ? mr.sharedMaterial : mr.material;
			if ( mat != null )
				return mat;
		}
		catch{
		}

		try
		{
			var mr = GetComponent<SpriteRenderer>();
			var mat = UseSharedMaterial ? mr.sharedMaterial : mr.material;
			if ( mat != null )
				return mat;
		}
		catch{
		}

		return null;
	}

	void OnEnable()
	{
		Positions = null;
	}

	void UpdateMaterial()
	{
		var Points = new Vector4[MAX_SPLINE_POINTS];
		Positions.CopyTo( Points );

		var mat = SplineMaterial;
		if (mat != null) {
			mat.SetVectorArray (WorldPositionsUniform, Points);
			mat.SetInt (WorldPositionsCountUniform, Positions.Count);
		}
	}

	void Update () 
	{
		UpdateRootValue ();

		if (AutoTrack)
			RecordPosition ();
		
		UpdateMaterial ();
	}

	void OnDrawGizmos() 
	{
		if ( Positions != null && Positions.Count > 0 )
		{
			Gizmos.color = Color.yellow;

			var Points = new List<Vector3> ();
			var Radius = SplineMaterial.HasProperty("Radius") ? SplineMaterial.GetFloat("Radius") : Debug_Radius;

			foreach (var Pos in Positions) {
				var WorldPos = Pos;
				Points.Add (WorldPos);
			}

			Gizmos.DrawWireSphere (Points[0], Debug_Radius);
			for ( int i=1;	i<Points.Count;	i++ )
			{
				Gizmos.DrawLine (Points[i-1], Points[i]);

				Gizmos.DrawWireSphere (Points[i], Debug_Radius);
			}
		}
	}
}
