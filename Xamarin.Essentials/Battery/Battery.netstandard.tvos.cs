namespace Xamarin.Essentials
{
    public static partial class Battery
    {
        static void StartBatteryListeners() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static void StopBatteryListeners() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static double PlatformChargeLevel =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static BatteryState PlatformState =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static BatteryPowerSource PlatformPowerSource =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static void StartEnergySaverListeners() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static void StopEnergySaverListeners() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static EnergySaverStatus PlatformEnergySaverStatus =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
