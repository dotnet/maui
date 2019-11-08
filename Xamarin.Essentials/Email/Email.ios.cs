using System;
using System.Threading.Tasks;
using Foundation;
using MessageUI;
using MobileCoreServices;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Email
    {
        internal static bool IsComposeSupported
        {
            get
            {
                var can = MFMailComposeViewController.CanSendMail;
                if (!can)
                {
                    var url = NSUrl.FromString("mailto:");
                    NSRunLoop.Main.InvokeOnMainThread(() => can = UIApplication.SharedApplication.CanOpenUrl(url));
                }
                return can;
            }
        }

        static Task PlatformComposeAsync(EmailMessage message)
        {
            if (MFMailComposeViewController.CanSendMail)
                return ComposeWithMailCompose(message);
            else
                return ComposeWithUrl(message);
        }

        static Task ComposeWithMailCompose(EmailMessage message)
        {
            // do this first so we can throw as early as possible
            var parentController = Platform.GetCurrentViewController();

            // create the controller
            var controller = new MFMailComposeViewController();
            if (!string.IsNullOrEmpty(message?.Body))
                controller.SetMessageBody(message.Body, message.BodyFormat == EmailBodyFormat.Html);
            if (!string.IsNullOrEmpty(message?.Subject))
                controller.SetSubject(message.Subject);
            if (message?.To?.Count > 0)
                controller.SetToRecipients(message.To.ToArray());
            if (message?.Cc?.Count > 0)
                controller.SetCcRecipients(message.Cc.ToArray());
            if (message?.Bcc?.Count > 0)
                controller.SetBccRecipients(message.Bcc.ToArray());

            if (message?.Attachments?.Count > 0)
            {
                foreach (var attachment in message.Attachments)
                {
                    var data = NSData.FromFile(attachment.FullPath);
                    controller.AddAttachmentData(data, attachment.ContentType, attachment.FileName);
                }
            }

            // show the controller
            var tcs = new TaskCompletionSource<bool>();
            controller.Finished += (sender, e) =>
            {
                controller.DismissViewController(true, null);
                tcs.SetResult(e.Result == MFMailComposeResult.Sent);
            };
            parentController.PresentViewController(controller, true, null);

            return tcs.Task;
        }

        static Task ComposeWithUrl(EmailMessage message)
        {
            var url = GetMailToUri(message);

            var tcs = new TaskCompletionSource<bool>();
            var nsurl = NSUrl.FromString(url);
            UIApplication.SharedApplication.OpenUrl(nsurl, new UIApplicationOpenUrlOptions(), r => tcs.TrySetResult(r));
            return tcs.Task;
        }
    }
}
