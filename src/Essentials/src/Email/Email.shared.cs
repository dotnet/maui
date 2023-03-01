#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	/// <summary>
	/// Provides an easy way to allow the user to send emails.
	/// </summary>
	public interface IEmail
	{
		/// <summary>
		/// Gets a value indicating whether composing an email is supported on this device.
		/// </summary>
		bool IsComposeSupported { get; }

		/// <summary>
		/// Opens the default email client to allow the user to send the message.
		/// </summary>
		/// <param name="message">Instance of <see cref="EmailMessage"/> containing details of the email message to compose.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task ComposeAsync(EmailMessage? message);
	}

	/// <summary>
	/// Static class with extension methods for the <see cref="IEmail"/> APIs.
	/// </summary>
	public static class EmailExtensions
	{
		/// <summary>
		/// Opens the default email client to allow the user to send the message.
		/// </summary>
		/// <param name="email">The object this method is invoked on.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task ComposeAsync(this IEmail email) =>
			email.ComposeAsync(null);

		/// <summary>
		/// Opens the default email client to allow the user to send the message with the provided subject, body, and recipients.
		/// </summary>
		/// <param name="email">The object this method is invoked on.</param>
		/// <param name="subject">The email subject.</param>
		/// <param name="body">The email body.</param>
		/// <param name="to">The email recipients.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
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

		internal static string GetMailToUri(EmailMessage message) =>
			"mailto:?" + string.Join("&", Parameters(message));

		static IEnumerable<string> Parameters(EmailMessage message)
		{
			if (message.To?.Count > 0)
				yield return "to=" + Recipients(message.To);

			if (message.Cc?.Count > 0)
				yield return "cc=" + Recipients(message.Cc);

			if (message.Bcc?.Count > 0)
				yield return "bcc=" + Recipients(message.Bcc);

			if (!string.IsNullOrWhiteSpace(message.Subject))
				yield return "subject=" + Uri.EscapeDataString(message.Subject);

			if (!string.IsNullOrWhiteSpace(message.Body))
				yield return "body=" + Uri.EscapeDataString(message.Body);
		}

		static string Recipients(IEnumerable<string> addresses) =>
			string.Join(",", addresses.Select(Uri.EscapeDataString));
	}

	/// <summary>
	/// Provides an easy way to allow the user to send emails.
	/// </summary>
	public static class Email
	{
		/// <summary>
		/// Opens the default email client to allow the user to send the message.
		/// </summary>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task ComposeAsync() =>
			Default.ComposeAsync();

		/// <summary>
		/// Opens the default email client to allow the user to send the message with the provided subject, body, and recipients.
		/// </summary>
		/// <param name="subject">The email subject.</param>
		/// <param name="body">The email body.</param>
		/// <param name="to">The email recipients.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task ComposeAsync(string subject, string body, params string[] to) =>
			Default.ComposeAsync(subject, body, to);

		/// <summary>
		/// Opens the default email client to allow the user to send the message.
		/// </summary>
		/// <param name="message">Instance of <see cref="EmailMessage"/> containing details of the email message to compose.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task ComposeAsync(EmailMessage message) =>
			Default.ComposeAsync(message);

		static IEmail? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IEmail Default =>
			defaultImplementation ??= new EmailImplementation();

		internal static void SetDefault(IEmail? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// Represents a single email message.
	/// </summary>
	public class EmailMessage
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EmailMessage"/> class.
		/// </summary>
		public EmailMessage()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EmailMessage"/> class with the specified subject, body, and recipients.
		/// </summary>
		/// <param name="subject">The email subject.</param>
		/// <param name="body">The email body.</param>
		/// <param name="to">The email recipients.</param>
		public EmailMessage(string subject, string body, params string[] to)
		{
			Subject = subject;
			Body = body;
			To = to?.ToList() ?? new List<string>();
		}

		/// <summary>
		/// Gets or sets the email's subject.
		/// </summary>
		public string? Subject { get; set; }

		/// <summary>
		/// Gets or sets the email's body.
		/// </summary>
		public string? Body { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the message is in plain text or HTML.
		/// </summary>
		/// <remarks><see cref="EmailBodyFormat.Html"/> is not supported on Windows.</remarks>
		public EmailBodyFormat BodyFormat { get; set; }

		/// <summary>
		/// Gets or sets the email's recipients.
		/// </summary>
		public List<string>? To { get; set; } = new List<string>();

		/// <summary>
		/// Gets or sets the email's CC (Carbon Copy) recipients.
		/// </summary>
		public List<string>? Cc { get; set; } = new List<string>();

		/// <summary>
		/// Gets or sets the email's BCC (Blind Carbon Copy) recipients.
		/// </summary>
		public List<string>? Bcc { get; set; } = new List<string>();

		/// <summary>
		/// Gets or sets a list of file attachments as <see cref="EmailAttachment"/> objects.
		/// </summary>
		public List<EmailAttachment>? Attachments { get; set; } = new List<EmailAttachment>();
	}

	/// <summary>
	/// Represents various types of email body formats.
	/// </summary>
	public enum EmailBodyFormat
	{
		/// <summary>The email message body is plain text.</summary>
		PlainText,

		/// <summary>The email message body is HTML (not supported on Windows).</summary>
		Html
	}

	/// <summary>
	/// Represents a email file attachment.
	/// </summary>
	public partial class EmailAttachment : FileBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="EmailAttachment"/> class based off the file specified in the provided path.
		/// </summary>
		/// <param name="fullPath">Full path and filename to file on filesystem.</param>
		public EmailAttachment(string fullPath)
			: base(fullPath)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EmailAttachment"/> class based off the file specified in the provided path
		/// and providing an explicit MIME filetype.
		/// </summary>
		/// <param name="fullPath">Full path and filename to file on filesystem.</param>
		/// <param name="contentType">Content type (MIME type) of the file (e.g.: <c>image/png</c>).</param>
		public EmailAttachment(string fullPath, string contentType)
			: base(fullPath, contentType)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EmailAttachment"/> class based off a <see cref="FileBase"/> object.
		/// </summary>
		/// <param name="file">An instance of <see cref="FileBase"/> with file information.</param>
		public EmailAttachment(FileBase file)
			: base(file)
		{
		}
	}
}
