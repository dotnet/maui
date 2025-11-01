using System.Threading.Tasks;
#if !(MACCATALYST || MACOS)
using System;
using System.Linq;
using MessageUI;
#endif

namespace Microsoft.Maui.ApplicationModel.Communication
{
	partial class SmsImplementation : ISms
	{
		public bool IsComposeSupported
#if !(MACCATALYST || MACOS)
			=> MFMessageComposeViewController.CanSendText;
#else
			=> false;
#endif

		Task PlatformComposeAsync(SmsMessage message)
		{
#if !(MACCATALYST || MACOS)
			// do this first so we can throw as early as possible
			var controller = WindowStateManager.Default.GetCurrentUIViewController(true);

			// create the controller
			var messageController = new MFMessageComposeViewController();
			if (!string.IsNullOrWhiteSpace(message?.Body))
				messageController.Body = message.Body;

			messageController.Recipients = message?.Recipients?.ToArray() ?? Array.Empty<string>();

			// show the controller
			var tcs = new TaskCompletionSource<bool>();
			messageController.Finished += (sender, e) =>
			{
				messageController.DismissViewController(true, null);
				tcs?.TrySetResult(e.Result == MessageComposeResult.Sent);
			};

			controller.PresentationController?.Delegate =
					new UIPresentationControllerDelegate(() => tcs.TrySetResult(false));

			controller.PresentViewController(messageController, true, null);

			return tcs.Task;
#else
			return Task.CompletedTask;
#endif
		}
	}
}
