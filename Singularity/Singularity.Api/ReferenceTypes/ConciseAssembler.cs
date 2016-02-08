﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace

namespace Singularity.Api
{
	public abstract class ConciseAssembler
	{
	}

	public abstract class ConciseAssembler<TIn, TOut> : ConciseAssembler
		where TIn : class
		where TOut : class, new()
	{
		public TOut Assemble(TIn input)
		{
			if (input == null)
			{
				return null;
			}

			var output = new TOut();
			Populate(output, input);
			return output;
		}

		protected abstract void Populate(TOut output, TIn input);
	}
}
