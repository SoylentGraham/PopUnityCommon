using UnityEngine;
using System.Collections;



[ExecuteInEditMode]
public class ShaderBlit : MonoBehaviour {

	public bool					Dirty = true;
	public Texture				Input;
	public Shader				BlitShader;
	public Material				BlitMaterial;
	public RenderTexture		Output;
	public UnityEngine.Events.UnityEvent	OnClean;

	public void SetDirty()
	{ 
		Dirty = true;
	}

	void Update ()
	{
		if ( !Dirty )
			return;

		if ( Input == null )
			return;

		if (BlitShader != null)
		{
			if ( BlitMaterial == null )
				BlitMaterial = new Material( BlitShader );
		}

		if ( BlitMaterial == null )
			return;

		Graphics.Blit( Input, Output, BlitMaterial );

		Dirty = false;

		if ( OnClean != null )
			OnClean.Invoke();

	}
}
