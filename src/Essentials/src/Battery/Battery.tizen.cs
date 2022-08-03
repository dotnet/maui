using System;
using Microsoft.Maui.ApplicationModel;
using TizenBattery = Tizen.System.Battery;

namespace Microsoft.Maui.Devices
{
	partial class BatteryImplementation : IBattery
	{
		void OnChanged(object sender, object e)
			=> MainThread.BeginInvokeOnMainThread(OnBatteryInfoChanged);

		void StartBatteryListeners()
		{
			TizenBattery.PercentChanged += OnChanged;
			TizenBattery.ChargingStateChanged += OnChanged;
			TizenBattery.LevelChanged += OnChanged;
		}

		void StopBatteryListeners()
		{
			TizenBattery.PercentChanged -= OnChanged;
			TizenBattery.ChargingStateChanged -= OnChanged;
			TizenBattery.LevelChanged -= OnChanged;
		}

		public double ChargeLevel
		{
			get
			{
				return (double)TizenBattery.Percent / 100;
			}
		}

		public BatteryState State
		{
			get
			{
				if (TizenBattery.IsCharging)
					return BatteryState.Charging;
				return BatteryState.Discharging;
			}
		}

		public BatteryPowerSource PowerSource
		{
			get
			{
				if (TizenBattery.IsCharging)
					return BatteryPowerSource.Usb;
				return BatteryPowerSource.Battery;
			}
		}

		void StartEnergySaverListeners()
			=> throw new FeatureNotSupportedException("This API is not currently supported on Tizen.");

		void StopEnergySaverListeners()
			=> throw new FeatureNotSupportedException("This API is not currently supported on Tizen.");

		public EnergySaverStatus EnergySaverStatus
			=> throw new FeatureNotSupportedException("This API is not currently supported on Tizen.");
	}
}
