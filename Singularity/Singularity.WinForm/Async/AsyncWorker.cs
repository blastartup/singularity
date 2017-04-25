﻿using System;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;

namespace Singularity.WinForm.Async
{
	// https://www.codeproject.com/Articles/42912/A-Simple-Way-To-Use-Asynchronous-Call-in-Your-Mult
	/// <summary>
	/// AsyncWorker does a bettor job than BackgroundWorker for running asynchronous tasks. 
	/// </summary>
	public class AsyncWorker
	{
		/// <summary>
		/// Occurs when [do work].
		/// </summary>
		public event DoWorkEventHandler DoWork;
		/// <summary>
		/// Occurs when [run worker completed].
		/// </summary>
		public event RunWorkerCompletedEventHandler RunWorkerCompleted;
		/// <summary>
		/// Occurs when [progress changed].
		/// </summary>
		public event ProgressChangedEventHandler ProgressChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="AsyncWorker"/> class.
		/// </summary>
		public AsyncWorker(Int32 maximumCount)
		{
			_maxCount = maximumCount;
			_workerCallback = new AsyncCallback(OnRunWorkerCompleted);
			_eventHandler = new DoWorkEventHandler(OnDoWork);
		}

		/// <summary>
		/// Gets a value indicating whether this instance is busy.
		/// </summary>
		/// <value><c>true</c> if this instance is busy; otherwise, <c>false</c>.</value>
		public Boolean IsBusy
		{
			get
			{
				lock (CountProtector)
				{
					return (_count >= _maxCount);
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether [cancellation pending].
		/// </summary>
		/// <value><c>true</c> if [cancellation pending]; otherwise, <c>false</c>.</value>
		public Boolean CancellationPending => _cancelationPending;

		/// <summary>
		/// Runs the worker async.
		/// </summary>
		/// <param name="abortIfBusy">if set to <c>true</c> [abort if busy].</param>
		public Boolean RunWorkerAsync(Boolean abortIfBusy)
		{
			return RunWorkerAsync(abortIfBusy, null);
		}


		/// <summary>
		/// Runs the worker async.
		/// </summary>
		/// <param name="abortIfBusy">if set to <c>true</c> [abort if busy].</param>
		/// <param name="argument">The argument.</param>
		public Boolean RunWorkerAsync(Boolean abortIfBusy, Object argument)
		{
			if (abortIfBusy && IsBusy)
			{
				return false;
			}
			_count++;

			_eventHandler.BeginInvoke(this, new DoWorkEventArgs(argument), _workerCallback, _eventHandler);
			return true;
		}

		/// <summary>
		/// Cancels the async.
		/// </summary>
		public void CancelAsync()
		{
			_cancelationPending = true;
		}

		/// <summary>
		/// Reports the progress.
		/// </summary>
		/// <param name="percentProgress">The percent progress.</param>
		public void ReportProgress(Int32 percentProgress)
		{
			OnProgressChanged(new ProgressChangedEventArgs(percentProgress, null));
		}
		/// <summary>
		/// Reports the progress.
		/// </summary>
		/// <param name="percentProgress">The percent progress.</param>
		/// <param name="userState">State of the user.</param>
		public void ReportProgress(Int32 percentProgress, Object userState)
		{
			OnProgressChanged(new ProgressChangedEventArgs(percentProgress, userState));
		}

		/// <summary>
		/// Called when [do work].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.ComponentModel.DoWorkEventArgs"/> instance containing the event data.</param>
		protected virtual void OnDoWork(Object sender, DoWorkEventArgs e)
		{
			if (e.Cancel)
			{
				return;
			}
			Console.WriteLine("Async started " + DateTime.UtcNow.ToString());
			DoWork?.Invoke(this, e);
		}

		/// <summary>
		/// Raises the <see cref="E:ProgressChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.ComponentModel.ProgressChangedEventArgs"/> instance containing the event data.</param>
		protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
		{
			ProgressChanged?.Invoke(this, e);
		}

		/// <summary>
		/// Called when [run worker completed].
		/// </summary>
		/// <param name="ar">The ar.</param>
		protected virtual void OnRunWorkerCompleted(IAsyncResult ar)
		{
			DoWorkEventHandler doWorkDelegate = (DoWorkEventHandler)((AsyncResult)ar).AsyncDelegate;
			RunWorkerCompleted?.Invoke(this, new RunWorkerCompletedEventArgs(ar, null, _cancelationPending));

			Console.WriteLine("Async ended " + DateTime.UtcNow.ToString());
			_count--;
		}

		private Boolean _cancelationPending;
		private static readonly Object CountProtector = new Object();
		private Int32 _count;
		private readonly Int32 _maxCount;
		private readonly AsyncCallback _workerCallback;
		private readonly DoWorkEventHandler _eventHandler;
	}
}
