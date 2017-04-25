using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.WinForm.Async
{
	// https://www.codeproject.com/Tips/1168027/Helper-Class-for-Calling-Asynchronous-Methods-usin
	public class AsyncActor<TSender, TResult>
	{
		// Optional delegate to pass back the result...
		public delegate void After(Object sender, TResult result);

		public AsyncActor(Func<TSender, TResult> job)
		{
			_job = job;
		}

		public virtual async Task Act(TSender sender, After after)
		{
			TResult result = await Task.Run(() => _job(sender));

			after?.Invoke(this, result);
		}

		public virtual async Task<TResult> Act(TSender sender)
		{
			return await Task.Run(() => _job(sender));
		}

		private readonly Func<TSender, TResult> _job;
	}
}
