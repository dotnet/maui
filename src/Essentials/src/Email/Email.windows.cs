using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class EmailImplementation : IEmail
	{
		public bool IsComposeSupported
			=> true;

		async Task PlatformComposeAsync(EmailMessage message)
		{
			if (message != null && message.BodyFormat != EmailBodyFormat.PlainText)
				throw new FeatureNotSupportedException("Windows can only compose plain text email messages.");

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

					platformEmailMessage.Attachments.Add(path);
				}
			}

			await EmailHelper.ShowComposeNewEmailAsync(platformEmailMessage);
		}

		static string NormalizePath(string path)
			=> path.Replace('/', Path.DirectorySeparatorChar);

		void Sync(List<string> recipients, IList<PlatformEmailRecipient> nativeRecipients)
		{
			if (recipients == null)
				return;

			foreach (var recipient in recipients)
			{
				if (string.IsNullOrWhiteSpace(recipient))
					continue;

				nativeRecipients.Add(new PlatformEmailRecipient(recipient));
			}
		}
	}
}
