using System;

namespace Xamarin.Essentials
{
    public static partial class Power
    {
        static event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChangedInternal;

        public static EnergySaverStatus EnergySaverStatus => PlatformEnergySaverStatus;

        public static event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged
        {
            add
            {
                var wasRunning = EnergySaverStatusChangedInternal != null;

                EnergySaverStatusChangedInternal += value;

                if (!wasRunning && EnergySaverStatusChangedInternal != null)
                    StartPowerListeners();
            }

            remove
            {
                var wasRunning = EnergySaverStatusChangedInternal != null;

                EnergySaverStatusChangedInternal -= value;

                if (wasRunning && EnergySaverStatusChangedInternal == null)
                    StopPowerListeners();
            }
        }

        static void OnPowerChanged()
            => OnPowerChanged(EnergySaverStatus);

        static void OnPowerChanged(EnergySaverStatus saverStatus)
            => OnPowerChanged(new EnergySaverStatusChangedEventArgs(saverStatus));

        static void OnPowerChanged(EnergySaverStatusChangedEventArgs e)
            => EnergySaverStatusChangedInternal?.Invoke(null, e);
    }

    public enum EnergySaverStatus
    {
        Unknown,
        On,
        Off
    }

    public class EnergySaverStatusChangedEventArgs : EventArgs
    {
        internal EnergySaverStatusChangedEventArgs(EnergySaverStatus saverStatus)
        {
            EnergySaverStatus = saverStatus;
        }

        public EnergySaverStatus EnergySaverStatus { get; }
    }
}
