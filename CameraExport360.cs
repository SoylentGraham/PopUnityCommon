using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



public class CameraExport360 {

	[Range(0, 100)]
	const int JpegQuality = 100;

	#if UNITY_EDITOR
	[MenuItem("CONTEXT/Camera/Capture 360 to png...")]
	public static void Capture360ToPng(MenuCommand menuCommand)
	{
		Capture360ToFile(menuCommand, PopX.IO.ImageFileType.png);
	}
#endif

	#if UNITY_EDITOR
	[MenuItem("CONTEXT/Camera/Capture 360 to jpeg...")]
	public static void Capture360ToJpeg(MenuCommand menuCommand)
	{
		Capture360ToFile(menuCommand, PopX.IO.ImageFileType.jpg);
	}
#endif

	#if UNITY_EDITOR
	[MenuItem("CONTEXT/Camera/Capture 360 to exr...")]
	public static void Capture360ToExr(MenuCommand menuCommand)
	{
		Capture360ToFile(menuCommand, PopX.IO.ImageFileType.exr);
	}
#endif

	#if UNITY_EDITOR
	public static void Capture360ToFile(MenuCommand menuCommand,PopX.IO.ImageFileType FileType)
	{
		var cam = menuCommand.context as Camera;
		var Texture = Capture360ToTexture(cam);

		var DefaultName = cam.name + "_360";
		System.Action<string, PopX.IO.ImageFileType> SaveTextureToFile = (Filename, Extension) =>
		{
			byte[] Bytes = null;

			switch (Extension)
			{
				case PopX.IO.ImageFileType.jpg:
					Bytes = Texture.EncodeToJPG(JpegQuality);
					break;

				case PopX.IO.ImageFileType.png:
					Bytes = Texture.EncodeToPNG();
					break;

				case PopX.IO.ImageFileType.exr:
					Bytes = Texture.EncodeToEXR();
					break;

				default:
					throw new System.Exception("Don't know how to export texture to " + Extension);
			}

			System.IO.File.WriteAllBytes(Filename, Bytes);
		};

		PopX.IO.SaveFile(DefaultName,FileType,SaveTextureToFile);
	}
#endif

	public Shader GetCubemapToEquirectShader()
	{
		return null;
	}

	public static Texture2D Capture360ToTexture(Camera cam)
	{
		var rt = RenderTexture.GetTemporary(4096, 4096, 0, RenderTextureFormat.ARGB32);
		try
		{
			Capture360ToRenderTexture(cam, rt);
			var Output = SaveTextureToPng.GetTexture2D(rt, false);
			RenderTexture.ReleaseTemporary(rt);
			return Output;
		}
		catch
		{
			RenderTexture.ReleaseTemporary(rt);
			throw;
		}
	}

	public static void Capture360ToRenderTexture(Camera cam,RenderTexture Target)
	{
		Target.dimension = UnityEngine.Rendering.TextureDimension.Cube;
		if ( !cam.RenderToCubemap(Target) )
			throw new System.Exception("cam(" + cam.name + ").RenderToCubemap failed");

		//	todo: take a temp texture, turn it into an equirect and return
	}

}
