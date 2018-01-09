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
		bool				HasInitialHeader	{	get{	return InfoHeader!=null;}}
		bool				HasCommentHeader	{	get{	return CommentHeader!=null;}}
		bool				HasCodeBookHeader = false;
		public bool			HasAllHeaders	{	get{ return HasInitialHeader && HasCommentHeader && HasCodeBookHeader; }}

		public string		EncoderVendor	{	get { return CommentHeader!=null ? CommentHeader.getVendor () : null; } }
		public string[]		EncoderComments
		{
			get {
				var Comments = new string[ CommentHeader == null ? 0 : CommentHeader.comments ];
				for (int i = 0;	i < Comments.Length;	i++)
					Comments [i] = CommentHeader.getComment (i);
				return Comments;
			}
		}
		public int			SampleRate		{	get { return InfoHeader!=null ? InfoHeader.rate : 0; } }
		public int			ChannelCount	{	get { return InfoHeader!=null ? InfoHeader.channels : 0; } }

		List<Byte>			PendingBytes = new List<Byte>();

		csvorbis.DspState	Dsp;
		csvorbis.Block		DspWorkingBlock;


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

			//	finished headers, setup DSP
			if (Dsp == null) {
				Dsp = new csvorbis.DspState ();
				Dsp.synthesis_init (InfoHeader);
				DspWorkingBlock.init (Dsp);
			}

			DecodeDsp ();


			Debug.Log ("Read non header vorbis data");
			return false;
		}


		void DecodeDsp()
		{
			var vi = InfoHeader;
		
			// so multiple block decodes can
			// proceed in parallel.  We could init
			// multiple vorbis_block structures
			// for vd here

			var vb = DspWorkingBlock;
			var vd = Dsp;

			//	op = packet
			// we have a packet.  Decode it
			if (vb.synthesis(op) == 0)
			{
				// test for success!
				vd.synthesis_blockin(vb);
			}

			// **pcm is a multichannel float vector.  In stereo, for
			// example, pcm[0] is left, and pcm[1] is right.  samples is
			// the size of each channel.  Convert the float values
			// (-1.<=range<=1.) to whatever PCM format and write it out

			while (true)
			{
				float[][][] _pcm = new float[1][][];
				int[] _index = new int[vi.channels];

				var samples = vd.synthesis_pcmout(_pcm, _index);
				if ( samples <= 0 )
					break;
				
				float[][] pcm = _pcm[0];
				bool clipflag = false;
				var convsize = 4096 / vi.channels;
				int bout = (samples < convsize ? samples : convsize);

				// convert floats to 16 bit signed ints (host order) and
				// interleave
				for ( var i = 0; i < vi.channels; i++)
				{
					int ptr = i * 2;
					//int ptr=i;
					int mono = _index[i];
					for (int j = 0; j < bout; j++)
					{
						var Sample = pcm [i] [mono + j];
						var ChannelIndex = i;
						OnReadSample (Sample,ChannelIndex);
						//ptr += 2 * (vi.channels);
					}
				}

				vd.synthesis_read(bout); // tell libvorbis how
				// many samples we
				// actually consumed
			}
		}
	}
	if (og.eos() != 0) eos = 1;
}
				}
			}
	}

}
