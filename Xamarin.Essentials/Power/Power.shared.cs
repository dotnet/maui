using System;

namespace Xamarin.Essentials
{
    public static partial class Power
    {
        static event EventHandler<EnergySaverStatusChanagedEventArgs> EnergySaverStatusChanagedInternal;

        public static EnergySaverStatus EnergySaverStatus => PlatformEnergySaverStatus;

        public static event EventHandler<EnergySaverStatusChanagedEventArgs> EnergySaverStatusChanaged
        {
            add
            {
                var wasRunning = EnergySaverStatusChanagedInternal != null;

                EnergySaverStatusChanagedInternal += value;

                if (!wasRunning && EnergySaverStatusChanagedInternal != null)
                    StartPowerListeners();
            }

            remove
            {
                var wasRunning = EnergySaverStatusChanagedInternal != null;

                EnergySaverStatusChanagedInternal -= value;

                if (wasRunning && EnergySaverStatusChanagedInternal == null)
                    StopPowerListeners();
            }
        }

        static void OnPowerChanged()
            => OnPowerChanged(EnergySaverStatus);

        static void OnPowerChanged(EnergySaverStatus saverStatus)
            => OnPowerChanged(new EnergySaverStatusChanagedEventArgs(saverStatus));

        static void OnPowerChanged(EnergySaverStatusChanagedEventArgs e)
            => EnergySaverStatusChanagedInternal?.Invoke(null, e);
    }

    public enum EnergySaverStatus
    {
        Unknown,
        On,
        Off
    }

    public class EnergySaverStatusChanagedEventArgs : EventArgs
    {
        internal EnergySaverStatusChanagedEventArgs(EnergySaverStatus saverStatus)
        {
            EnergySaverStatus = saverStatus;
        }

        public EnergySaverStatus EnergySaverStatus { get; }
    }
}
