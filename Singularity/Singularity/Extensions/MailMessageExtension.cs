using System.Diagnostics;
using System.Net.Mail;

namespace Singularity
{
	[DebuggerStepThrough]
	public static class MailMessageExtension
	{
		/// <summary>
		/// Clone a MailMessage
		/// </summary>
		/// <param name="aTemplateMailMessage">MailMessage to clone.</param>
		/// <returns>A cloned MailMessage.</returns>
		public static MailMessage Clone(this MailMessage aTemplateMailMessage)
		{
			var result = new MailMessage();
			if (aTemplateMailMessage == null)
			{
				return result;
			}

			result.From = aTemplateMailMessage.From;
			result.Subject = aTemplateMailMessage.Subject;
			result.Body = aTemplateMailMessage.Body;
			result.IsBodyHtml = aTemplateMailMessage.IsBodyHtml;
			result.DeliveryNotificationOptions = aTemplateMailMessage.DeliveryNotificationOptions;
			result.Priority = aTemplateMailMessage.Priority;
			result.Sender = aTemplateMailMessage.Sender;
			result.SubjectEncoding = aTemplateMailMessage.SubjectEncoding;
			result.BodyEncoding = aTemplateMailMessage.BodyEncoding;

			if (aTemplateMailMessage.To != null)
			{
				result.To.AddRange(aTemplateMailMessage.To);
			}

			if (aTemplateMailMessage.CC != null)
			{
				result.CC.AddRange(aTemplateMailMessage.CC);
			}

			if (aTemplateMailMessage.Bcc != null)
			{
				result.Bcc.AddRange(aTemplateMailMessage.Bcc);
			}

			if (aTemplateMailMessage.ReplyToList != null)
			{
				result.ReplyToList.AddRange(aTemplateMailMessage.ReplyToList);
			}

			if (aTemplateMailMessage.Attachments != null)
			{
				foreach (var attachment in aTemplateMailMessage.Attachments)
				{
					result.Attachments.Add(attachment);
				}
			}

			if (aTemplateMailMessage.AlternateViews != null)
			{
				foreach (var alternateView in aTemplateMailMessage.AlternateViews)
				{
					result.AlternateViews.Add(alternateView);
				}
			}

			return result;
		}

	}
}
