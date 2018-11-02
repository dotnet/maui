using System;

namespace Xamarin.Essentials
{
    public static partial class Battery
    {
        static event EventHandler<BatteryChangedEventArgs> BatteryChangedInternal;

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

        public static event EventHandler<BatteryChangedEventArgs> BatteryChanged
        {
            add
            {
                var wasRunning = BatteryChangedInternal != null;

                BatteryChangedInternal += value;

                if (!wasRunning && BatteryChangedInternal != null)
                {
                    SetCurrent();
                    StartBatteryListeners();
                }
            }

            remove
            {
                var wasRunning = BatteryChangedInternal != null;

                BatteryChangedInternal -= value;

                if (wasRunning && BatteryChangedInternal == null)
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

        static void OnBatteryChanged(double level, BatteryState state, BatteryPowerSource source)
            => OnBatteryChanged(new BatteryChangedEventArgs(level, state, source));

        static void OnBatteryChanged()
            => OnBatteryChanged(ChargeLevel, State, PowerSource);

        static void OnBatteryChanged(BatteryChangedEventArgs e)
        {
            if (currentLevel != e.ChargeLevel || currentSource != e.PowerSource || currentState != e.State)
            {
                SetCurrent();
                BatteryChangedInternal?.Invoke(null, e);
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
        Unknown,
        Charging,
        Discharging,
        Full,
        NotCharging,
        NotPresent
    }

    public enum BatteryPowerSource
    {
        Unknown,
        Battery,
        AC,
        Usb,
        Wireless
    }

    public enum EnergySaverStatus
    {
        Unknown,
        On,
        Off
    }

    public class BatteryChangedEventArgs : EventArgs
    {
        internal BatteryChangedEventArgs(double level, BatteryState state, BatteryPowerSource source)
        {
            ChargeLevel = level;
            State = state;
            PowerSource = source;
        }

        public double ChargeLevel { get; }

        public BatteryState State { get; }

        public BatteryPowerSource PowerSource { get; }

        public override string ToString() =>
            $"{nameof(ChargeLevel)}: {ChargeLevel}, " +
            $"{nameof(State)}: {State}, " +
            $"{nameof(PowerSource)}: {PowerSource}";
    }

    public class EnergySaverStatusChangedEventArgs : EventArgs
    {
        internal EnergySaverStatusChangedEventArgs(EnergySaverStatus saverStatus)
        {
            EnergySaverStatus = saverStatus;
        }

        public EnergySaverStatus EnergySaverStatus { get; }

        public override string ToString() =>
            $"{nameof(EnergySaverStatus)}: {EnergySaverStatus}";
    }
}
