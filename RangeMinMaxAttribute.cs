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
	public static bool DrawAttribute(Rect position,ref Vector2 Value,Vector2 MinMax,string Label,bool ShowNumbers)
	{
		var xy = Value;

		//	gr: label width is way too big for materials...
		var LabelWidth = EditorGUIUtility.labelWidth * 0.20f;
		//EditorGUIUtility.labelWidth = LabelWidth;

		//	work out rects for each bit
		var LeftWidth = position.height * 3;
		var LeftStart = position.xMin + LabelWidth;
		var LeftEnd = LeftStart + LeftWidth;
		var RightEnd = position.xMax;
		var RightStart = RightEnd - LeftWidth;
		if (!ShowNumbers)
		{
			LeftEnd = LeftStart;
			RightStart = RightEnd;
		}
		var MiddleStart = LeftEnd;
		var MiddleEnd = RightStart;

		var LabelRect = new Rect(position.x, position.y, LabelWidth, position.height);
		var LeftRect = new Rect(LeftStart, position.y, LeftEnd - LeftStart, position.height);
		var RightRect = new Rect(RightStart, position.y, RightEnd - RightStart, position.height);
		var MiddleRect = new Rect(MiddleStart, position.y, MiddleEnd - MiddleStart, position.height);

		EditorGUI.LabelField(LabelRect, Label);
		EditorGUI.BeginChangeCheck();

		if ( ShowNumbers )
			xy.x = EditorGUI.FloatField(LeftRect, xy.x);
		EditorGUI.MinMaxSlider(MiddleRect, ref xy.x, ref xy.y, MinMax.x, MinMax.y);
		if (ShowNumbers)
			xy.y = EditorGUI.FloatField(RightRect, xy.y);

		if (!EditorGUI.EndChangeCheck())
			return false;

		Value = xy;
		return true;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var Attrib = (RangeMinMaxAttribute)attribute;
		var MinMax = new Vector2(Attrib.Min, Attrib.Max);

		var Value2 = property.vector2Value;
		if (DrawAttribute(position, ref Value2, MinMax, label.text, Attrib.ShowNumbers))
		{
			property.vector2Value = Value2;
		}
	}
}
#endif


//	add to material property like
//	[RangeMinMax(0,1)]MyRange("MyRange", VECTOR) = (0,0,0,0)
public class RangeMinMaxDrawer : MaterialPropertyDrawer
{
	float ValueMin = 0;
	float ValueMax = 0;

	public RangeMinMaxDrawer(float Min, float Max)
	{
		this.ValueMin = Min;
		this.ValueMax = Max;
	}

	// Draw the property inside the given rect
	public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
	{
		var MinMax = new Vector2(ValueMin, ValueMax);
		var ShowNumbers = true;
		var Value2 = prop.vectorValue.xy();

		if (RangeMinMaxAttributeDrawer.DrawAttribute(position, ref Value2, MinMax, label, ShowNumbers))
		{
			var Value4 = Value2.xyzw(prop.vectorValue.z, prop.vectorValue.w);
			prop.vectorValue = Value4;
		}

	}
}

