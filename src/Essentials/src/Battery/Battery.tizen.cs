using System;
using TizenBattery = Tizen.System.Battery;

namespace Microsoft.Maui.Devices
{
	public partial class BatteryImplementation : IBattery
	{
		void OnChanged(object sender, object e)
			=> MainThread.BeginInvokeOnMainThread(OnBatteryInfoChanged);

		public void StartBatteryListeners()
		{
			TizenBattery.PercentChanged += OnChanged;
			TizenBattery.ChargingStateChanged += OnChanged;
			TizenBattery.LevelChanged += OnChanged;
		}

		public void StopBatteryListeners()
		{
			TizenBattery.PercentChanged -= OnChanged;
			TizenBattery.ChargingStateChanged -= OnChanged;
			TizenBattery.LevelChanged -= OnChanged;
		}

		public double PlatformChargeLevel
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

		public void StartEnergySaverListeners()
			=> throw new FeatureNotSupportedException("This API is not currently supported on Tizen.");

		public void StopEnergySaverListeners()
			=> throw new FeatureNotSupportedException("This API is not currently supported on Tizen.");

		public EnergySaverStatus EnergySaverStatus
			=> throw new FeatureNotSupportedException("This API is not currently supported on Tizen.");
	}
}
