using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


namespace PopX
{
	public static class Materials
	{
		const string InstanceSuffix = "Instance";

		static public bool IsInstanceName(string Name)
		{
			if (Name.ToLower().Contains(InstanceSuffix.ToLower()))
				return true;

			return false;
		}

		public static Material GetInstancedMaterial(Material mat)
		{
			//	don't make instances in edit mode
			if (Application.isEditor && !Application.isPlaying)
			{
				return mat;
			}

			//	check if current is an instance
			if (IsInstanceName(mat.name))
				return mat;

			//	make instance
			mat = new Material(mat);
			mat.name += " " + InstanceSuffix;
			if (!IsInstanceName(mat.name))
				throw new System.Exception("New name " + mat.name + " isn't an instance");
			return mat;
		}


#if UNITY_EDITOR
		public static Dictionary<string, System.Type> GetProperties(Shader shader)
		{
			//	get file contents of asset
			var Filename = UnityEditor.AssetDatabase.GetAssetPath(shader);
			var Contents = System.IO.File.ReadAllText(Filename);

			return GetProperties(Contents);
		}
#endif

		public static System.Type GetType(string TypeName)
		{
			if (TypeName == "2D") return typeof(Texture);
			if (TypeName == "VECTOR") return typeof(Vector4);
			if (TypeName == "COLOR") return typeof(Color);
			throw new System.Exception("Unhandled shader typename " + TypeName);
		}

		public static Dictionary<string, System.Type> GetProperties(string Shader)
		{
			//	get file contents of asset
			var Contents = Shader;

			//	find properties section
			//var Pattern = new Regex(@"([_A-Za-z0-9]+)\s*\(\s*""[_A-Za-z0-9]+""\s*,");

			var Pattern_Name = @"([_A-Za-z0-9]+)\s*";    //	xyz
			var Pattern_OpenParenth = @"\(\s*";			//	(
			var Pattern_Label = @"""([_A-Za-z0-9]+)""\s*,\s*";	//	"xyz",
			var Pattern_Type = "(2D|COLOR|VECTOR)";     //	
			var Pattern_CloseParenth = @"\s*\)\s*";      //	)
			var Pattern = new Regex(Pattern_Name + Pattern_OpenParenth + Pattern_Label + Pattern_Type + Pattern_CloseParenth);
			var Matches = Pattern.Matches(Contents);

			//	gr: change this to empty return if it's valid to have no properties. (Guess it is)
			if (Matches == null)
				throw new System.Exception("Could not find any matches in shader");

			//Debug.Log("Match count " + Matches.Count);
			var Properties = new Dictionary<string, System.Type>();

			foreach (var MatchObj in Matches)
			{
				var Match = (Match)MatchObj;

				try
				{
					/*
				Debug.Log("Group count = " + Match.Groups.Count );
				var Found = "";
				foreach ( var GroupObj in Match.Groups )
				{
					var Group = (Group)GroupObj;
					var Capture = Group.Captures[0];
					Found += Capture.Value + " ---> ";
				}
				Debug.Log(Found);
*/
					var Name = Match.Groups[1].Captures[0].Value;
					var Label = Match.Groups[2].Captures[0].Value;
					var TypeString = Match.Groups[3].Captures[0].Value;
					var Type = GetType(TypeString);
					Properties.Add( Name, Type);
				}
				catch(System.Exception e)
				{
					Debug.LogError("Faled to parse property match; " + e.Message);
				}
			}

			return Properties;
		}
	}

}

