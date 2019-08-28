using System;
using TizenBattery = Tizen.System.Battery;

namespace Xamarin.Essentials
{
    public static partial class Battery
    {
        static void OnChanged(object sender, object e)
            => MainThread.BeginInvokeOnMainThread(OnBatteryInfoChanged);

        static void StartBatteryListeners()
        {
            TizenBattery.PercentChanged += OnChanged;
            TizenBattery.ChargingStateChanged += OnChanged;
            TizenBattery.LevelChanged += OnChanged;
        }

        static void StopBatteryListeners()
        {
            TizenBattery.PercentChanged -= OnChanged;
            TizenBattery.ChargingStateChanged -= OnChanged;
            TizenBattery.LevelChanged -= OnChanged;
        }

        static double PlatformChargeLevel
        {
            get
            {
                return (double)TizenBattery.Percent / 100;
            }
        }

        static BatteryState PlatformState
        {
            get
            {
                if (TizenBattery.IsCharging)
                    return BatteryState.Charging;
                return BatteryState.Discharging;
            }
        }

        static BatteryPowerSource PlatformPowerSource
        {
            get
            {
                if (TizenBattery.IsCharging)
                    return BatteryPowerSource.Usb;
                return BatteryPowerSource.Battery;
            }
        }

        static void StartEnergySaverListeners()
            => throw new FeatureNotSupportedException("This API is not currently supported on Tizen.");

        static void StopEnergySaverListeners()
            => throw new FeatureNotSupportedException("This API is not currently supported on Tizen.");

        static EnergySaverStatus PlatformEnergySaverStatus
            => throw new FeatureNotSupportedException("This API is not currently supported on Tizen.");
    }
}
