using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Caboodle
{
    public static partial class Battery
    {
        public static double ChargeLevel => UIDevice.CurrentDevice.BatteryLevel;
        public static BatteryState State
        {
            get
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
        }
        public static BatteryPowerSource PowerSource
        {
            get
            {
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
        }
    }
}