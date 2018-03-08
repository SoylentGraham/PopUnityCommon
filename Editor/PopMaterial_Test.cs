using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NUnit.Framework;


namespace PopX
{
	public class Material_UnitTest
	{
		[Test]
		public void FindProperties()
		{
			var Shader = @"Shader ShaderName
			{
				Properties
				{
					Colour(""Colour"", COLOR) = (1, 1, 1, 1)
					TextureVar( ""SomeTexture"" , 2D ) = ""white""
				}
				SubShader
				{	
				}
			}";

			var Props = PopX.Materials.GetProperties(Shader);
			Assert.AreEqual(Props["Colour"], typeof(Color));
			Assert.AreEqual(Props["TextureVar"], typeof(Texture));
		
		}

	}
}

