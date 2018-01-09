using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace PopX
{
	public static class Vorbis
	{
		static public readonly char[] HeaderPrefix = "vorbis".ToCharArray ();
	};


	public class VorbisDecoder
	{
		csvorbis.Info		InfoHeader;
		csvorbis.Comment	CommentHeader;
		bool	HasInitialHeader	{	get{	return InfoHeader!=null;}}
		bool	HasCommentHeader	{	get{	return CommentHeader!=null;}}
		bool	HasCodeBookHeader = false;
		bool	HasAllHeaders	{	get{ return HasInitialHeader && HasCommentHeader && HasCodeBookHeader; }}

		List<Byte>			PendingBytes = new List<Byte>();

		public void			PushBytes(byte[] Data)
		{
			PendingBytes.AddRange (Data);

			while (true) {
				if (!PopPendingData ())
					break;
			}
		}

		void ReadInfoHeader()
		{
			var opb = new csogg.csBuffer ();
			opb.readinit (PendingBytes.ToArray (), 0, PendingBytes.Count);

			InfoHeader = new csvorbis.Info ();
			if (InfoHeader.unpack_info (opb) != 0)
				throw new System.Exception ("Failed to unpack info header");

			//	eat the data opb ate
			PendingBytes.RemoveRange(0, opb.ptr+1 );
		}

		void ReadCommentsHeader()
		{
			var opb = new csogg.csBuffer ();
			opb.readinit (PendingBytes.ToArray (), 0, PendingBytes.Count);
			CommentHeader = new csvorbis.Comment ();
			if (CommentHeader.unpack (opb) != 0)
				throw new System.Exception ("failed to unpack comment header");

			//	eat the data opb ate
			PendingBytes.RemoveRange(0, opb.ptr+1 );
		}

		void ReadCodeBooksHeader()
		{
			var opb = new csogg.csBuffer ();
			opb.readinit (PendingBytes.ToArray (), 0, PendingBytes.Count);

			if (InfoHeader.unpack_books (opb) != 0)
				throw new System.Exception ("Failed to unpack books header");

			//	eat the data opb ate
			PendingBytes.RemoveRange(0, opb.ptr+1 );
		}

		void PopHeader ()
		{
			//	read type byte
			var PacketType = PendingBytes[0];
			var MarkerPosition = PopX.Data.FindPattern (PendingBytes, Vorbis.HeaderPrefix, 0);
			if (MarkerPosition != 1)
				throw new System.Exception ("Expecting vorbis header at [1], not [" + MarkerPosition + "]");

			//	eat those header bytes
			PendingBytes.RemoveRange( 0, 1 + Vorbis.HeaderPrefix.Length );

			if (PacketType == 0x01) {
				if (HasInitialHeader != false)
					throw new System.Exception ("Already processed initial header");
				ReadInfoHeader ();
				return;
			}

			if (PacketType == 0x03) {
				if (HasCommentHeader != false)
					throw new System.Exception ("Already processed comments header");
				ReadCommentsHeader ();
				return;
			}

			if (PacketType == 0x05) {
				if (HasCodeBookHeader != false)
					throw new System.Exception ("Already processed comments header");
				ReadCodeBooksHeader ();
				HasCodeBookHeader = true;
				return;
			}

			throw new System.Exception ("Unknown vorbis header type " + PacketType);
		}

		bool PopPendingData()
		{
			if (PendingBytes.Count == 0)
				return false;

			//	read headers
			if (!HasAllHeaders) {
				PopHeader ();
				return true;
			}

			Debug.Log ("Read non header vorbis data");
			return false;
		}

	}

}
