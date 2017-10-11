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
	const int EnumHeight = 20;

	public enum TexturePreviewMode
	{
		Colour,
		Alpha,
		Mixed,
	};

	static TexturePreviewMode	PreviewMode = TexturePreviewMode.Colour;

	public void Draw(Rect rect,Texture texture)
	{
		switch ( PreviewMode )
		{
		case TexturePreviewMode.Colour:
			EditorGUI.DrawPreviewTexture( rect, texture );
			return;

		case TexturePreviewMode.Alpha:
			EditorGUI.DrawTextureAlpha( rect, texture );
			return;

		case TexturePreviewMode.Mixed:
			EditorGUI.DrawTextureTransparent( rect, texture );
			return;
		}
	}

	static Texture GetPropertyAsTexture(SerializedProperty property)
	{
		//	not an object
		if (property.propertyType != SerializedPropertyType.ObjectReference)
			throw new System.Exception ("Property " + property.name + " is " + property.propertyType);

		var t = property.objectReferenceValue as Texture;
		return t;
	}

	Rect EatRect(ref Rect rect,float EatHeight)
	{
		var SubRect = new Rect (rect);
		SubRect.height = EatHeight;

		rect.y += EatHeight;
		rect.height -= EatHeight;
	
		return SubRect;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var Attrib = (TexturePreviewAttribute)attribute;
		var TargetObject = property.serializedObject.targetObject;

		//	show base selector
		var BaseHeight = base.GetPropertyHeight ( property,  label);
		var PropRect = EatRect (ref position, BaseHeight);
		EditorGUI.PropertyField (PropRect, property, label, true);
	
		//	space
		var SpaceRect = EatRect (ref position, Spacing);

		var EnumRect = EatRect (ref position, EnumHeight);
		PreviewMode = (TexturePreviewMode)EditorGUI.EnumPopup (EnumRect, "Preview Mode", PreviewMode as System.Enum);

		try
		{
			var Texture = GetPropertyAsTexture( property );
			Draw(position, Texture);
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
		Height += EnumHeight;
		Height += TextureHeight;

		return Height;
	}

}
#endif
