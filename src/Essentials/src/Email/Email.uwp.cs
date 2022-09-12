using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using PlatformEmailMessage = Windows.ApplicationModel.Email.EmailMessage;

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class EmailImplementation : IEmail
	{
		public bool IsComposeSupported
		=> true;

		Task<object> PlatformComposeAsync(EmailMessage message)
		{
			if (message != null && message.BodyFormat != EmailBodyFormat.PlainText)
				throw new FeatureNotSupportedException("WinUI can only compose plain text email messages.");

			var platformEmailMessage = new PlatformEmailMessage();

			if (!string.IsNullOrEmpty(message?.Body))
				platformEmailMessage.Body = message.Body;

			if (!string.IsNullOrEmpty(message?.Subject))
				platformEmailMessage.Subject = message.Subject;

			Sync(message?.To, platformEmailMessage.To);
			Sync(message?.Cc, platformEmailMessage.CC);
			Sync(message?.Bcc, platformEmailMessage.Bcc);

			var emailHelper = new EmailHelper();

			foreach (var to in platformEmailMessage.To)
			{
				var recipient = new MapiRecipDesc();

				if (emailHelper.ResolveName(to.Address, ref recipient))
					emailHelper.AddRecipient(recipient);
			}

			if (message?.Attachments?.Count > 0)
			{
				foreach (var attachment in message.Attachments)
					emailHelper.AddAttachment(attachment.FullPath);
			}
	
			var result = emailHelper.SendMail(platformEmailMessage.Subject, platformEmailMessage.Body);

			return Task.FromResult<object>(result);
		}

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