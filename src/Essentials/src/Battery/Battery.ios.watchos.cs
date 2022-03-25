using Foundation;
using Microsoft.Maui.ApplicationModel;
#if __IOS__
using ObjCRuntime;
using UIKit;
#elif __WATCHOS__
using UIDevice = WatchKit.WKInterfaceDevice;
using UIDeviceBatteryState = WatchKit.WKInterfaceDeviceBatteryState;
#endif

namespace Microsoft.Maui.Devices
{
	partial class BatteryImplementation : IBattery
	{
#if !__WATCHOS__
		NSObject levelObserver;
		NSObject stateObserver;
#endif

		NSObject saverStatusObserver;

		void StartEnergySaverListeners()
		{
			saverStatusObserver = NSNotificationCenter.DefaultCenter.AddObserver(NSProcessInfo.PowerStateDidChangeNotification, PowerChangedNotification);
		}

		void StopEnergySaverListeners()
		{
			saverStatusObserver?.Dispose();
			saverStatusObserver = null;
		}

		void PowerChangedNotification(NSNotification notification)
			=> PlatformUtils.BeginInvokeOnMainThread(OnEnergySaverChanged);

		public EnergySaverStatus EnergySaverStatus =>
			NSProcessInfo.ProcessInfo?.LowPowerModeEnabled == true ? EnergySaverStatus.On : EnergySaverStatus.Off;

		void StartBatteryListeners()
		{
#if __WATCHOS__
			throw new FeatureNotSupportedException();
#else
			UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
			levelObserver = UIDevice.Notifications.ObserveBatteryLevelDidChange(BatteryInfoChangedNotification);
			stateObserver = UIDevice.Notifications.ObserveBatteryStateDidChange(BatteryInfoChangedNotification);
#endif
		}

		void StopBatteryListeners()
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

		void BatteryInfoChangedNotification(object sender, NSNotificationEventArgs args)
			=> PlatformUtils.BeginInvokeOnMainThread(OnBatteryInfoChanged);

		public double ChargeLevel
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

		public BatteryState State
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

		public BatteryPowerSource PowerSource
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
