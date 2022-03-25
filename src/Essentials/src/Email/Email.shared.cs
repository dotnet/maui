#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	public interface IEmail
	{
		bool IsComposeSupported { get; }

		Task ComposeAsync(EmailMessage? message);
	}

	public static class EmailExtensions
	{
		public static Task ComposeAsync(this IEmail email) =>
			email.ComposeAsync(null);

		public static Task ComposeAsync(this IEmail email, string subject, string body, params string[] to) =>
			email.ComposeAsync(new EmailMessage(subject, body, to));
	}

	partial class EmailImplementation : IEmail
	{
		public Task ComposeAsync(EmailMessage? message)
		{
			if (!IsComposeSupported)
				throw new FeatureNotSupportedException();

			return PlatformComposeAsync(message);
		}

		static string GetMailToUri(EmailMessage message)
		{
			if (message != null && message.BodyFormat != EmailBodyFormat.PlainText)
				throw new FeatureNotSupportedException("Only EmailBodyFormat.PlainText is supported if no email account is set up.");

			var parts = new List<string>();
			if (!string.IsNullOrEmpty(message?.Body))
				parts.Add("body=" + Uri.EscapeDataString(message!.Body));
			if (!string.IsNullOrEmpty(message?.Subject))
				parts.Add("subject=" + Uri.EscapeDataString(message!.Subject));
			if (message?.Cc?.Count > 0)
				parts.Add("cc=" + Uri.EscapeDataString(string.Join(",", message.Cc)));
			if (message?.Bcc?.Count > 0)
				parts.Add("bcc=" + Uri.EscapeDataString(string.Join(",", message.Bcc)));

			var uri = "mailto:";

			if (message?.To?.Count > 0)
				uri += Uri.EscapeDataString(string.Join(",", message.To));

			if (parts.Count > 0)
				uri += "?" + string.Join("&", parts);

			return uri;
		}
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Email']/Docs" />
	public static class Email
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="//Member[@MemberName='ComposeAsync'][1]/Docs" />
		public static Task ComposeAsync() =>
			Default.ComposeAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="//Member[@MemberName='ComposeAsync'][3]/Docs" />
		public static Task ComposeAsync(string subject, string body, params string[] to) =>
			Default.ComposeAsync(subject, body, to);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="//Member[@MemberName='ComposeAsync'][2]/Docs" />
		public static Task ComposeAsync(EmailMessage message) =>
			Default.ComposeAsync(message);

		static IEmail? defaultImplementation;

		public static IEmail Default =>
			defaultImplementation ??= new EmailImplementation();

		internal static void SetDefault(IEmail? implementation) =>
			defaultImplementation = implementation;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="Type[@FullName='Microsoft.Maui.Essentials.EmailMessage']/Docs" />
	public class EmailMessage
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public EmailMessage()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public EmailMessage(string subject, string body, params string[] to)
		{
			Subject = subject;
			Body = body;
			To = to?.ToList() ?? new List<string>();
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='Subject']/Docs" />
		public string? Subject { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='Body']/Docs" />
		public string? Body { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='BodyFormat']/Docs" />
		public EmailBodyFormat BodyFormat { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='To']/Docs" />
		public List<string>? To { get; set; } = new List<string>();

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='Cc']/Docs" />
		public List<string>? Cc { get; set; } = new List<string>();

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='Bcc']/Docs" />
		public List<string>? Bcc { get; set; } = new List<string>();

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='Attachments']/Docs" />
		public List<EmailAttachment>? Attachments { get; set; } = new List<EmailAttachment>();
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/EmailBodyFormat.xml" path="Type[@FullName='Microsoft.Maui.Essentials.EmailBodyFormat']/Docs" />
	public enum EmailBodyFormat
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailBodyFormat.xml" path="//Member[@MemberName='PlainText']/Docs" />
		PlainText,
		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailBodyFormat.xml" path="//Member[@MemberName='Html']/Docs" />
		Html
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/EmailAttachment.xml" path="Type[@FullName='Microsoft.Maui.Essentials.EmailAttachment']/Docs" />
	public partial class EmailAttachment : FileBase
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailAttachment.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public EmailAttachment(string fullPath)
			: base(fullPath)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailAttachment.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public EmailAttachment(string fullPath, string contentType)
			: base(fullPath, contentType)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailAttachment.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public EmailAttachment(FileBase file)
			: base(file)
		{
		}
	}
}
