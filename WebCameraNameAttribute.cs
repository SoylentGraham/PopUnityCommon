using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif



[System.AttributeUsage(System.AttributeTargets.Field)]
public class WebCamNameAttribute : PropertyAttribute
{
}



#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(WebCamNameAttribute))]
public class WebCamNameAttributePropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var Attrib = (WebCamNameAttribute)attribute;
		var TargetObject = property.serializedObject.targetObject;

		var WebCamName = property.stringValue;

		EditorGUI.BeginChangeCheck();

		var Options = new List<string>();
		var OtherIndex = Options.Count;
		var OtherLabel = "Custom...";
		Options.Add(OtherLabel);
		int SelectedIndex = OtherIndex;
		var Devices = WebCamTexture.devices;
		foreach (var Device in Devices)
		{
			if (WebCamName.Equals(Device.name))
				SelectedIndex = Options.Count;
			Options.Add(Device.name);
		}


		position.height = base.GetPropertyHeight(property, label);

		var WasMatch = (SelectedIndex != OtherIndex);
		SelectedIndex = EditorGUI.Popup(position, property.displayName, SelectedIndex, Options.ToArray());

		if (SelectedIndex == OtherIndex)
		{
			//	need to reset the label if it changed, otherwise it'll revert back to a selection again
			if ( WasMatch )
				WebCamName = "";
			position.y += position.height;
			WebCamName = EditorGUI.TextField(position, OtherLabel, WebCamName);
		}
		else
		{
			WebCamName = Options[SelectedIndex];
		}

		if (EditorGUI.EndChangeCheck())
		{
			property.stringValue = WebCamName;
		}


		property.serializedObject.ApplyModifiedProperties();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		var OrigHeight = base.GetPropertyHeight(property, label);
		return OrigHeight * 2;
	}
}
#endif


