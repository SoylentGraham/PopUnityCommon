using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace PopX
{
	public static class Ogg
	{
		static readonly char[] OggPagePrefix = "OggS".ToCharArray ();

		//	throws on EOF
		public static int	FindNextOggPage (List<Byte> Data, int Start)
		{
			return PopX.Data.FindPattern (Data, OggPagePrefix, Start);
		}


	};

	//	https://en.wikipedia.org/wiki/Ogg#Page_structure
	class OggPage
	{
		byte[]				Data;
		public byte			Prefix0	{	get{	return Data[0];	}}
		public byte			Prefix1	{	get{	return Data[1];	}}
		public byte			Prefix2	{	get{	return Data[2];	}}
		public byte			Prefix3	{	get{	return Data[3];	}}
		public byte			Version	{	get{	return Data[4];	}}
		public byte			HeaderType		{	get{	return Data[5];	}}
		public bool			IsContinuation	{	get{	return (HeaderType & (1<<0))!=0;	}}
		public bool			IsBeginningOfStream	{	get{	return (HeaderType & (1<<1))!=0;	}}
		public bool			IsEndOfStream		{	get{	return (HeaderType & (1<<2))!=0;	}}
		long				GranulePosition	{	get { return BytesToInt64 (6); } }
		public long			Timecode	{	get { return GranulePosition; } }
		public int			BitstreamSerial	{	get { return BytesToInt32 (14); } }
		public int			PageSequence	{	get { return BytesToInt32 (18); } }
		public int			Checksum		{	get { return BytesToInt32 (22); } }	//	includes page header
		public byte			PageSegments	{	get{ return Data[26];	}}
		int					PacketStartIndex	{	get { return 27 + PageSegments; } }

		public OggPage(List<byte> _Data,int Size)
		{
			Data = new byte[Size];
			for (int i = 0;	i < Size;	i++)
				Data [i] = _Data [i];

			if (Version != 0)
				throw new System.Exception ("OggPage version not zero (" + Version + ")");
		}



		public void CopyPacketBytes(List<byte> Packet,out bool IsFinished)
		{
			var SegmentSizes = GetSegmentSizes ();

			var PacketSize = 0;
			foreach (var SegmentSize in SegmentSizes)
				PacketSize += SegmentSize;
				
			var LastSize = SegmentSizes [SegmentSizes.Length - 1];
			IsFinished = (LastSize != 255) && ( LastSize != 0 );

			for (int i = 0;	i < PacketSize;	i++)
				Packet.Add (Data [i + PacketStartIndex]);

			//	extra data in packet...
			var DataPacketSize = Data.Length - PacketStartIndex;
			if (DataPacketSize != PacketSize)
				throw new System.Exception ("Packet data misalignment... " + DataPacketSize + " vs header " + PacketSize);

			/*
			int PacketDataIndex = PacketStartIndex;
			foreach (var SegmentSize in SegmentSizes) {

				try
				{
					//	copy bytes
					for (int i = 0;	i < SegmentSize;	i++)
						Packet.Add (Data [PacketDataIndex + i]);
				}
				catch(System.Exception e) {
					throw new System.Exception ("Not enough data in packet",e);
				}

				//	move along
				PacketDataIndex += SegmentSize;

				//	last one, but size is mulitple of 255 so needs an endofpacket
				//if (SegmentSize == 0)
				//	break;
				//	last in sequence if not 255
				if (SegmentSize < 255)
					break;
			}

			//	extra data in packet...
			var LeftoverData = Data.Length - PacketDataIndex;
			if (LeftoverData > 0)
				throw new System.Exception ("Packet misalignment... " + LeftoverData + " bytes remaining");
				*/
		}

		int[]		GetSegmentSizes()
		{
			var SegmentSizes = new int[PageSegments];
			for (int i = 0;	i < SegmentSizes.Length;	i++)
				SegmentSizes [i] = Data [27 + i];
			return SegmentSizes;
		}


		long				BytesToInt64(int FirstByte)
		{
			int x = 0;
			x |= Data [FirstByte + 0] << 0;
			x |= Data [FirstByte + 1] << 8;
			x |= Data [FirstByte + 2] << 16;
			x |= Data [FirstByte + 3] << 24;
			x |= Data [FirstByte + 4] << 32;
			x |= Data [FirstByte + 5] << 40;
			x |= Data [FirstByte + 6] << 48;
			x |= Data [FirstByte + 7] << 56;
			return x;
		}

		int					BytesToInt32(int FirstByte)
		{
			int x = 0;
			x |= Data [FirstByte + 0] << 0;
			x |= Data [FirstByte + 1] << 8;
			x |= Data [FirstByte + 2] << 16;
			x |= Data [FirstByte + 3] << 24;
			return x;
		}

	};

	public class OggDecoder
	{
		List<OggPage>		Pages = new List<OggPage>();

		//	handle split pages
		List<Byte>			PendingBytes = new List<Byte>();

		public void			PushBytes(byte[] Data)
		{
			PendingBytes.AddRange (Data);

			while (true) {
				if (!PopPendingPage ())
					break;
			}
		}

		bool PopPendingPage()
		{
			if (PendingBytes.Count == 0)
				return false;
			
			int Length = 0;
			try 
			{
				Length = Ogg.FindNextOggPage (PendingBytes,1);
			}
			catch(Data.NotFound) {
				Length = PendingBytes.Count;
			}

			var Page = new OggPage(PendingBytes,Length);
			PendingBytes.RemoveRange(0,Length);
			Pages.Add( Page );
			return true;
		}

		public List<byte> PopPacket(out long Timecode)
		{
			var PoppedPages = new List<OggPage> ();

			//	pages may be interleaved, so only concatonate matching serials
			int? StreamSerialNumber = null;
			Timecode = 0;
			int NextPageIndex = 0;
			var PacketBytes = new List<Byte> ();

			System.Action Finish = () => {
				foreach( var Page in PoppedPages )
					Pages.Remove( Page );
			};

			while ( NextPageIndex < Pages.Count ) {
				var NextPage = Pages [NextPageIndex];
				if (!StreamSerialNumber.HasValue) {
					StreamSerialNumber = NextPage.BitstreamSerial;
					Timecode = NextPage.Timecode;
				}

				//	gr: should timecode also always match?
				if (NextPage.BitstreamSerial != StreamSerialNumber) {
					NextPageIndex++;
					continue;
				}
				if (NextPage.Timecode != Timecode) {
					Debug.LogWarning ("Next page timecode(" + NextPage.Timecode + ") different to first page in sequence's timecode(" + Timecode + ")");
				}

				bool Finished;
				NextPage.CopyPacketBytes (PacketBytes, out Finished);
				PoppedPages.Add (NextPage);
				NextPageIndex++;

				if ( Finished )
				{
					Finish.Invoke ();
					return PacketBytes;
				}
			}

			//	waiting for rest of packet
			return null;
		}
	}

}
