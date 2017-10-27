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
	const int SingleTextureHeight = 200;
	const int Spacing = 5;
	const int EnumHeight = 20;

	public enum TexturePreviewMode
	{
		Colour,
		Alpha,
		ColourAndAlpha,
		Mixed,
	};

	static TexturePreviewMode	PreviewMode = TexturePreviewMode.Colour;
	static int					GetTexturePreviewCount()
	{
		switch ( PreviewMode )
		{
		case TexturePreviewMode.ColourAndAlpha:
			return 2;

		default:
			return 1;
		}
	}
	static int					GetTextureHeight()
	{
		return SingleTextureHeight * GetTexturePreviewCount ();
	}

	static public Rect EatRect(ref Rect rect,float EatHeight)
	{
		var SubRect = new Rect (rect);
		SubRect.height = EatHeight;

		rect.y += EatHeight;
		rect.height -= EatHeight;

		return SubRect;
	}



	public static void Draw(Rect rect,Texture texture,TexturePreviewMode Mode)
	{
		//	see http://answers.unity3d.com/questions/377207/drawing-a-texture-in-a-custom-propertydrawer.html
		/*	gr: this check is done internally; https://github.com/MattRix/UnityDecompiled/blob/b4b209f8d1c93d66f560bf23c81bc0910cef177c/UnityEditor/UnityEditor/EditorGUI.cs#L6651
		if (Event.current.type != EventType.Repaint) {
			return;
		}
		*/
		if (texture == null) {
			EditorGUI.LabelField (rect, "null");
			return;
		}
	
		switch ( Mode )
		{
		case TexturePreviewMode.Colour:
			//Graphics.DrawTexture (rect, texture);
			EditorGUI.DrawPreviewTexture( rect, texture );
			//GUI.DrawTexture( rect, texture, ScaleMode.ScaleToFit, false );
			return;

		case TexturePreviewMode.Alpha:
			//	gr: need an alternative to this
			EditorGUI.DrawTextureAlpha( rect, texture );
			return;

		case TexturePreviewMode.Mixed:
			EditorGUI.DrawTextureTransparent( rect, texture );
			//GUI.DrawTexture( rect, texture, ScaleMode.ScaleToFit, true );
			return;

		case TexturePreviewMode.ColourAndAlpha:
			var ColourRect = EatRect (ref rect, SingleTextureHeight);
			var AlphaRect = EatRect (ref rect, SingleTextureHeight);
			Draw (ColourRect, texture, TexturePreviewMode.Colour);
			Draw (AlphaRect, texture, TexturePreviewMode.Alpha);
			break;
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

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var Attrib = (TexturePreviewAttribute)attribute;
		var TargetObject = property.serializedObject.targetObject;

		//	show base selector
		var BaseHeight = base.GetPropertyHeight ( property,  label);
		var PropRect = EatRect (ref position, BaseHeight);
		EditorGUI.PropertyField (PropRect, property, label, true);
	
		//	space
		/*var SpaceRect = */EatRect (ref position, Spacing);
		//GUILayout.Space (Spacing);

		var EnumRect = EatRect (ref position, EnumHeight);
		PreviewMode = (TexturePreviewMode)EditorGUI.EnumPopup (EnumRect, "Preview Mode", PreviewMode as System.Enum);

		try
		{
			var Texture = GetPropertyAsTexture( property );
			Draw(position, Texture, PreviewMode );
		}
		catch
		{
			//EditorGUI.HelpBox (position, e.Message, MessageType.Error);
			Debug.Log("Error");
		}
	}

	public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
	{
		var Attrib = (TexturePreviewAttribute)attribute;
		var TargetObject = property.serializedObject.targetObject;

		var Height = base.GetPropertyHeight ( property,  label);

		Height += Spacing;
		Height += EnumHeight;
		Height += GetTextureHeight();

		return Height;
	}

}
#endif
