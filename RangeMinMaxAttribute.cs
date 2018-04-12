using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif



[System.AttributeUsage(System.AttributeTargets.Field)]
public class RangeMinMaxAttribute : PropertyAttribute
{
	public float Min;
	public float Max;
	public bool ShowNumbers;

	public RangeMinMaxAttribute(float Min,float Max,bool ShowNumbers=true)
	{
		this.Min = Min;
		this.Max = Max;
		this.ShowNumbers = ShowNumbers;
	}
}



#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(RangeMinMaxAttribute))]
public class RangeMinMaxAttributeDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var Attrib = (RangeMinMaxAttribute)attribute;

		var xy = property.vector2Value;

		//	work out rects for each bit
		var LeftWidth = position.height*3;
		var LeftStart = position.xMin + EditorGUIUtility.labelWidth;
		var LeftEnd = LeftStart + LeftWidth;
		var RightEnd = position.xMax;
		var RightStart = RightEnd - LeftWidth;
		if ( !Attrib.ShowNumbers )
		{
			LeftEnd = LeftStart;
			RightStart = RightEnd;
		}
		var MiddleStart = LeftEnd;
		var MiddleEnd = RightStart;

		var LeftRect = new Rect(LeftStart, position.y, LeftEnd - LeftStart, position.height);
		var RightRect = new Rect(RightStart, position.y, RightEnd - RightStart, position.height);
		var MiddleRect = new Rect(MiddleStart, position.y, MiddleEnd - MiddleStart, position.height);

		EditorGUI.LabelField(position,label);
		xy.x = EditorGUI.FloatField(LeftRect, xy.x);
		EditorGUI.MinMaxSlider( MiddleRect,ref xy.x, ref xy.y, Attrib.Min, Attrib.Max);
		xy.y = EditorGUI.FloatField(RightRect, xy.y);

		property.vector2Value = xy;
	}
}
#endif


