namespace Microsoft.Caboodle
{
    public static partial class Battery
    {
        public static double ChargeLevel { get; } = -1;
        public static BatteryState State { get; } = BatteryState.Unknown;
        public static BatteryPowerSource PowerSource { get; } = BatteryPowerSource.Unknown;
    }
}