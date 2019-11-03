using System;
using System.Net.Mail;
using System.Text;

// ReSharper disable once CheckNamespace

namespace Singularity.EmailService
{
	public class EmailAddress : MailAddress, IComparable, IComparable<EmailAddress>, IMessageFrom
	{
		public EmailAddress(String address) : base(address) { }
		public EmailAddress(String address, String displayName) : base(address, displayName) { }
		public EmailAddress(String address, String displayName, Encoding displayNameEncoding) : base(address, displayName, displayNameEncoding) { }

		public Boolean IsAddressVerified { get; set; }

		public Int32 CompareTo(EmailAddress other)
		{
			if (other == null)
			{
				throw new ArgumentNullException("Argument other cannot be null.");
			}

			return String.Compare(Address, other.Address, StringComparison.CurrentCulture);
		}

		public Int32 CompareTo(Object obj)
		{
			return Address.CompareTo(obj);
		}

		public static Int32 GetHashCode(EmailAddress obj)
		{
			return obj?.GetHashCode() ?? 0;
		}

		public override Int32 GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				Int32 hash = (Int32)2166136261;
				// Suitable nullity checks etc, of course :)
				hash = hash * 16777619 ^ Address.GetHashCode();
				hash = hash * 16777619 ^ DisplayName.GetHashCode();
				hash = hash * 16777619 ^ Host.GetHashCode();
				hash = hash * 16777619 ^ User.GetHashCode();
				return hash;
			}
		}

		public override Boolean Equals(Object obj)
		{
			return ReferenceEquals(this, obj);
		}

	}
}
