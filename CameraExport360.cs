using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



public class CameraExport360 {

	[Range(0, 100)]
	const int JpegQuality = 100;
	const string BlitCubemapToEquirect_ShaderName = "BlitCubemapToEquirect";

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
		System.Action<PopX.IO.ImageFileType,string> SaveTextureToFile = (Extension,Filename) =>
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

	public static Shader GetCubemapToEquirectShader()
	{
#if UNITY_EDITOR
		var AssetGuids = AssetDatabase.FindAssets(BlitCubemapToEquirect_ShaderName);
		foreach (var AssetGuid in AssetGuids)
		{
			try
			{
				var AssetPath = AssetDatabase.GUIDToAssetPath(AssetGuid);
				var AssetShader = AssetDatabase.LoadAssetAtPath<Shader>(AssetPath);
				return AssetShader;
			}
			catch (System.Exception e)
			{
				Debug.LogError("Failed to grab shader asset " +  AssetGuid + ": " + e.Message );
			}
		}
		throw new System.Exception("Failed to find shader " + BlitCubemapToEquirect_ShaderName);
#else
		throw new System.Exception("This is currently editor only. Change caller to allow shader param to allow capture in-player");
#endif
	}

	public static Texture2D Capture360ToTexture(Camera cam)
	{
		var rt = RenderTexture.GetTemporary(4096, 4096, 0, RenderTextureFormat.ARGB32);
		try
		{
			Capture360ToRenderTexture(cam, rt);
			var Output = PopX.Textures.GetTexture2D(rt, false);
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

		//	take a temp texture, turn it into an equirect and return
		var EquirectShader = GetCubemapToEquirectShader();

		var EquirectTarget = RenderTexture.GetTemporary(Target.width, Target.height, 0, Target.format);
		try
		{
			var EquirectShader_CubemapUniform = "Cubemap";
			var EquirectShader_UseCubemapKeyword = "USE_CUBEMAP";

			var EquirectMaterial = new Material(EquirectShader);
			EquirectMaterial.SetTexture(EquirectShader_CubemapUniform, Target);
			EquirectMaterial.EnableKeyword(EquirectShader_UseCubemapKeyword);

			Graphics.Blit(Target, EquirectTarget, EquirectMaterial);
			Graphics.Blit( EquirectTarget, Target );
			RenderTexture.ReleaseTemporary(EquirectTarget);
		}
		catch
		{
			RenderTexture.ReleaseTemporary(EquirectTarget);
			throw;
		}
	}

}
