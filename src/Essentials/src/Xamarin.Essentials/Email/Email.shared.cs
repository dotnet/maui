using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
	public static partial class Email
	{
		public static Task ComposeAsync()
			=> ComposeAsync(null);

		public static Task ComposeAsync(string subject, string body, params string[] to)
			=> ComposeAsync(new EmailMessage(subject, body, to));

		public static Task ComposeAsync(EmailMessage message)
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
				parts.Add("body=" + Uri.EscapeDataString(message.Body));
			if (!string.IsNullOrEmpty(message?.Subject))
				parts.Add("subject=" + Uri.EscapeDataString(message.Subject));
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

	public class EmailMessage
	{
		public EmailMessage()
		{
		}

		public EmailMessage(string subject, string body, params string[] to)
		{
			Subject = subject;
			Body = body;
			To = to?.ToList() ?? new List<string>();
		}

		public string Subject { get; set; }

		public string Body { get; set; }

		public EmailBodyFormat BodyFormat { get; set; }

		public List<string> To { get; set; } = new List<string>();

		public List<string> Cc { get; set; } = new List<string>();

		public List<string> Bcc { get; set; } = new List<string>();

		public List<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
	}

	public enum EmailBodyFormat
	{
		PlainText,
		Html
	}

	public partial class EmailAttachment : FileBase
	{
		public EmailAttachment(string fullPath)
			: base(fullPath)
		{
		}

		public EmailAttachment(string fullPath, string contentType)
			: base(fullPath, contentType)
		{
		}

		public EmailAttachment(FileBase file)
			: base(file)
		{
		}
	}
}
