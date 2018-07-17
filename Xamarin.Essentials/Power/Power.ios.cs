using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Power
    {
        static NSObject saverStatusObserver;

        static void StartPowerListeners()
        {
            saverStatusObserver = NSNotificationCenter.DefaultCenter.AddObserver(NSProcessInfo.PowerStateDidChangeNotification, PowerChangedNotification);
        }

        static void StopPowerListeners()
        {
            saverStatusObserver?.Dispose();
            saverStatusObserver = null;
        }

        static void PowerChangedNotification(NSNotification notification)
            => MainThread.BeginInvokeOnMainThread(OnPowerChanged);

        static EnergySaverStatus PlatformEnergySaverStatus =>
            NSProcessInfo.ProcessInfo?.LowPowerModeEnabled == true ? EnergySaverStatus.On : EnergySaverStatus.Off;
    }
}
