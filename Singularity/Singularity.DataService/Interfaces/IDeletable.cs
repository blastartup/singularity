using System;

// ReSharper disable once CheckNamespace

namespace Singularity.DataService
{
	public interface IDeletable
	{
		Boolean IsDeleted { get; set; }
	}
}
