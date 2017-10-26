using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//	gr: PopX being renamed to Pop later
namespace PopX
{
	public static class IO
	{
		public static void WriteStringToFile (string Filename, string Data)
		{
			var FileHandle = System.IO.File.CreateText( Filename );
			FileHandle.Write(Data);
			FileHandle.Close();
		}
	}
}

