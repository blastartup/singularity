using System;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	public abstract class RetryStrategry
	{
		public RetryStrategry() { }

		public DateTime Started
		{
			get { return started; }
			set { started = value; }
		}
		protected DateTime started;

		public Int32 Attempts
		{
			get { return attempts; }
			set { attempts = value; }
		}
		protected Int32 attempts;

		public Int32 Delay
		{
			get { return delay; }
			set { delay = value; }
		}
		protected Int32 delay;

		public abstract Boolean CanContinue();
		public abstract Boolean ShouldWait();

		protected Int32 TimeoutInSeconds, Frequency, MaxRetries;
	}
}
