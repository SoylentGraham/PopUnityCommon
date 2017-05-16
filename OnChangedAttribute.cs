using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

[System.AttributeUsage(System.AttributeTargets.Field)]
public class OnChangedAttribute : PropertyAttribute
{
	public readonly string	FunctionName;

	public OnChangedAttribute(string _FunctionName)
	{
		this.FunctionName = _FunctionName;
	}

}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(OnChangedAttribute))]
public class OnChangedAttributePropertyDrawer : PropertyDrawer
{
	private MethodInfo CachedEventMethodInfo = null;


	void CallOnChanged(Object TargetObject,OnChangedAttribute Attrib)
	{
		var TargetObjectType = TargetObject.GetType();

		if (CachedEventMethodInfo == null)
			CachedEventMethodInfo = TargetObjectType.GetMethod(Attrib.FunctionName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		if (CachedEventMethodInfo == null) {
			Debug.LogWarning("OnChangedAttribute: Unable to find method "+ Attrib.FunctionName + " in " + TargetObjectType);
			return;
		}

		CachedEventMethodInfo.Invoke (TargetObject, null);
	}


	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var Attrib = (OnChangedAttribute)attribute;
		var TargetObject = property.serializedObject.targetObject;


		EditorGUI.BeginChangeCheck ();

		//base.OnGUI (position, prop, label);
		EditorGUI.PropertyField (position, property, label, true);

		if (EditorGUI.EndChangeCheck ()) {
			CallOnChanged (TargetObject, Attrib);
		}
	}
	/*
	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		var Attrib = (ShowIfAttribute)attribute;
		var TargetObject = property.serializedObject.targetObject;

		if ( IsVisible(TargetObject,Attrib)) {
			//base.OnGUI (position, prop, label);
			return base.GetPropertyHeight ( property,  label);
		}
		return 0;
	}
*/
}
#endif


