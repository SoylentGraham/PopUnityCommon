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
		
	static public void GetTexture2D(RenderTexture Tex,ref Texture2D Target,TextureFormat Format)
	{ 
		if (Target == null) {
			Target = new Texture2D (Tex.width, Tex.height, Format, false);
		}
		RenderTexture.active = Tex;
		Target.ReadPixels( new Rect(0,0,Tex.width,Tex.height), 0, 0 );
		Target.Apply();
		RenderTexture.active = null;
	}

	static public void GetTexture2D(Texture Tex,ref Texture2D Target,TextureFormat Format)
	{ 
		if ( Tex is RenderTexture )
		{
			GetTexture2D(Tex as RenderTexture, ref Target, Format);
			return;
		}

		//	copy to render texture and read
		RenderTexture rt = RenderTexture.GetTemporary( Tex.width, Tex.height, 0, RenderTextureFormat.ARGBFloat );
		Graphics.Blit( Tex, rt );
		GetTexture2D(Tex, ref Target, Format);
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

	static public void ClearTexture(Texture2D Tex,Color Colour,bool Apply=true)
	{
		//	gr: there's bound to be faster versions.
		//	but at least we only have one palce to optimise
		var Pixels = Tex.GetPixels();

		for (var i = 0; i < Pixels.Length; i++)
			Pixels[i] = Colour;
		
		Tex.SetPixels(Pixels);
		if (Apply)
			Tex.Apply();
	}

}
