using System;

// ReSharper disable once CheckNamespace

namespace Singularity.EmailService
{
	public struct EmailFrom : IMessageFrom
	{
		public EmailFrom(String address)
		{
			Address = address;
		}

		public String Address { get; }

		public override Boolean Equals(Object obj)
		{
			return (obj is EmailFrom && ((EmailFrom)obj).Address == Address);
		}
	}
}
