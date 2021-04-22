using Windows.System.Power;

namespace Microsoft.Maui.Essentials
{
	public static partial class Battery
	{
		static void StartEnergySaverListeners() =>
			PowerManager.EnergySaverStatusChanged += ReportEnergySaverUpdated;

		static void StopEnergySaverListeners() =>
			PowerManager.EnergySaverStatusChanged -= ReportEnergySaverUpdated;

		static void ReportEnergySaverUpdated(object sender, object e)
			=> MainThread.BeginInvokeOnMainThread(OnEnergySaverChanged);

		static void StartBatteryListeners() =>
			DefaultBattery.ReportUpdated += ReportUpdated;

		static void StopBatteryListeners() =>
			DefaultBattery.ReportUpdated -= ReportUpdated;

		static void ReportUpdated(object sender, object e)
			=> MainThread.BeginInvokeOnMainThread(OnBatteryInfoChanged);

		static Windows.Devices.Power.Battery DefaultBattery =>
			Windows.Devices.Power.Battery.AggregateBattery;

		static double PlatformChargeLevel
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

		static BatteryState PlatformState
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

		static BatteryPowerSource PlatformPowerSource
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

		static EnergySaverStatus PlatformEnergySaverStatus =>
			PowerManager.EnergySaverStatus == Windows.System.Power.EnergySaverStatus.On ? EnergySaverStatus.On : EnergySaverStatus.Off;
	}
}
