using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

[System.AttributeUsage(System.AttributeTargets.Field)]
public class TexturePreviewAttribute : PropertyAttribute
{
	public TexturePreviewAttribute()
	{
	}


}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TexturePreviewAttribute))]
public class TexturePreviewAttributePropertyDrawer : PropertyDrawer
{
	const int TextureHeight = 200;
	const int Spacing = 5;

	static Texture GetPropertyAsTexture(SerializedProperty property)
	{
		//	not an object
		if (property.propertyType != SerializedPropertyType.ObjectReference)
			throw new System.Exception ("Property " + property.name + " is " + property.propertyType);

		var t = property.objectReferenceValue as Texture;
		return t;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var Attrib = (TexturePreviewAttribute)attribute;
		var TargetObject = property.serializedObject.targetObject;

		//	show base selector
		EditorGUI.PropertyField (position, property, label, true);
	
		//	now show the rest
		var BaseHeight = base.GetPropertyHeight ( property,  label);

		position.height -= BaseHeight;
		position.y += BaseHeight;

		position.height -= Spacing;
		position.y += Spacing;

		try
		{
			var Texture = GetPropertyAsTexture( property );
			EditorGUI.DrawPreviewTexture (position, Texture);
		}
		catch(System.Exception e) {
			EditorGUI.HelpBox (position, e.Message, MessageType.Error);
		}
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		var Attrib = (TexturePreviewAttribute)attribute;
		var TargetObject = property.serializedObject.targetObject;

		var Height = base.GetPropertyHeight ( property,  label);

		Height += Spacing;
		Height += TextureHeight;

		return Height;
	}

}
#endif
