using Windows.System.Power;

namespace Xamarin.Essentials
{
    public static partial class Power
    {
        static void StartPowerListeners()
        {
            PowerManager.EnergySaverStatusChanged += ReportUpdated;
        }

        static void StopPowerListeners()
        {
            PowerManager.EnergySaverStatusChanged -= ReportUpdated;
        }

        static void ReportUpdated(object sender, object e)
            => MainThread.BeginInvokeOnMainThread(OnPowerChanged);

        static EnergySaverStatus PlatformEnergySaverStatus =>
            PowerManager.EnergySaverStatus == Windows.System.Power.EnergySaverStatus.On ? EnergySaverStatus.On : EnergySaverStatus.Off;
    }
}
