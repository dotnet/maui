using System;
using System.Collections.Generic;
using System.IO;
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
                parts.Add("body=" + Uri.EscapeUriString(message.Body));
            if (!string.IsNullOrEmpty(message?.Subject))
                parts.Add("subject=" + Uri.EscapeUriString(message.Subject));
            if (message?.To.Count > 0)
                parts.Add("to=" + string.Join(",", message.To));
            if (message?.Cc.Count > 0)
                parts.Add("cc=" + string.Join(",", message.Cc));
            if (message?.Bcc.Count > 0)
                parts.Add("bcc=" + string.Join(",", message.Bcc));

            var uri = "mailto:";
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
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.EmailAttachments);
        }

        public EmailAttachment(string fullPath, string contentType)
            : base(fullPath, contentType)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.EmailAttachments);
        }

        public EmailAttachment(FileBase file)
            : base(file)
        {
            ExperimentalFeatures.VerifyEnabled(ExperimentalFeatures.EmailAttachments);
        }

        string attachmentName;

        public string AttachmentName
        {
            get => GetAttachmentName();
            set => attachmentName = value;
        }

        internal string GetAttachmentName()
        {
            // try the provided file name
            if (!string.IsNullOrWhiteSpace(attachmentName))
                return attachmentName;

            // try get from the path
            if (!string.IsNullOrWhiteSpace(FullPath))
                return Path.GetFileName(FullPath);

            // this should never happen as the path is validated in the constructor
            throw new InvalidOperationException($"Unable to determine the attachment file name from '{FullPath}'.");
        }
    }
}
