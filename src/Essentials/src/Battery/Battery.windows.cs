using Microsoft.Maui.ApplicationModel;
using Windows.System.Power;

namespace Microsoft.Maui.Devices
{
	partial class BatteryImplementation : IBattery
	{
		void StartEnergySaverListeners() =>
			PowerManager.EnergySaverStatusChanged += ReportEnergySaverUpdated;

		void StopEnergySaverListeners() =>
			PowerManager.EnergySaverStatusChanged -= ReportEnergySaverUpdated;

		void ReportEnergySaverUpdated(object sender, object e)
			=> MainThread.BeginInvokeOnMainThread(OnEnergySaverChanged);

		public void StartBatteryListeners() =>
			DefaultBattery.ReportUpdated += ReportUpdated;

		public void StopBatteryListeners() =>
			DefaultBattery.ReportUpdated -= ReportUpdated;

		void ReportUpdated(object sender, object e)
			=> MainThread.BeginInvokeOnMainThread(OnBatteryInfoChanged);

		global::Windows.Devices.Power.Battery DefaultBattery =>
			global::Windows.Devices.Power.Battery.AggregateBattery;

		public double ChargeLevel
		{
			get
			{
				var finalReport = DefaultBattery.GetReport();
				var finalPercent = 1.0;

				var remaining = finalReport.RemainingCapacityInMilliwattHours;
				var full = finalReport.FullChargeCapacityInMilliwattHours;

				if (remaining.HasValue && full.HasValue)
					finalPercent = (double)remaining.Value / (double)full.Value;

				return finalPercent;
			}
		}

		public BatteryState State
		{
			get
			{
				var report = DefaultBattery.GetReport();

				switch (report.Status)
				{
					case BatteryStatus.Charging:
						return BatteryState.Charging;
					case BatteryStatus.Discharging:
						return BatteryState.Discharging;
					case BatteryStatus.Idle:
						if (ChargeLevel >= 1.0)
							return BatteryState.Full;
						return BatteryState.NotCharging;
					case BatteryStatus.NotPresent:
						return BatteryState.NotPresent;
				}

				if (ChargeLevel >= 1.0)
					return BatteryState.Full;

				return BatteryState.Unknown;
			}
		}

		public BatteryPowerSource PowerSource
		{
			get
			{
				switch (State)
				{
					case BatteryState.Full:
					case BatteryState.Charging:
					case BatteryState.NotPresent:
						return BatteryPowerSource.AC;
					case BatteryState.Unknown:
						return BatteryPowerSource.Unknown;
					default:
						return BatteryPowerSource.Battery;
				}
			}
		}

		public EnergySaverStatus EnergySaverStatus =>
			PowerManager.EnergySaverStatus == global::Windows.System.Power.EnergySaverStatus.On ? EnergySaverStatus.On : EnergySaverStatus.Off;
	}
}
