using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace PopX
{
	public static class Cli
	{
		//	returns the process exit code
		public static int Run(string ExeFilename, string Arguments, System.Action<byte[]> OnStdOut, System.Action<byte[]> OnStdErr = null, System.Func<bool> Continue = null)
		{
			//	fill in the auto-continue func if none provided
			if (Continue == null)
				Continue = () => { return true; };

			var info = new System.Diagnostics.ProcessStartInfo(ExeFilename, Arguments);
			info.UseShellExecute = false;
			info.CreateNoWindow = false;

			info.RedirectStandardInput = false;
			info.RedirectStandardOutput = OnStdOut != null;
			info.RedirectStandardError = OnStdErr != null;

			var SubProcess = System.Diagnostics.Process.Start(info);

			System.Action<BinaryReader, System.Action<byte[]>> ReadAllOfStream = (BinaryStream, OnRead) =>
			{
				if (BinaryStream == null)
					return;
				if (OnRead == null)
					return;

				var Buffer = new byte[1024 * 10];

				//	allow external abort
				while (Continue())
				{
					var BytesRead = BinaryStream.Read(Buffer, 0, Buffer.Length);
					if (BytesRead == 0)
						break;

					if (BytesRead < Buffer.Length)
					{
						var SmallBuffer = new byte[BytesRead];
						Array.Copy(Buffer, SmallBuffer, SmallBuffer.Length);
					}
					else
					{
						OnRead(Buffer);
					}
				}
			};


			var StdoutReader = OnStdOut != null ? new BinaryReader(SubProcess.StandardOutput.BaseStream) : null;
			var StderrReader = OnStdErr != null ? new BinaryReader(SubProcess.StandardError.BaseStream) : null;


			//	run until done
			try
			{
				while (!SubProcess.HasExited)
				{
					if (!Continue())
						throw new System.Exception("Aborted");

					ReadAllOfStream(StdoutReader, OnStdOut);
					ReadAllOfStream(StderrReader, OnStdErr);

					System.Threading.Thread.Sleep(1);
				}
			}
			catch (System.Exception e)
			{
				UnityEngine.Debug.LogException(e);
				SubProcess.Kill();
				throw;
			}

			var ExitCode = SubProcess.ExitCode;
			SubProcess.Close();
			SubProcess.Dispose();
			return ExitCode;
		}


		public static bool HasArgument(string Argument)
		{
			var Args = System.Environment.GetCommandLineArgs();
			if (System.Array.IndexOf(Args, Argument) == -1)
				return false;
			return true;
		}
	}
}
