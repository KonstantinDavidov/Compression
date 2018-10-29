using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipCompressionEx
{
	public class GZipCompressor : GZipTemplateMethod
	{
		private static readonly ReaderWriterLockSlim Rwl = new ReaderWriterLockSlim();

		public GZipCompressor(string pathToInputFile, string pathToOutputFile, Action<double> reportProgressPercentAction) 
			: base(pathToInputFile, pathToOutputFile, reportProgressPercentAction)
		{
		}
		
		protected override void Read()
		{
			using (var file = new FileStream(PathToInputFile, FileMode.Open, FileAccess.Read))
			{
				while (file.Position < file.Length)
				{
					int bytesRead;
					if (file.Length - file.Position <= BufferSizeMb)
					{
						bytesRead = (int)(file.Length - file.Position);
					}
					else
					{
						bytesRead = BufferSizeMb;
					}

					var chunk = new byte[bytesRead];
					file.Read(chunk, 0, bytesRead);

					CompressQueue.EnqueueItem(() => CompressFile(chunk));
				}
			}
		}

		protected override void Write(byte[] chunk)
		{
			Rwl.EnterWriteLock();
			try
			{
				using (var file = new FileStream(PathToOutputFile + ".gz", FileMode.Append))
				{
					BitConverter.GetBytes(chunk.Length).CopyTo(chunk, 4);
					file.Write(chunk, 0, chunk.Length);
					ProgressTick();
				}
			}
			finally
			{
				Rwl.ExitWriteLock();
			}
		}

		private void CompressFile(byte[] chunk)
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var cs = new GZipStream(memoryStream, CompressionMode.Compress))
					cs.Write(chunk, 0, chunk.Length);

				var compressedData = memoryStream.ToArray();
				WriteQueue.EnqueueItem(() => Write(compressedData));
			}
		}
	}
}
