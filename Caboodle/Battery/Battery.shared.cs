using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Caboodle
{
    public static partial class Battery
    {
    }

    public enum BatteryState
    {
        Unknown,
        Charging,
        Discharging,
        Full,
        NotCharging
    }

    public enum BatteryPowerSource
    {
        Unknown,
        Battery,
        AC,
        USB,
        Wireless
    }

    public class BatteryChangedEventArgs : EventArgs
    {
        public BatteryChangedEventArgs(double level, BatteryState state, BatteryPowerSource source)
        {
            ChargeLevel = level;
            State = state;
            PowerSource = source;
        }

        public double ChargeLevel { get; }
        public BatteryState State { get; }
        public BatteryPowerSource PowerSource { get; }
    }
}