using System;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	public class ReplyMessage : ReplySimple
	{
		public ReplyMessage(String message, Boolean condition = false) : base(condition)
		{
			Message = message;
		}

		public String Message { get; set; }
	}
}