using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

//	gr: PopX being renamed to Pop later
namespace PopX
{
	public static class Net
	{
		//	when we open sockets with Address.Any, our address is still usually 0.0.0.0
		//	no good for outsiders. 
		//	https://stackoverflow.com/a/27376368/355753
		public static IPAddress GetLocalAddress()
		{
			using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
			{
				socket.Connect("8.8.8.8", 65530);
				var endPoint = socket.LocalEndPoint as IPEndPoint;
				var LocalAddress = endPoint.Address;
				return LocalAddress;
			}
		}

	}
}

