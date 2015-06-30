#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class SaveTextureToPng : MonoBehaviour {

	[MenuItem("Assets/Texture/Save Texture to Png")]
	static void _SaveTextureToPng()
	{
		//	get selected textures
		string[] AssetGuids = Selection.assetGUIDs;
		for (int i=0; i<AssetGuids.Length; i++) {
			string Guid = AssetGuids[i];
			string Path = AssetDatabase.GUIDToAssetPath (Guid);
			Texture Tex = AssetDatabase.LoadAssetAtPath( Path, typeof(Texture) ) as Texture;
			if ( !Tex )
				continue;

			DoSaveTextureToPng( Tex, Path, Guid );
		}
	}

	static bool DoSaveTextureToPng(Texture Tex,string AssetPath,string AssetGuid)
	{
		//	copy to render texture
		RenderTexture rt = RenderTexture.GetTemporary( Tex.width, Tex.height, 0, RenderTextureFormat.ARGB32 );
		Graphics.Blit( Tex, rt );
		Texture2D Temp = new Texture2D( rt.width, rt.height, TextureFormat.RGB24, false );
		RenderTexture.active = rt;
		Temp.ReadPixels( new Rect(0,0,rt.width,rt.height), 0, 0 );
		Temp.Apply();
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary( rt );

		byte[] Bytes = Temp.EncodeToPNG();
		string Filename = EditorUtility.SaveFilePanel("save " + AssetPath, "", AssetGuid, "png");
		if ( Filename.Length == 0 )
			return false;

		File.WriteAllBytes( Filename, Bytes );
		return true;
	}
}

#endif
