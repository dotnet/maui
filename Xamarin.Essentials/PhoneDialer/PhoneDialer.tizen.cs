using Tizen.Applications;

namespace Xamarin.Essentials
{
    public static partial class PhoneDialer
    {
        internal static bool IsSupported
            => Platform.GetFeatureInfo<bool>("contact");

        static void PlatformOpen(string number)
        {
            Permissions.EnsureDeclared(PermissionType.LaunchApp);

            var appControl = new AppControl
            {
                Operation = AppControlOperations.Dial,
                Uri = "tel:",
            };

            if (!string.IsNullOrEmpty(number))
                appControl.Uri += number;

            AppControl.SendLaunchRequest(appControl);
        }
    }
}
