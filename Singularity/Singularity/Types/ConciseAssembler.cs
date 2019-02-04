using System;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	public abstract class ConciseAssembler
	{
	}

	[CLSCompliant(true)]
	public abstract class ConciseAssembler<TIn, TOut> : ConciseAssembler
		where TIn : class
		where TOut : class, new()
	{
		protected TOut AssembleClass(TIn input)
		{
			if (input == null)
			{
				return null;
			}

			TOut output = NewOutput();
			Populate(output, input);
			return output;
		}
		protected virtual TOut NewOutput()
		{
			return new TOut();
		}

		protected abstract void Populate(TOut output, TIn input);
	}
}
