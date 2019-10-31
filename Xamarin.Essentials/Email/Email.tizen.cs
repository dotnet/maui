using System.Threading.Tasks;
using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class Email
    {
        internal static bool IsComposeSupported
            => Platform.GetFeatureInfo<bool>("email");

        static Task PlatformComposeAsync(EmailMessage message)
        {
            Permissions.EnsureDeclared(PermissionType.LaunchApp);

            var appControl = new AppControl
            {
                Operation = AppControlOperations.Compose,
                Uri = "mailto:",
            };

            if (message.Bcc.Count > 0)
                appControl.ExtraData.Add("http://tizen.org/appcontrol/data/bcc", message.Bcc);
            if (!string.IsNullOrEmpty(message.Body))
                appControl.ExtraData.Add("http://tizen.org/appcontrol/data/text", message.Body);
            if (message.Cc.Count > 0)
                appControl.ExtraData.Add("http://tizen.org/appcontrol/data/cc", message.Cc);
            if (!string.IsNullOrEmpty(message.Subject))
                appControl.ExtraData.Add("http://tizen.org/appcontrol/data/subject", message.Subject);
            if (message.To.Count > 0)
                appControl.ExtraData.Add("http://tizen.org/appcontrol/data/to", message.To);

            AppControl.SendLaunchRequest(appControl);

            return Task.CompletedTask;
        }
    }
}
