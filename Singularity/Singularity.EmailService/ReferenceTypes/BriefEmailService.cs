using System;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Net.Mail;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace

namespace Singularity.EmailService
{
	public class BriefEmailService
	{
		public TEmailMessage PackageEmail<TEmailMessage>(TEmailMessage emailContent, params Object[] contents)
			where TEmailMessage : IEmailMessage, new()
		{
			TEmailMessage result = new TEmailMessage();
			if (!emailContent.IsEmpty() && !emailContent.Subject.IsEmpty() && !emailContent.Body.IsEmpty())
			{
				result = new TEmailMessage
				{
					Subject = emailContent.Subject,
					Body = emailContent.Body,
					IsBodyHtml = emailContent.IsBodyHtml,
				};
				ExpandoObject model = contents.ToExpandoObject();
				result.Body = PackageTemplateCore(result.Body, model);
				result.Subject = PackageTemplateCore(result.Subject, model);
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IStaticMessage PackageTemplate(IStaticMessage staticMessage, params Object[] contents)
		{
			if (staticMessage.IsEmpty()) throw new ArgumentException("Given staticMessage argument cannot be empty.", "staticMessage");

			ExpandoObject model = contents.ToExpandoObject();
			return new StaticMessage
				(
					new EmailFrom(PackageTemplateCore(staticMessage.From.Address, model)), 
					PackageTemplateCore(staticMessage.Subject, model),
					PackageTemplateCore(staticMessage.Body, model)
				);
		}

		protected virtual String PackageTemplateCore(String emailTemplate, ExpandoObject model)
		{
			return emailTemplate.FormatWith(model);
		}

		/*
		 * For Razor support - inherit this class and add the following override to Singularity.Web project.
		 
		protected override String PackageEmailBody(String body, ExpandoObject model)
		{
			String result = body;
			if (emailContent.Body.StartsWith("@model"))
			{
				//performance optimization - to ensure the template is compiled only once
				Razor.GetTemplate(emailContent.Body, typeof(ExpandoObject), emailContent.Name);
				result = Razor.Run(emailContent.Name, model);
			}
		   else
		   {
				result = base.PackageEmailBody(body, model);
			}
			return result;
		}

		*/

		public Boolean IsValidEmailAddress(String emailAddress)
		{
			if (emailAddress == null)
			{
				return false;
			}

			return Regex.IsMatch(emailAddress, @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
				@"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
				@".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
		}

		// Preferred method to send email.  Modify interface for new features...
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public SendEmailResponse<INotification> SendSmtpMail(IEmailMessage emailMessage)
		{
			if (emailMessage is null)
			{
				throw new ArgumentException("Argument emailMessage cannot be null.");
			}

			SendEmailResponse<INotification> result = new SendEmailResponse<INotification>();
			try
			{
				using (MailMessage mailMessage = emailMessage.ToMailMessage())
				using (SmtpClient smtp = new SmtpClient())
				{
					smtp.Send(mailMessage);
				}
			}
			catch (SmtpException smtpException)
			{
				result.Value = new Notification(smtpException);
			}
			catch (InvalidOperationException invalidOperationException)
			{
				result.Value = new Notification(invalidOperationException);
			}
			catch (ArgumentException argumentExceptionException)
			{
				result.Value = new Notification(argumentExceptionException);
			}
			return result;
		}
	}
}
