using System;
using System.Threading;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	/// <summary>
	/// A Timeout or Counted Retry class.
	/// </summary>
	public abstract class Retry : ICommand
	{
		protected Retry(RetryStrategry retryStrategry) 
		{
			RetryStrategry = retryStrategry;
		}

		public IReply Execute()
		{
			var result = new ReplySimple(true);
			RetryStrategry.Started = DateTime.UtcNow;
			while (CanContinue())
			{
				if ((result = ExecuteAction()).Condition)
				{
					break;
				}
				RetryStrategry.Attempts++;
				if (ShouldWait())
				{
					ExecuteDelay();
				}
			}
			return result;
		}

		private Boolean CanContinue()
		{
			return RetryStrategry.CanContinue();
		}

		private Boolean ShouldWait()
		{
			return RetryStrategry.ShouldWait();
		}

		protected abstract ReplySimple ExecuteAction();  // AKA Do something...

		protected virtual void ExecuteDelay()
		{
			Thread.Sleep(RetryStrategry.Delay);
		}

		protected readonly RetryStrategry RetryStrategry;
	}
}
