using System;
using System.IO;
using System.Threading;

namespace GZipTest.GZipCore
{
	public abstract class GZipTemplateMethod
	{
		protected string PathToInputFile { get; private set; }
		protected string PathToOutputFile { get; private set; }

		protected virtual int BufferSizeMb { get { return BufferSize5Mb; } }
		
		protected ProducerConsumerQueue CompressQueue { get; private set; }
		protected ProducerConsumerQueue WriteQueue { get; private set; }

		private const int BufferSize5Mb = 5 * 1024 * 1024;
		private const int WorkerCount = 5;

		private readonly long _originalProgressChunks;
		private long _currentProgressChunks;

		private readonly Action<double> _reportProgressPercentAction;

		protected GZipTemplateMethod(string pathToInput, string pathToOutput, Action<double> reportProgressPercentAction)
		{
			PathToInputFile = pathToInput;
			PathToOutputFile = pathToOutput;
			var fileInfo = new FileInfo(pathToInput);
			_originalProgressChunks = fileInfo.Length / BufferSize5Mb;
			_currentProgressChunks = _originalProgressChunks;
			CompressQueue = new ProducerConsumerQueue("CompressQueue", WorkerCount);
			WriteQueue = new ProducerConsumerQueue("WriteQueue", 1);
			_reportProgressPercentAction = reportProgressPercentAction;
		}

		/// <summary>
		/// Starts compress/decompress operation
		/// </summary>
		public void Start()
		{
			var reader = new Thread(Read);
			reader.Start();
			reader.Join();

			CompressQueue.Shutdown(true);
			WriteQueue.Shutdown(true);
		}

		/// <summary>
		/// Calculates a progress of compressing/decompression operation. Invokes delegate to report progress of operation. 
		/// </summary>
		protected void ProgressTick()
		{
			_currentProgressChunks--;
			var diff = (double)_currentProgressChunks / _originalProgressChunks;
			var percentValue = 100 - diff * 100;

			if (_reportProgressPercentAction != null)
				_reportProgressPercentAction(percentValue < 100 ? percentValue : 100);
		}

		/// <summary>
		/// Reads an input file.
		/// </summary>
		protected abstract void Read();

		/// <summary>
		/// Writes compressed/decompressed portion of information to file.
		/// </summary>
		/// <param name="chunk"></param>
		protected abstract void Write(byte[] chunk);
	}
}
