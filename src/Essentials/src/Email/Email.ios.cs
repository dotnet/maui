using System.IO;
using System.Threading.Tasks;
using Foundation;
using UIKit;
#if !(MACOS)
using MessageUI;
#endif

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class EmailImplementation : IEmail
	{
		public bool IsComposeSupported =>
#if !(MACOS)
			MFMailComposeViewController.CanSendMail ||
			MainThread.InvokeOnMainThread(() => UIApplication.SharedApplication.CanOpenUrl(NSUrl.FromString("mailto:")));
#else
			false;
#endif

		Task PlatformComposeAsync(EmailMessage message)
		{
#if !(MACOS)
			if (MFMailComposeViewController.CanSendMail)
				return ComposeWithMailCompose(message);
			else
				return ComposeWithUrl(message);
#else
			return Task.CompletedTask;
#endif
		}

		Task ComposeWithMailCompose(EmailMessage message)
		{
#if !(MACOS)
			// do this first so we can throw as early as possible
			var parentController = WindowStateManager.Default.GetCurrentUIViewController(true);

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
					if (data == null)
						throw new FileNotFoundException($"Attachment {attachment.FileName} not found.", attachment.FullPath);

					controller.AddAttachmentData(data, attachment.ContentType, attachment.FileName);
				}
			}

			// show the controller
			var tcs = new TaskCompletionSource<bool>();
			controller.Finished += (sender, e) =>
			{
				controller.DismissViewController(true, null);
				tcs.TrySetResult(e.Result == MFMailComposeResult.Sent);
			};

			controller.PresentationController?.Delegate =
					new UIPresentationControllerDelegate(() => tcs.TrySetResult(false));

			parentController.PresentViewController(controller, true, null);

			return tcs.Task;
#else
			return Task.CompletedTask;
#endif
		}

		Task ComposeWithUrl(EmailMessage message)
		{
			var url = GetMailToUri(message);
			var nsurl = NSUrl.FromString(url);
			return Launcher.Default.OpenAsync(nsurl);
		}
	}
}
