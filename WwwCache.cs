using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class WwwCache
{
	static string GetCachePath()
	{
		//	gr: \ actually doesn't work on windows for file: urls ... so can't use the proper seperator
		var Seperator = '/';
		//var Seperator = System.IO.Path.DirectorySeparatorChar;
		return Application.persistentDataPath + Seperator;
	}

	static string strToBase64(string str) {
		byte[] byt = System.Text.Encoding.UTF8.GetBytes(str);
		return Convert.ToBase64String(byt);
	}

	static string base64ToStr(string base64) {
		byte[] b = Convert.FromBase64String(base64);
#if UNITY_WSA
		return System.Text.Encoding.UTF8.GetString(b,0,b.Length);
#else
		return System.Text.Encoding.UTF8.GetString(b);
#endif
	}

	static string urlToCachePath(string url) {
		return GetCachePath() + strToBase64(url);
	}

	public static  bool HasCache(string url) {
		return File.Exists(urlToCachePath(url));
	}

	public static  void WriteCache(WWW www) 
	{
		//	skip if url in cached path
		var CachePrefix = PopUrl.MakeFileUrl( GetCachePath() );
		if (www.url.StartsWith ( CachePrefix ) )
			return;
		
		try
		{			
			var CachePath = urlToCachePath (www.url);
			File.WriteAllBytes( CachePath, www.bytes);
			Debug.Log ("Wrote " + www.url + " cache to " + CachePath);
		}
		catch(System.Exception e)
		{
			Debug.LogError ("Failed to write cache (" + www.url + "); " + e.Message);
		}
	}

	//	returns cached url if the file is there
	public static string GetCachedUrl(string url) 
	{
		//	if url is local, no cache
		if (PopUrl.IsLocalUrl (url))
			return url;
		
		string path = urlToCachePath(url);
		if ( !File.Exists(path) ) 
		{
			//Debug.LogWarning("dont have cache url:"+url);
			return url;
		}

		return PopUrl.MakeFileUrl (path);
	}
}

