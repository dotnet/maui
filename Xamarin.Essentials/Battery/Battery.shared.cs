using System;

namespace Xamarin.Essentials
{
    public static partial class Battery
    {
        static event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChangedInternal;

        static event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChangedInternal;

        // a cache so that events aren't fired unnecessarily
        // this is mainly an issue on Android, but we can stiil do this everywhere
        static double currentLevel;
        static BatteryPowerSource currentSource;
        static BatteryState currentState;

        public static double ChargeLevel => PlatformChargeLevel;

        public static BatteryState State => PlatformState;

        public static BatteryPowerSource PowerSource => PlatformPowerSource;

        public static EnergySaverStatus EnergySaverStatus => PlatformEnergySaverStatus;

        public static event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChanged
        {
            add
            {
                var wasRunning = BatteryInfoChangedInternal != null;

                BatteryInfoChangedInternal += value;

                if (!wasRunning && BatteryInfoChangedInternal != null)
                {
                    SetCurrent();
                    StartBatteryListeners();
                }
            }

            remove
            {
                var wasRunning = BatteryInfoChangedInternal != null;

                BatteryInfoChangedInternal -= value;

                if (wasRunning && BatteryInfoChangedInternal == null)
                    StopBatteryListeners();
            }
        }

        public static event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged
        {
            add
            {
                var wasRunning = EnergySaverStatusChangedInternal != null;

                EnergySaverStatusChangedInternal += value;

                if (!wasRunning && EnergySaverStatusChangedInternal != null)
                    StartEnergySaverListeners();
            }

            remove
            {
                var wasRunning = EnergySaverStatusChangedInternal != null;

                EnergySaverStatusChangedInternal -= value;

                if (wasRunning && EnergySaverStatusChangedInternal == null)
                    StopEnergySaverListeners();
            }
        }

        static void SetCurrent()
        {
            currentLevel = Battery.ChargeLevel;
            currentSource = Battery.PowerSource;
            currentState = Battery.State;
        }

        static void OnBatteryInfoChanged(double level, BatteryState state, BatteryPowerSource source)
            => OnBatteryInfoChanged(new BatteryInfoChangedEventArgs(level, state, source));

        static void OnBatteryInfoChanged()
            => OnBatteryInfoChanged(ChargeLevel, State, PowerSource);

        static void OnBatteryInfoChanged(BatteryInfoChangedEventArgs e)
        {
            if (currentLevel != e.ChargeLevel || currentSource != e.PowerSource || currentState != e.State)
            {
                SetCurrent();
                BatteryInfoChangedInternal?.Invoke(null, e);
            }
        }

        static void OnEnergySaverChanged()
            => OnEnergySaverChanged(EnergySaverStatus);

        static void OnEnergySaverChanged(EnergySaverStatus saverStatus)
            => OnEnergySaverChanged(new EnergySaverStatusChangedEventArgs(saverStatus));

        static void OnEnergySaverChanged(EnergySaverStatusChangedEventArgs e)
            => EnergySaverStatusChangedInternal?.Invoke(null, e);
    }

    public enum BatteryState
    {
        Unknown = 0,
        Charging = 1,
        Discharging = 2,
        Full = 3,
        NotCharging = 4,
        NotPresent = 5
    }

    public enum BatteryPowerSource
    {
        Unknown = 0,
        Battery = 1,
        AC = 2,
        Usb = 3,
        Wireless = 4
    }

    public enum EnergySaverStatus
    {
        Unknown = 0,
        On = 1,
        Off = 2
    }

    public class BatteryInfoChangedEventArgs : EventArgs
    {
        public BatteryInfoChangedEventArgs(double level, BatteryState state, BatteryPowerSource source)
        {
            ChargeLevel = level;
            State = state;
            PowerSource = source;
        }

        public double ChargeLevel { get; }

        public BatteryState State { get; }

        public BatteryPowerSource PowerSource { get; }

        public override string ToString() =>
            $"{nameof(ChargeLevel)}: {ChargeLevel.ToString()}, " +
            $"{nameof(State)}: {State}, " +
            $"{nameof(PowerSource)}: {PowerSource}";
    }

    public class EnergySaverStatusChangedEventArgs : EventArgs
    {
        public EnergySaverStatusChangedEventArgs(EnergySaverStatus saverStatus)
        {
            EnergySaverStatus = saverStatus;
        }

        public EnergySaverStatus EnergySaverStatus { get; }

        public override string ToString() =>
            $"{nameof(EnergySaverStatus)}: {EnergySaverStatus}";
    }
}
