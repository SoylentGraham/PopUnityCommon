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

	static public Texture2D GetTexture2D(Texture Tex,bool Alpha)
	{ 
		return GetTexture2D( Tex, Alpha ? TextureFormat.ARGB32 : TextureFormat.RGB24 );
	}
		
	static public void GetTexture2D(Texture Tex,ref Texture2D Target,TextureFormat Format)
	{ 
		//	copy to render texture and read
		RenderTexture rt = RenderTexture.GetTemporary( Tex.width, Tex.height, 0, RenderTextureFormat.ARGBFloat );
		Graphics.Blit( Tex, rt );
		if (Target == null) {
			Target = new Texture2D (rt.width, rt.height, Format, false);
		}
		RenderTexture.active = rt;
		Target.ReadPixels( new Rect(0,0,rt.width,rt.height), 0, 0 );
		Target.Apply();
		RenderTexture.active = null;
		RenderTexture.ReleaseTemporary( rt );
	}
		
	static public Texture2D GetTexture2D(Texture Tex,TextureFormat Format)
	{ 
		if ( Tex is Texture2D )
			return Tex as Texture2D;
	
		Texture2D Temp = null;
		GetTexture2D (Tex, ref Temp, Format);
		return Temp;
	}


	static public bool DoSaveTextureToPng(Texture Tex,string Filename,bool Alpha)
	{
		var Temp = GetTexture2D( Tex, Alpha );

		byte[] Bytes = Temp.EncodeToPNG();
		File.WriteAllBytes( Filename, Bytes );
		return true;
	}
}
