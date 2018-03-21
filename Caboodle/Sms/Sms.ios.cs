using System.Threading.Tasks;
using MessageUI;

namespace Microsoft.Caboodle
{
    public static partial class Sms
    {
        static bool IsComposeSupported
            => MFMessageComposeViewController.CanSendText;

        public static Task ComposeAsync(SmsMessage message)
        {
            if (!IsComposeSupported)
                throw new FeatureNotSupportedException();

            // do this first so we can throw as early as possible
            var controller = Platform.GetCurrentViewController();

            // create the controller
            var messageController = new MFMessageComposeViewController();
            if (!string.IsNullOrWhiteSpace(message?.Body))
                messageController.Body = message.Body;
            if (!string.IsNullOrWhiteSpace(message?.Recipient))
                messageController.Recipients = new[] { message.Recipient };

            // show the controller
            var tcs = new TaskCompletionSource<bool>();
            messageController.Finished += (sender, e) =>
            {
                messageController.DismissViewController(true, null);
                tcs.SetResult(e.Result == MessageComposeResult.Sent);
            };
            controller.PresentViewController(messageController, true, null);

            return tcs.Task;
        }
    }
}
