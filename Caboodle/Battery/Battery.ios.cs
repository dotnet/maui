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
                var batteryMonitoringEnabled = UIDevice.CurrentDevice.BatteryMonitoringEnabled;
                UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
                try
                {
                    return UIDevice.CurrentDevice.BatteryLevel;
                }
                finally
                {
                    UIDevice.CurrentDevice.BatteryMonitoringEnabled = batteryMonitoringEnabled;
                }
            }
        }

        public static BatteryState State
        {
            get
            {
                var batteryMonitoringEnabled = UIDevice.CurrentDevice.BatteryMonitoringEnabled;
                UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
                try
                {
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
                    UIDevice.CurrentDevice.BatteryMonitoringEnabled = batteryMonitoringEnabled;
                }
            }
        }

        public static BatteryPowerSource PowerSource
        {
            get
            {
                var batteryMonitoringEnabled = UIDevice.CurrentDevice.BatteryMonitoringEnabled;
                UIDevice.CurrentDevice.BatteryMonitoringEnabled = true;
                try
                {
                    switch (UIDevice.CurrentDevice.BatteryState)
                    {
                        case UIDeviceBatteryState.Full:
                        case UIDeviceBatteryState.Charging:
                            return BatteryPowerSource.Ac;
                        case UIDeviceBatteryState.Unplugged:
                            return BatteryPowerSource.Battery;
                        default:
                            return BatteryPowerSource.Unknown;
                    }
                }
                finally
                {
                    UIDevice.CurrentDevice.BatteryMonitoringEnabled = batteryMonitoringEnabled;
                }
            }
        }
    }
}