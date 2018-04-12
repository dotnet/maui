namespace Xamarin.Essentials
{
    public static partial class Battery
    {
        static void StartBatteryListeners() =>
            DefaultBattery.ReportUpdated += ReportUpdated;

        static void StopBatteryListeners() =>
            DefaultBattery.ReportUpdated -= ReportUpdated;

        static void ReportUpdated(object sender, object e)
            => Platform.BeginInvokeOnMainThread(OnBatteryChanged);

        static Windows.Devices.Power.Battery DefaultBattery =>
            Windows.Devices.Power.Battery.AggregateBattery;

        public static double ChargeLevel
        {
            get
            {
                var finalReport = DefaultBattery.GetReport();
                var finalPercent = -1.0;

                var remaining = finalReport.RemainingCapacityInMilliwattHours;
                var full = finalReport.FullChargeCapacityInMilliwattHours;

                if (remaining.HasValue && full.HasValue)
                    finalPercent = (double)remaining.Value / (double)full.Value;

                return finalPercent;
            }
        }

        public static BatteryState State
        {
            get
            {
                var report = DefaultBattery.GetReport();

                switch (report.Status)
                {
                    case Windows.System.Power.BatteryStatus.Charging:
                        return BatteryState.Charging;
                    case Windows.System.Power.BatteryStatus.Discharging:
                        return BatteryState.Discharging;
                    case Windows.System.Power.BatteryStatus.Idle:
                        if (ChargeLevel >= 1.0)
                            return BatteryState.Full;
                        return BatteryState.NotCharging;
                    case Windows.System.Power.BatteryStatus.NotPresent:
                        return BatteryState.NotPresent;
                }

                if (ChargeLevel >= 1.0)
                    return BatteryState.Full;

                return BatteryState.Unknown;
            }
        }

        public static BatteryPowerSource PowerSource
        {
            get
            {
                switch (State)
                {
                    case BatteryState.Full:
                    case BatteryState.Charging:
                    case BatteryState.NotPresent:
                        return BatteryPowerSource.Ac;
                    case BatteryState.Unknown:
                        return BatteryPowerSource.Unknown;
                    default:
                        return BatteryPowerSource.Battery;
                }
            }
        }
    }
}
