using System.Linq;
using System.Threading.Tasks;
#if !(MACCATALYST || MACOS)
using MessageUI;
#endif

namespace Microsoft.Maui.Essentials.Implementations
{
	public class SmsImplementation: ISms
	{
		internal static bool IsComposeSupported
#if !(MACCATALYST || MACOS)
			=> MFMessageComposeViewController.CanSendText;
#else
			=> false;
#endif

		public Task ComposeAsync(SmsMessage message)
		{
#if !(MACCATALYST || MACOS)
			// do this first so we can throw as early as possible
			var controller = Platform.GetCurrentViewController();

			// create the controller
			var messageController = new MFMessageComposeViewController();
			if (!string.IsNullOrWhiteSpace(message?.Body))
				messageController.Body = message.Body;

			messageController.Recipients = message?.Recipients?.ToArray() ?? new string[] { };

			// show the controller
			var tcs = new TaskCompletionSource<bool>();
			messageController.Finished += (sender, e) =>
			{
				messageController.DismissViewController(true, null);
				tcs?.TrySetResult(e.Result == MessageComposeResult.Sent);
			};

			if (controller.PresentationController != null)
			{
				controller.PresentationController.Delegate =
					new Platform.UIPresentationControllerDelegate(() => tcs.TrySetResult(false));
			}

			controller.PresentViewController(messageController, true, null);

			return tcs.Task;
#else
			return Task.CompletedTask;
#endif
		}
	}
}
