using System;
using Windows.Phone.Devices.Notification;

namespace Microsoft.Maui.Essentials
{
    public static partial class Vibration
    {
        internal static bool IsSupported
            => Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.Devices.Notification.VibrationDevice") && DefaultDevice != null;

        static VibrationDevice DefaultDevice => VibrationDevice.GetDefault();

        static void PlatformVibrate(TimeSpan duration) =>
            DefaultDevice.Vibrate(duration);

        static void PlatformCancel() =>
            DefaultDevice.Cancel();
    }
}
