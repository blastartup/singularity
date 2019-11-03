using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace

namespace Singularity.EmailService
{
	[DebuggerStepThrough]
	public static class MailAddressExtension
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static Boolean IsValid(this MailAddress mailAddress)
		{
			if (mailAddress.IsEmpty())
			{
				return false;
			}

			return Regex.IsMatch(mailAddress.Address, @"^([\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+\.)*[\w\!\#$\%\&\'\*\+\-\/\=\?\^\`{\|\}\~]+@((((([a-z0-9]{1}[a-z0-9\-]{0,62}[a-z0-9]{1})|[a-z])\.)+[a-z]{2,6})|(\d{1,3}\.){3}\d{1,3}(\:\d{1,5})?)$", RegexOptions.Singleline);
		}

	}
}
