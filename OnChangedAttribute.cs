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
	public readonly string	BeforeChangeFunctionName;
	public readonly string	AfterChangeFunctionName;

	public OnChangedAttribute(string BeforeChangeFunctionName,string AfterChangeFunctionName)
	{
		this.BeforeChangeFunctionName = BeforeChangeFunctionName;
		this.AfterChangeFunctionName = AfterChangeFunctionName;
	}

}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(OnChangedAttribute))]
public class OnChangedAttributePropertyDrawer : PropertyDrawer
{
	private MethodInfo CachedEventMethodInfo = null;


	void CallOnChanged(Object TargetObject,OnChangedAttribute Attrib,string FunctionName)
	{
		if (string.IsNullOrEmpty (FunctionName))
			return;
		
		var TargetObjectType = TargetObject.GetType();

		if (CachedEventMethodInfo == null)
			CachedEventMethodInfo = TargetObjectType.GetMethod(FunctionName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		if (CachedEventMethodInfo == null) {
			Debug.LogWarning("OnChangedAttribute: Unable to find method "+ FunctionName + " in " + TargetObjectType);
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

			//	gr: property is changed, but not modified back to the serialised object at this point. 
			//	So we can do a pre-emptive call. But I don't know if calling ApplyModifiedProperties() is really bad here.
			CallOnChanged(TargetObject, Attrib, Attrib.BeforeChangeFunctionName);
			property.serializedObject.ApplyModifiedProperties();
			CallOnChanged(TargetObject, Attrib, Attrib.AfterChangeFunctionName);
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


