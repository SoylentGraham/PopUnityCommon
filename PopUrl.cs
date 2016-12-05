using UnityEngine;
using System.Collections;
using System.IO;
using System;



public class PopUrl  {

	public static string	FileProtocol = "file://";
	public static string	StreamingAssetsProtocol = "streamingassets:";
	public static string	PersistentDataProtocol = "persistentdata:";
	public static string	SDCardProtocol = "sdcard:";


	#if UNITY_ANDROID &&  !UNITY_EDITOR
	static string GetExternalStoragePath() 
	{
		//	gr: updated OS and now legacy exists but is unreadable
		return "/sdcard/";
		return "/storage/self/primary/";
		return "/storage/emulated/legacy/";

		//	gr: this is crashing somewhere with a javavm throw with CallObjectMethodA
		try 
		{
			IntPtr obj_context = AndroidJNI.FindClass("android/os/Environment");
			IntPtr method_getFilesDir = AndroidJNIHelper.GetMethodID(obj_context, "getExternalStorageDirectory", "()Ljava/io/File;", true);

			using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) 
			{
				using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) 
				{
					IntPtr file = AndroidJNI.CallObjectMethod(obj_Activity.GetRawObject(), method_getFilesDir, new jvalue[0]);
					IntPtr obj_file = AndroidJNI.FindClass("java/io/File");
					IntPtr method_getAbsolutePath = AndroidJNIHelper.GetMethodID(obj_file, "getAbsolutePath", "()Ljava/lang/String;");  

					var Path = AndroidJNI.CallStringMethod(file, method_getAbsolutePath, new jvalue[0]);                    
					if ( Path == null )
						throw new Exception("getAbsolutePath returned null");
					return Path;
				}
			}
			return null;
		}
		catch(Exception e) 
		{
			Debug.LogError(e.ToString());
			return null;
		}
	}
	#endif



	//	okay, so WWW.escapeUrl escapes everything like,
	//		https%3a%2f%2fs3-us-west-2.amazonaws.com%2froomeditoruploads%2fkensington%2fKensington_01.RoomSource.json
	//	but we need to correct paths like
	//		http://www.thing.com/hello world.txt 
	//	as spaces are bad
	static public string		EscapeUrl(string Url)
	{
		//	+ or %20
		return Url.Replace(' ','+');
	}

	//	works as clarifier - returns true if file:
	static public bool			ResolveFileUrl(ref string Url)
	{
		if (!Url.ToLower ().StartsWith ( FileProtocol ))
			return false;
		return true;
	}

	public static string		MakeFileUrl(string FilePath)
	{
		//	gr: windows requires 3 slashes?
		return FileProtocol + FilePath;
	}

	static bool					ReplacePathUrl(ref string Url,string Prefix,string Path)
	{
		if (!Url.ToLower ().StartsWith (Prefix))
			return false;

		Path += Url.Substring (Prefix.Length);
		Url = MakeFileUrl( Path );
		return true;
	}

	static public bool			ResolveSdcardUrl(ref string Url)
	{
		#if UNITY_ANDROID && !UNITY_EDITOR
			string NewPath = GetExternalStoragePath ();
			if (NewPath == null)
				return false;

			return ReplacePathUrl( ref Url, SDCardProtocol, NewPath );
		#else
			//	no sdcard
			return false;
		#endif
	}

	static public bool			ResolveStreamingAssetsUrl(ref string Url)
	{
		return ReplacePathUrl( ref Url, StreamingAssetsProtocol, Application.streamingAssetsPath + "/" );
	}

	static public bool			ResolvePersistentDataUrl(ref string Url)
	{
		return ReplacePathUrl( ref Url, PersistentDataProtocol, Application.persistentDataPath + "/" );
	}


	static public bool			IsLocalUrl(string Url)
	{
		var TempUrl = Url;
		if (ResolveFileUrl (ref TempUrl))
			return true;

		if (ResolveSdcardUrl (ref TempUrl))
			return true;

		if (ResolveStreamingAssetsUrl (ref TempUrl))
			return true;

		if (ResolvePersistentDataUrl (ref TempUrl))
			return true;

		return false;
	}

	static public string		ReolveUrl(string Url)
	{
		//	turn local urls into file urls
		if (ResolveFileUrl (ref Url))
			return Url;

		if (ResolveSdcardUrl (ref Url))
			return Url;

		if (ResolveStreamingAssetsUrl (ref Url))
			return Url;

		if (ResolvePersistentDataUrl (ref Url))
			return Url;

		//	assuming http, clean up for WWW (which doesn't escape spaces so sends malformed HTTP1.x headers
		return PopUrl.EscapeUrl (Url);
	}

	static public string		GetRelativeUrl(string BaseUrl,string Filename)
	{
		var EndIndex = BaseUrl.LastIndexOfAny( new char[] { '\\','/' }	);
		if (EndIndex >= 0) {
			BaseUrl = BaseUrl.Substring (0, EndIndex);
		}

#if !UNITY_WSA
		//	file:// on windows needs \\ slashes
		if (BaseUrl.ToLower().StartsWith("file:") )
		{
			BaseUrl += System.IO.Path.DirectorySeparatorChar + Filename;
		}
		else
		{
			//	urls need forward slashes
			BaseUrl += "/" + Filename;
		}
#endif
		return BaseUrl;
	}
}
