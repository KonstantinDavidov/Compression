using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace GZipCompressionEx
{
	public class GZipDecompressor : GZipTemplateMethod
	{
		private static readonly ReaderWriterLockSlim Rwl = new ReaderWriterLockSlim();

		public GZipDecompressor(string pathToInputFile, string pathToOutputFile, Action<double> reportProgressPercentAction) 
			: base(pathToInputFile, pathToOutputFile, reportProgressPercentAction)
		{
		}

		protected override void Read()
		{
			using (var fs = new FileStream(PathToInputFile, FileMode.Open, FileAccess.Read))
			{
				while (fs.Position < fs.Length)
				{
					var lengthBuffer = new byte[8];
					fs.Read(lengthBuffer, 0, lengthBuffer.Length);
					var blockLength = BitConverter.ToInt32(lengthBuffer, 4);
					var compressedData = new byte[blockLength];
					lengthBuffer.CopyTo(compressedData, 0);

					fs.Read(compressedData, lengthBuffer.Length, blockLength - lengthBuffer.Length);
					var dataSize = BitConverter.ToInt32(compressedData, blockLength - 4);
					CompressQueue.EnqueueItem(() => DecompressFile(compressedData, dataSize));
				}
			}
		}

		protected override void Write(byte[] chunk)
		{
			Rwl.EnterWriteLock();
			try
			{
				using (var file = new FileStream(PathToOutputFile, FileMode.Append))
				{
					file.Write(chunk, 0, chunk.Length);
					ProgressTick();
				}
			}
			finally
			{
				Rwl.ExitWriteLock();
			}
		}
		
		private void DecompressFile(byte[] compressedChunk, long decompressedChunkLength)
		{
			var result = new byte[decompressedChunkLength];
			using (var ms = new MemoryStream(compressedChunk))
			{
				using (var gz = new GZipStream(ms, CompressionMode.Decompress))
					gz.Read(result, 0, result.Length);

				WriteQueue.EnqueueItem(() => Write(result.ToArray()));
			}
		}
	}
}
