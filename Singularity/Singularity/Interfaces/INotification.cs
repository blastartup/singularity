using System;

namespace Singularity
{
	public interface INotification
	{
		String Message { get; set; }
		Exception Exception { get; set; }
	}
}
