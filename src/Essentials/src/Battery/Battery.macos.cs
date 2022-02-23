using System;
using CoreFoundation;

namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class BatteryImplementation : IBattery
	{
		CFRunLoopSource powerSourceNotification;

		public void StartBatteryListeners()
		{
			powerSourceNotification = IOKit.CreatePowerSourceNotification(PowerSourceNotification);
			CFRunLoop.Current.AddSource(powerSourceNotification, CFRunLoop.ModeDefault);
		}

		public void StopBatteryListeners()
		{
			if (powerSourceNotification != null)
			{
				CFRunLoop.Current.RemoveSource(powerSourceNotification, CFRunLoop.ModeDefault);
				powerSourceNotification = null;
			}
		}

		public void PowerSourceNotification()
			=> MainThread.BeginInvokeOnMainThread(OnBatteryInfoChanged);

		public double ChargeLevel => IOKit.GetInternalBatteryChargeLevel();

		public BatteryState State => IOKit.GetInternalBatteryState();

		public BatteryPowerSource PowerSource => IOKit.GetProvidingPowerSource();

		public void StartEnergySaverListeners()
		{
		}

		public void StopEnergySaverListeners()
		{
		}

		public EnergySaverStatus EnergySaverStatus => EnergySaverStatus.Off;
	}
}
