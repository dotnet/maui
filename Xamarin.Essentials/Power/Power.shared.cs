using System;

namespace Xamarin.Essentials
{
    public static partial class Power
    {
        static event EnergySaverStatusChanagedEventHandler EnergySaverStatusChanagedInternal;

        public static EnergySaverStatus EnergySaverStatus => PlatformEnergySaverStatus;

        public static event EnergySaverStatusChanagedEventHandler EnergySaverStatusChanaged
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
            => EnergySaverStatusChanagedInternal?.Invoke(e);
    }

    public enum EnergySaverStatus
    {
        Unknown,
        On,
        Off
    }

    public delegate void EnergySaverStatusChanagedEventHandler(EnergySaverStatusChanagedEventArgs e);

    public class EnergySaverStatusChanagedEventArgs : EventArgs
    {
        internal EnergySaverStatusChanagedEventArgs(EnergySaverStatus saverStatus)
        {
            EnergySaverStatus = saverStatus;
        }

        public EnergySaverStatus EnergySaverStatus { get; }
    }
}
