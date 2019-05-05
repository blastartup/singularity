using System;

// ReSharper disable once CheckNamespace

namespace Singularity.Web.Types
{
	public abstract class ConciseAssembler
	{
	}

	[CLSCompliant(true)]
	public abstract class ConciseAssembler<TIn, TOut> : ConciseAssembler
		where TIn : class
		where TOut : class, new()
	{
		protected TOut Assemble(TIn input)
		{
			if (input == null)
			{
				return null;
			}

			TOut output = new TOut();
			Populate(output, input);
			return output;
		}

		protected abstract void Populate(TOut output, TIn input);
	}
}
