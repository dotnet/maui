using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Foundation;
#if !(MACCATALYST || MACOS)
using MessageUI;
#endif
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class EmailImplementation : IEmail
	{
		public bool IsComposeSupported =>
#if !(MACCATALYST || MACOS)
			MFMailComposeViewController.CanSendMail ||
				MainThread.InvokeOnMainThread(() => UIApplication.SharedApplication.CanOpenUrl(NSUrl.FromString("mailto:")));
#else
			false;
#endif

		public Task ComposeAsync(EmailMessage message)
		{
#if !(MACCATALYST || MACOS)
			if (MFMailComposeViewController.CanSendMail)
				return ComposeWithMailCompose(message);
			else
				return ComposeWithUrl(message);
#else
			return Task.CompletedTask;
#endif
		}

		public Task ComposeAsync(string subject, string body, params string[] to)
			=> ComposeAsync(
				new EmailMessage()
				{
					Subject = subject,
					Body = body,
					To = to.ToList()
				});

		public Task ComposeAsync()
			=> ComposeAsync(null);


		internal Task ComposeWithMailCompose(EmailMessage message)
		{
#if !(MACCATALYST || MACOS)
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

			if (controller.PresentationController != null)
			{
				controller.PresentationController.Delegate =
					new Platform.UIPresentationControllerDelegate(() => tcs.TrySetResult(false));
			}

			parentController.PresentViewController(controller, true, null);

			return tcs.Task;
#else
			return Task.CompletedTask;
#endif
		}

		internal async Task ComposeWithUrl(EmailMessage message)
		{
			var url = Email.GetMailToUri(message);
			var nsurl = NSUrl.FromString(url);
			await Launcher.OpenAsync(nsurl);
		}
	}
}
