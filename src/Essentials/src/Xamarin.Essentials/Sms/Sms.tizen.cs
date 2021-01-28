using System.Threading.Tasks;
using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class Sms
    {
        internal static bool IsComposeSupported
            => Platform.GetFeatureInfo<bool>("network.telephony.sms");

        static Task PlatformComposeAsync(SmsMessage message)
        {
            Permissions.EnsureDeclared<Permissions.LaunchApp>();

            var appControl = new AppControl
            {
                Operation = AppControlOperations.Compose,
                Uri = "sms:",
            };

            if (!string.IsNullOrEmpty(message.Body))
                appControl.ExtraData.Add(AppControlData.Text, message.Body);
            if (message.Recipients.Count > 0)
                appControl.Uri += string.Join(" ", message.Recipients);

            AppControl.SendLaunchRequest(appControl);

            return Task.CompletedTask;
        }
    }
}
