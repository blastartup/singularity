using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	/// <summary>
	/// A faster parallel processor then the inbuilt one (Parallel.ForEach())
	/// </summary>
	/// <typeparam name="T">Any type to pass into the action to process in parallel.</typeparam>
	[DebuggerStepThrough]
	public class FastParallel<T>
	{
		private readonly SlicedList<T>[] _listSlices;
		private readonly Int32 _numberOfThreads;
		private readonly Action<T> _action;
		private readonly ManualResetEvent[] _manualResetEvents;

		[DebuggerStepThrough]
		public FastParallel(Int32 numberOfThreads, Action<T> action)
		{
			this._numberOfThreads = numberOfThreads;
			this._listSlices = new SlicedList<T>[_numberOfThreads];
			this._action = action;
			this._manualResetEvents = new ManualResetEvent[_numberOfThreads];

			for (var i = 0; i < _numberOfThreads; i++)
			{
				_listSlices[i] = new SlicedList<T>();
				_manualResetEvents[i] = new ManualResetEvent(false);
				_listSlices[i].Indexes = new LinkedList<Int32>();
				_listSlices[i].ManualResetEvent = _manualResetEvents[i];
			}
		}

		[DebuggerStepThrough]
		public void ForEach(IList<T> items)
		{
			if (items == null)
			{
				throw new ArgumentNullException(nameof(items));
			}

			PrepareListSlices(items);
			for (var i = 0; i < _numberOfThreads; i++)
			{
				_manualResetEvents[i].Reset();
				ThreadPool.QueueUserWorkItem(new WaitCallback(DoWork), _listSlices[i]);
			}
			WaitHandle.WaitAll(_manualResetEvents);
		}

		private void PrepareListSlices(IList<T> items)
		{
			for (var i = 0; i < _numberOfThreads; i++)
			{
				_listSlices[i].Items = items;
				_listSlices[i].Indexes.Clear();
			}
			for (var i = 0; i < items.Count; i++)
			{
				_listSlices[i % _numberOfThreads].Indexes.AddLast(i);
			}
		}

		private void DoWork(Object o)
		{
			var slicedList = (SlicedList<T>)o;

			foreach (var i in slicedList.Indexes)
			{
				_action(slicedList.Items[i]);
			}
			slicedList.ManualResetEvent.Set();
		}
	}

	public class SlicedList<T>
	{
		public IList<T> Items { get; set; }
		public LinkedList<Int32> Indexes { get; set; }
		public ManualResetEvent ManualResetEvent { get; set; }
	}
}
