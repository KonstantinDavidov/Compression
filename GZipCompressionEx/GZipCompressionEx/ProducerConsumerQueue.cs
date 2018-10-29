using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
	public class ProducerConsumerQueue
	{
		private readonly object _locker = new object();
		private readonly Thread[] _workers;
		private readonly Queue<Action> _itemQ = new Queue<Action>();

		public ProducerConsumerQueue(string name, int workerCount)
		{
			_workers = new Thread[workerCount];
			
			for (var i = 0; i < workerCount; i++)
			{
				_workers[i] = new Thread(Consume) { Name = name + i };
				_workers[i].Start();
			}
		}

		public void Shutdown(bool waitForWorkers)
		{
			for (var i = 0; i < _workers.Length; i++)
				EnqueueItem(null);

			if (waitForWorkers)
			{
				foreach (var worker in _workers)
					worker.Join();
			}
		}

		public void EnqueueItem(Action item)
		{
			lock (_locker)
			{
				_itemQ.Enqueue(item);
				Monitor.Pulse(_locker);
			}
		}

		private void Consume()
		{
			while (true)                        
			{                                  
				Action item;
				lock (_locker)
				{
					while (_itemQ.Count == 0) Monitor.Wait(_locker);
					item = _itemQ.Dequeue();
				}
				if (item == null) return;
				item();
			}
		}
	}
}
