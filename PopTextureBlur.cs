using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu ("PopUnityCommon/TextureBlur")]
public class PopTextureBlur : MonoBehaviour
{
	public RenderTexture	mInput;
	public RenderTexture	mOutput;
	public Rect				mDebugViewport = new Rect(0,0,1,1);


    [Range(0, 2)]
    public int downsample = 1;

    public enum BlurType {
        StandardGauss = 0,
        SgxGauss = 1,
    }

    [Range(0.0f, 10.0f)]
    public float blurSize = 3.0f;

    [Range(1, 4)]
    public int blurIterations = 2;

    public BlurType blurType= BlurType.StandardGauss;

    public Shader blurShader = null;
    private Material blurMaterial = null;


    public void OnDisable () {
        if (blurMaterial)
			UnityEngine.Object.DestroyImmediate (blurMaterial);
    }

	void Update()
	{
		Blur (mInput, mOutput);
	}


	public bool Blur(RenderTexture Source,RenderTexture Target)
	{
		if (!Source || !Target)
			return false;

		//	make/find assets if we need to
		if ( !blurShader )
			blurShader = Shader.Find("FastBlur");

		if (!blurMaterial && blurShader)
			blurMaterial = new Material (blurShader);


		if (!blurMaterial) {
			Graphics.Blit (Source, Target);
			return false;
		}
		float widthMod = 1.0f / (1.0f * (1<<downsample));
		
        blurMaterial.SetVector ("_Parameter", new Vector4 (blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
       	//source.filterMode = FilterMode.Bilinear;

		int rtW = Source.width >> downsample;
		int rtH = Source.height >> downsample;

        // downsample
		RenderTexture rt = RenderTexture.GetTemporary (rtW, rtH, 0, Source.format);

        rt.filterMode = FilterMode.Bilinear;
		Graphics.Blit (Source, rt, blurMaterial, 0);

        var passOffs= blurType == BlurType.StandardGauss ? 0 : 2;

        for(int i = 0; i < blurIterations; i++) {
            float iterationOffs = (i*1.0f);
            blurMaterial.SetVector ("_Parameter", new Vector4 (blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

            // vertical blur
            RenderTexture rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, Source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit (rt, rt2, blurMaterial, 1 + passOffs);
            RenderTexture.ReleaseTemporary (rt);
            rt = rt2;

            // horizontal blur
            rt2 = RenderTexture.GetTemporary (rtW, rtH, 0, Source.format);
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit (rt, rt2, blurMaterial, 2 + passOffs);
            RenderTexture.ReleaseTemporary (rt);
            rt = rt2;
        }

		Target.DiscardContents ();
		Graphics.Blit (rt, Target);

        RenderTexture.ReleaseTemporary (rt);
       

		return true;
    }

	void OnGUI()
	{
		if ( mOutput && mDebugViewport.width+mDebugViewport.height>0 )
		{
			GUI.DrawTexture( new Rect(mDebugViewport.x*Screen.width, mDebugViewport.y*Screen.height, mDebugViewport.width*Screen.width, mDebugViewport.height*Screen.height ), mOutput );
		}
	}
}
