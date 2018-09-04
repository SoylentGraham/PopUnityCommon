using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif



[System.AttributeUsage(System.AttributeTargets.Field)]
public class MicrophoneNameAttribute : PropertyAttribute
{
}



#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MicrophoneNameAttribute))]
public class MicrophoneNameAttributePropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var Attrib = (MicrophoneNameAttribute)attribute;
		var TargetObject = property.serializedObject.targetObject;

		var MicrophoneName = property.stringValue;

		EditorGUI.BeginChangeCheck();

		var Options = new List<string>();
		var OtherIndex = Options.Count;
		var OtherLabel = "Custom...";
		Options.Add(OtherLabel);
		int SelectedIndex = OtherIndex;
		var Devices = Microphone.devices;
		foreach (var Device in Devices)
		{
			if (MicrophoneName.Equals(Device))
				SelectedIndex = Options.Count;
			Options.Add(Device);
		}


		position.height = base.GetPropertyHeight(property, label);

		var WasMatch = (SelectedIndex != OtherIndex);
		SelectedIndex = EditorGUI.Popup(position, property.displayName, SelectedIndex, Options.ToArray());

		if (SelectedIndex == OtherIndex)
		{
			//	need to reset the label if it changed, otherwise it'll revert back to a selection again
			if ( WasMatch )
				MicrophoneName = "";
			position.y += position.height;
			MicrophoneName = EditorGUI.TextField(position, OtherLabel, MicrophoneName);
		}
		else
		{
			MicrophoneName = Options[SelectedIndex];
		}

		if (EditorGUI.EndChangeCheck())
		{
			property.stringValue = MicrophoneName;
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


