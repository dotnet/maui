using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public interface IEmail
	{
		bool IsComposeSupported { get; }
		
		Task ComposeAsync();

		Task ComposeAsync(string subject, string body, params string[] to);

		Task ComposeAsync(EmailMessage message);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Email']/Docs" />
	public static partial class Email
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="//Member[@MemberName='ComposeAsync'][1]/Docs" />
		public static Task ComposeAsync()
			=> ComposeAsync(null);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="//Member[@MemberName='ComposeAsync'][3]/Docs" />
		public static Task ComposeAsync(string subject, string body, params string[] to)
			=> ComposeAsync(new EmailMessage(subject, body, to));

		/// <include file="../../docs/Microsoft.Maui.Essentials/Email.xml" path="//Member[@MemberName='ComposeAsync'][2]/Docs" />
		public static Task ComposeAsync(EmailMessage message)
		{
			if (!Current.IsComposeSupported)
				throw new FeatureNotSupportedException();

			return Current.ComposeAsync(message);
		}

		internal static string GetMailToUri(EmailMessage message)
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

#nullable enable
		static IEmail? currentImplementation;
#nullable disable

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IEmail Current =>
			currentImplementation ??= new EmailImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
#nullable enable
		public static void SetCurrent(IEmail? implementation) =>
			currentImplementation = implementation;
#nullable disable
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
		public string Subject { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='Body']/Docs" />
		public string Body { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='BodyFormat']/Docs" />
		public EmailBodyFormat BodyFormat { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='To']/Docs" />
		public List<string> To { get; set; } = new List<string>();

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='Cc']/Docs" />
		public List<string> Cc { get; set; } = new List<string>();

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='Bcc']/Docs" />
		public List<string> Bcc { get; set; } = new List<string>();

		/// <include file="../../docs/Microsoft.Maui.Essentials/EmailMessage.xml" path="//Member[@MemberName='Attachments']/Docs" />
		public List<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
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
