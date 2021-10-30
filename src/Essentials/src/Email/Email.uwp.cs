using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Streams;
using NativeEmailAttachment = Windows.ApplicationModel.Email.EmailAttachment;
using NativeEmailMessage = Windows.ApplicationModel.Email.EmailMessage;

namespace Microsoft.Maui.Essentials
{
	public static partial class Email
	{
		internal static bool IsComposeSupported
			=> ApiInformation.IsTypePresent("Windows.ApplicationModel.Email.EmailManager");

		static async Task PlatformComposeAsync(EmailMessage message)
		{
			if (message != null && message.BodyFormat != EmailBodyFormat.PlainText)
				throw new FeatureNotSupportedException("UWP can only compose plain text email messages.");

			var nativeMessage = new NativeEmailMessage();
			if (!string.IsNullOrEmpty(message?.Body))
				nativeMessage.Body = message.Body;
			if (!string.IsNullOrEmpty(message?.Subject))
				nativeMessage.Subject = message.Subject;
			Sync(message?.To, nativeMessage.To);
			Sync(message?.Cc, nativeMessage.CC);
			Sync(message?.Bcc, nativeMessage.Bcc);

			if (message?.Attachments?.Count > 0)
			{
				foreach (var attachment in message.Attachments)
				{
					var path = FileSystem.NormalizePath(attachment.FullPath);
					var file = attachment.File ?? await StorageFile.GetFileFromPathAsync(path);

					var stream = RandomAccessStreamReference.CreateFromFile(file);
					var nativeAttachment = new NativeEmailAttachment(attachment.FileName, stream);

					if (!string.IsNullOrEmpty(attachment.ContentType))
						nativeAttachment.MimeType = attachment.ContentType;
					else if (!string.IsNullOrWhiteSpace(file?.ContentType))
						nativeAttachment.MimeType = file.ContentType;

					nativeMessage.Attachments.Add(nativeAttachment);
				}
			}

			await EmailManager.ShowComposeNewEmailAsync(nativeMessage);
		}

		static void Sync(List<string> recipients, IList<EmailRecipient> nativeRecipients)
		{
			if (recipients == null)
				return;

			foreach (var recipient in recipients)
			{
				if (string.IsNullOrWhiteSpace(recipient))
					continue;

				nativeRecipients.Add(new EmailRecipient(recipient));
			}
		}
	}
}
