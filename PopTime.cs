using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PopX
{
	public static class Time
	{
		//	this is okay as an int as 
		//	max in a day will be	86400000
		//	max int32				2147483647
		//	but YOU should probably not be storing timecodes in an int. 
		//	being utc, maybe this can be negative?
		public static int GetTodayUtcTimeMs()
		{
			var TimeMs = 0;
			var MsMs = 1;
			var SecondMs = 1000;
			var MinuteMs = SecondMs * 60;
			var HourMs = MinuteMs * 60;
			var Now = System.DateTime.UtcNow;
			TimeMs += Now.Hour * HourMs;
			TimeMs += Now.Minute * MinuteMs;
			TimeMs += Now.Second * SecondMs;
			TimeMs += Now.Millisecond * MsMs;
			return TimeMs;
		}
	};
}
