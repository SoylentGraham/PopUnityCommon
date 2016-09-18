using UnityEngine;
using System.Collections;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class SaveTextureToPng : MonoBehaviour {

	#if UNITY_EDITOR
	[MenuItem("Assets/Texture/Save Texture to Png No Alpha")]
	static void _SaveTextureToPng()
	{
		_SaveTextureToPng(false);
	}
	#endif

	#if UNITY_EDITOR
	[MenuItem("Assets/Texture/Save Texture to Png With Alpha")]
	static void _SaveTextureToPngWithAlpha()
	{
		_SaveTextureToPng(true);
	}
	#endif

	#if UNITY_EDITOR
	static void _SaveTextureToPng(bool Alpha)
	{
		//	get selected textures
		string[] AssetGuids = Selection.assetGUIDs;
		for (int i=0; i<AssetGuids.Length; i++) {
			string Guid = AssetGuids[i];
			string Path = AssetDatabase.GUIDToAssetPath (Guid);
			Texture Tex = AssetDatabase.LoadAssetAtPath( Path, typeof(Texture) ) as Texture;
			if ( !Tex )
				continue;

			DoSaveTextureToPng( Tex, Path, Guid, Alpha );
		}
	}
	#endif

	#if UNITY_EDITOR
	static public bool DoSaveTextureToPng(Texture Tex,string AssetPath,string AssetGuid,bool Alpha)
	{
		string Filename = EditorUtility.SaveFilePanel("save " + AssetPath, "", AssetGuid, "png");
		if ( Filename.Length == 0 )
			return false;

		return DoSaveTextureToPng( Tex, Filename, Alpha );
	}
	#endif

	static public bool DoSaveTextureToPng(Texture Tex,string Filename,bool Alpha)
	{
		//	copy to render texture
		RenderTexture rt = RenderTexture.GetTemporary( Tex.width, Tex.height, 0, RenderTextureFormat.ARGB32 );
		Graphics.Blit( Tex, rt );
		Texture2D Temp = new Texture2D( rt.width, rt.height, Alpha ? TextureFormat.ARGB32 : TextureFormat.RGB24, false );
		RenderTexture.active = rt;
		Temp.ReadPixels( new Rect(0,0,rt.width,rt.height), 0, 0 );
		Temp.Apply();
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary( rt );

		byte[] Bytes = Temp.EncodeToPNG();
		File.WriteAllBytes( Filename, Bytes );
		return true;
	}
}
