using System;
using System.IO;
using System.Threading;

namespace GZipCompressionEx
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
			var fileInfo = new FileInfo(pathToInput);
			PathToOutputFile = pathToOutput;
			_originalProgressChunks = fileInfo.Length / BufferSize5Mb;
			_currentProgressChunks = _originalProgressChunks;
			CompressQueue = new ProducerConsumerQueue("CompressQueue", WorkerCount);
			WriteQueue = new ProducerConsumerQueue("WriteQueue", 1);
			_reportProgressPercentAction = reportProgressPercentAction;
		}

		public void Start()
		{
			var reader = new Thread(Read);
			reader.Start();
			reader.Join();

			CompressQueue.Shutdown(true);
			WriteQueue.Shutdown(true);
		}

		protected void ProgressTick()
		{
			_currentProgressChunks--;
			var diff = (double)_currentProgressChunks / _originalProgressChunks;
			var percentValue = 100 - diff * 100;

			_reportProgressPercentAction(percentValue < 100 ? percentValue : 100);
		}

		protected abstract void Read();
		protected abstract void Write(byte[] chunk);
	}
}
