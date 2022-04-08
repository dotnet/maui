using System;
using CoreFoundation;

namespace Microsoft.Maui.Devices
{
	partial class BatteryImplementation : IBattery
	{
		CFRunLoopSource powerSourceNotification;

		void StartBatteryListeners()
		{
			powerSourceNotification = IOKit.CreatePowerSourceNotification(PowerSourceNotification);
			CFRunLoop.Current.AddSource(powerSourceNotification, CFRunLoop.ModeDefault);
		}

		void StopBatteryListeners()
		{
			if (powerSourceNotification != null)
			{
				CFRunLoop.Current.RemoveSource(powerSourceNotification, CFRunLoop.ModeDefault);
				powerSourceNotification = null;
			}
		}

		void PowerSourceNotification()
			=> MainThread.BeginInvokeOnMainThread(OnBatteryInfoChanged);

		public double ChargeLevel => IOKit.GetInternalBatteryChargeLevel();

		public BatteryState State => IOKit.GetInternalBatteryState();

		public BatteryPowerSource PowerSource => IOKit.GetProvidingPowerSource();

		void StartEnergySaverListeners()
		{
		}

		void StopEnergySaverListeners()
		{
		}

		public EnergySaverStatus EnergySaverStatus => EnergySaverStatus.Off;
	}
}
