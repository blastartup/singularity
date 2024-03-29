﻿using System;
using System.Diagnostics;
using System.Text;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	public class StrayBarcodeStrategy : StrayStrategy
	{
		public StrayBarcodeStrategy(Int32 length, Int16 checkSumLength)
			: base(length)
		{
			if (length < 4 || length > 20)
			{
				throw new InvalidOperationException("Length exceeds boundary requires 4 =< length =< 30.");
			}
			CheckSumLength = (Int16)checkSumLength.LimitMin(1);
			Length = length - checkSumLength;
		}

		[DebuggerStepThrough]
		public override IReply Execute()
		{
			var numberBuilder = new StringBuilder();
			for (var idx = 0; idx < Length; idx++)
			{
				numberBuilder.Append(ValueLib.BarcodeAlphabet[Random.Next(0, 28)]);
			}
			numberBuilder.Append(numberBuilder.ToString().GetChecksum(CheckSumLength));
			return new ReplyMessage(numberBuilder.ToString(), true);
		}

		protected readonly Int32 Length;
		protected readonly Int16 CheckSumLength;
	}
}
