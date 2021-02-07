using Foundation;
#if __IOS__
using UIKit;
#elif __WATCHOS__
using UIDevice = WatchKit.WKInterfaceDevice;
using UIDeviceBatteryState = WatchKit.WKInterfaceDeviceBatteryState;
#endif

namespace Xamarin.Essentials
{
	public static partial class Battery
	{
#if !__WATCHOS__
		static NSObject levelObserver;
		static NSObject stateObserver;
#endif

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
#if __WATCHOS__
            throw new FeatureNotSupportedException();
#else
			UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
			levelObserver = UIDevice.Notifications.ObserveBatteryLevelDidChange(BatteryInfoChangedNotification);
			stateObserver = UIDevice.Notifications.ObserveBatteryStateDidChange(BatteryInfoChangedNotification);
#endif
		}

		static void StopBatteryListeners()
		{
#if __WATCHOS__
            throw new FeatureNotSupportedException();
#else
			UIDevice.CurrentDevice.BatteryMonitoringEnabled = false;
			levelObserver?.Dispose();
			levelObserver = null;
			stateObserver?.Dispose();
			stateObserver = null;
#endif
		}

		static void BatteryInfoChangedNotification(object sender, NSNotificationEventArgs args)
			=> MainThread.BeginInvokeOnMainThread(OnBatteryInfoChanged);

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
