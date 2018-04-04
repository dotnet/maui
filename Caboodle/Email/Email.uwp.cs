using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Email;
using Windows.Foundation.Metadata;

using NativeEmailMessage = Windows.ApplicationModel.Email.EmailMessage;

namespace Microsoft.Caboodle
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

            await EmailManager.ShowComposeNewEmailAsync(nativeMessage);
        }

        static void Sync(List<string> recipients, IList<EmailRecipient> nativeRecipients)
        {
            if (recipients == null)
                return;

            foreach (var recipient in recipients)
            {
                nativeRecipients.Add(new EmailRecipient(recipient));
            }
        }
    }
}
