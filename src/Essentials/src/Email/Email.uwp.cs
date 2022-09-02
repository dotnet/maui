using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Streams;
using PlatformEmailAttachment = Windows.ApplicationModel.Email.EmailAttachment;
using PlatformEmailMessage = Windows.ApplicationModel.Email.EmailMessage;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class EmailImplementation : IEmail
	{
		public bool IsComposeSupported
			=> ApiInformation.IsTypePresent("Windows.ApplicationModel.Email.EmailManager");

		async Task PlatformComposeAsync(EmailMessage message)
		{
			if (message != null && message.BodyFormat != EmailBodyFormat.PlainText)
				throw new FeatureNotSupportedException("UWP can only compose plain text email messages.");

			var platformEmailMessage = new PlatformEmailMessage();
			if (!string.IsNullOrEmpty(message?.Body))
				platformEmailMessage.Body = message.Body;
			if (!string.IsNullOrEmpty(message?.Subject))
				platformEmailMessage.Subject = message.Subject;
			Sync(message?.To, platformEmailMessage.To);
			Sync(message?.Cc, platformEmailMessage.CC);
			Sync(message?.Bcc, platformEmailMessage.Bcc);

			if (message?.Attachments?.Count > 0)
			{
				foreach (var attachment in message.Attachments)
				{
					var path = NormalizePath(attachment.FullPath);
					var file = attachment.File ?? await StorageFile.GetFileFromPathAsync(path);

					var stream = RandomAccessStreamReference.CreateFromFile(file);
					var nativeAttachment = new PlatformEmailAttachment(attachment.FileName, stream);

					if (!string.IsNullOrEmpty(attachment.ContentType))
						nativeAttachment.MimeType = attachment.ContentType;
					else if (!string.IsNullOrWhiteSpace(file?.ContentType))
						nativeAttachment.MimeType = file.ContentType;

					platformEmailMessage.Attachments.Add(nativeAttachment);
				}
			}

			await EmailManager.ShowComposeNewEmailAsync(platformEmailMessage);
		}

		static string NormalizePath(string path)
			=> path.Replace('/', Path.DirectorySeparatorChar);

		void Sync(List<string> recipients, IList<EmailRecipient> nativeRecipients)
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
