using UnityEngine;
using System.Collections;



[ExecuteInEditMode]
public class ShaderBlit : MonoBehaviour {

	[InspectorButton("Execute")]
	public bool					Dirty = true;
	public bool					AlwaysDirtyInEditor = true;
	public Texture				Input;
	public Shader				BlitShader;
	public Material				BlitMaterial;
	public RenderTexture		Output;
	public UnityEngine.Events.UnityEvent	OnClean;

	public void SetDirty()
	{ 
		Dirty = true;
	}

	public void Execute()
	{
		if (BlitShader != null)
		{
			if ( BlitMaterial == null )
				BlitMaterial = new Material( BlitShader );
		}

		if ( BlitMaterial == null )
			return;

		Graphics.Blit( Input, Output, BlitMaterial );

	}


	void Update ()
	{
		if ( Application.isEditor && !Application.isPlaying && AlwaysDirtyInEditor )
			Dirty = true;

		if ( !Dirty )
			return;

		if ( Input == null )
			return;

		Execute();

		Dirty = false;

		if ( OnClean != null )
			OnClean.Invoke();

	}
}
