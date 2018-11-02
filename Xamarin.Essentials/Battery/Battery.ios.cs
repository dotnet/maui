using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Battery
    {
        static NSObject levelObserver;
        static NSObject stateObserver;
        static NSObject saverStatusObserver;

        static void StartEnergySaverListeners()
        {
            saverStatusObserver = NSNotificationCenter.DefaultCenter.AddObserver(NSProcessInfo.PowerStateDidChangeNotification, PowerChangedNotification);
        }

        static void StopEnergySaverListeners()
        {
            saverStatusObserver?.Dispose();
            saverStatusObserver = null;
        }

        static void PowerChangedNotification(NSNotification notification)
            => MainThread.BeginInvokeOnMainThread(OnEnergySaverChanged);

        static EnergySaverStatus PlatformEnergySaverStatus =>
            NSProcessInfo.ProcessInfo?.LowPowerModeEnabled == true ? EnergySaverStatus.On : EnergySaverStatus.Off;

        static void StartBatteryListeners()
        {
            UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
            levelObserver = UIDevice.Notifications.ObserveBatteryLevelDidChange(BatteryChangedNotification);
            stateObserver = UIDevice.Notifications.ObserveBatteryStateDidChange(BatteryChangedNotification);
        }

        static void StopBatteryListeners()
        {
            UIDevice.CurrentDevice.BatteryMonitoringEnabled = false;
            levelObserver?.Dispose();
            levelObserver = null;
            stateObserver?.Dispose();
            stateObserver = null;
        }

        static void BatteryChangedNotification(object sender, NSNotificationEventArgs args)
            => MainThread.BeginInvokeOnMainThread(OnBatteryChanged);

        static double PlatformChargeLevel
        {
            get
            {
                var batteryMonitoringEnabled = UIDevice.CurrentDevice.BatteryMonitoringEnabled;
                UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
                try
                {
                    return UIDevice.CurrentDevice.BatteryLevel;
                }
                finally
                {
                    UIDevice.CurrentDevice.BatteryMonitoringEnabled = batteryMonitoringEnabled;
                }
            }
        }

        static BatteryState PlatformState
        {
            get
            {
                var batteryMonitoringEnabled = UIDevice.CurrentDevice.BatteryMonitoringEnabled;
                UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
                try
                {
                    switch (UIDevice.CurrentDevice.BatteryState)
                    {
                        case UIDeviceBatteryState.Charging:
                            return BatteryState.Charging;
                        case UIDeviceBatteryState.Full:
                            return BatteryState.Full;
                        case UIDeviceBatteryState.Unplugged:
                            return BatteryState.Discharging;
                        default:
                            if (ChargeLevel >= 1.0)
                                return BatteryState.Full;
                            return BatteryState.Unknown;
                    }
                }
                finally
                {
                    UIDevice.CurrentDevice.BatteryMonitoringEnabled = batteryMonitoringEnabled;
                }
            }
        }

        static BatteryPowerSource PlatformPowerSource
        {
            get
            {
                var batteryMonitoringEnabled = UIDevice.CurrentDevice.BatteryMonitoringEnabled;
                UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
                try
                {
                    switch (UIDevice.CurrentDevice.BatteryState)
                    {
                        case UIDeviceBatteryState.Full:
                        case UIDeviceBatteryState.Charging:
                            return BatteryPowerSource.AC;
                        case UIDeviceBatteryState.Unplugged:
                            return BatteryPowerSource.Battery;
                        default:
                            return BatteryPowerSource.Unknown;
                    }
                }
                finally
                {
                    UIDevice.CurrentDevice.BatteryMonitoringEnabled = batteryMonitoringEnabled;
                }
            }
        }
    }
}
