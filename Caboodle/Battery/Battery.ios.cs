using Foundation;
using UIKit;

namespace Microsoft.Caboodle
{
    public static partial class Battery
    {
        public static double ChargeLevel
        {
            get
            {
                try
                {
                    UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
                    return UIDevice.CurrentDevice.BatteryLevel;
                }
                finally
                {
                    UIDevice.CurrentDevice.BatteryMonitoringEnabled = false;
                }
            }
        }

        public static BatteryState State
        {
            get
            {
                try
                {
                    UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
                    switch (UIDevice.CurrentDevice.BatteryState)
                    {
                        case UIDeviceBatteryState.Charging:
                            return BatteryState.Charging;
                        case UIDeviceBatteryState.Full:
                            return BatteryState.Full;
                        case UIDeviceBatteryState.Unplugged:
                            return BatteryState.Discharging;
                        default:
                            return BatteryState.Unknown;
                    }
                }
                finally
                {
                    UIDevice.CurrentDevice.BatteryMonitoringEnabled = false;
                }
            }
        }

        public static BatteryPowerSource PowerSource
        {
            get
            {
                try
                {
                    UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
                    switch (UIDevice.CurrentDevice.BatteryState)
                    {
                        case UIDeviceBatteryState.Full:
                        case UIDeviceBatteryState.Charging:
                            return BatteryPowerSource.AC;
                        case UIDeviceBatteryState.Unplugged:
                            return BatteryPowerSource.Battery;
                        default:
                            return BatteryPowerSource.Unknown;
                    }
                }
                finally
                {
                    UIDevice.CurrentDevice.BatteryMonitoringEnabled = false;
                }
            }
        }
    }
}