using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureBlitChain : MonoBehaviour {

	public List<Material>		mShaders;

	public void Execute (RenderTexture mInput,RenderTexture mOutput) 
	{
		if (!mInput || !mOutput)
			return;

		RenderTexture BufferIn = RenderTexture.GetTemporary (mInput.width, mInput.height, mInput.depth, mInput.format);
		RenderTexture BufferOut = RenderTexture.GetTemporary (mInput.width, mInput.height, mInput.depth, mInput.format);
		Graphics.Blit (mInput, BufferOut);

		for (int i=0; i<mShaders.Count; i++) {
			//	last output, is now input
			RenderTexture Swap = BufferOut;
			BufferOut = BufferIn;
			BufferIn = Swap;

			if( mShaders[i] != null )
				Graphics.Blit (BufferIn, BufferOut, mShaders [i]);
			else
				Graphics.Blit (BufferIn, BufferOut);
		}

		//	output
		Graphics.Blit (BufferOut, mOutput);

		RenderTexture.ReleaseTemporary (BufferIn);
		RenderTexture.ReleaseTemporary (BufferOut);

	}
}
