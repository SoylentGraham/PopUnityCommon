using System.Collections;
using System.Collections.Generic;
using UnityEngine;



#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif



[System.AttributeUsage(System.AttributeTargets.Field)]
public class SceneNameAttribute : PropertyAttribute
{
}



#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneNameAttribute))]
public class SceneNameAttributePropertyDrawer : PropertyDrawer
{
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var Attrib = (SceneNameAttribute)attribute;
		var TargetObject = property.serializedObject.targetObject;

		var SceneName = property.stringValue;


		SceneAsset Scene = null;
		try
		{
			if ( !string.IsNullOrEmpty(SceneName) )
			{
				var SceneGuids = AssetDatabase.FindAssets ("t:scene " + SceneName);
				var SceneGuid = SceneGuids[0];
				var ScenePath = AssetDatabase.GUIDToAssetPath( SceneGuid );
				Scene = AssetDatabase.LoadAssetAtPath<SceneAsset>( ScenePath );
			}
		}
		catch {
		}

		//serializedObject.Update();

		EditorGUI.BeginChangeCheck();

		//Scene = EditorGUI.ObjectField( position, property.displayName, Scene, typeof(SceneAsset), false) as SceneAsset;
		Scene = EditorGUI.ObjectField( position, property.displayName, Scene, typeof(SceneAsset), false ) as SceneAsset;

		if (EditorGUI.EndChangeCheck())
		{
			property.stringValue = Scene ? Scene.name : null;
		}

		property.serializedObject.ApplyModifiedProperties();

	}

}
#endif


