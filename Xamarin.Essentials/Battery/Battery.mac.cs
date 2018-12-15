namespace Xamarin.Essentials
{
    public static partial class Battery
    {
        static void StartBatteryListeners() =>
            throw new System.PlatformNotSupportedException();

        static void StopBatteryListeners() =>
            throw new System.PlatformNotSupportedException();

        static double PlatformChargeLevel =>
            throw new System.PlatformNotSupportedException();

        static BatteryState PlatformState =>
            throw new System.PlatformNotSupportedException();

        static BatteryPowerSource PlatformPowerSource =>
            throw new System.PlatformNotSupportedException();

        static void StartEnergySaverListeners() =>
            throw new System.PlatformNotSupportedException();

        static void StopEnergySaverListeners() =>
            throw new System.PlatformNotSupportedException();

        static EnergySaverStatus PlatformEnergySaverStatus =>
            throw new System.PlatformNotSupportedException();
    }
}
