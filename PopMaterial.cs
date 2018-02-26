using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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

	}

}
