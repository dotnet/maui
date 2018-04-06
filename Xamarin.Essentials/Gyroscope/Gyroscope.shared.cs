using System;

namespace Xamarin.Essentials
{
    public static partial class Gyroscope
    {
        public static event GyroscopeChangedEventHandler ReadingChanged;

        public static bool IsMonitoring { get; private set; }

        public static void Start(SensorSpeed sensorSpeed)
        {
            if (!IsSupported)
            {
                throw new FeatureNotSupportedException();
            }

            if (IsMonitoring)
            {
                return;
            }

            IsMonitoring = true;

            UseSyncContext = sensorSpeed == SensorSpeed.Normal || sensorSpeed == SensorSpeed.Ui;
            try
            {
                PlatformStart(sensorSpeed);
            }
            catch
            {
                IsMonitoring = false;
                throw;
            }
        }

        public static void Stop()
        {
            if (!IsMonitoring)
            {
                return;
            }

            IsMonitoring = false;

            try
            {
                PlatformStop();
            }
            catch
            {
                IsMonitoring = true;
                throw;
            }
        }

        internal static bool UseSyncContext { get; set; }

        internal static void OnChanged(GyroscopeData reading)
            => OnChanged(new GyroscopeChangedEventArgs(reading));

        internal static void OnChanged(GyroscopeChangedEventArgs e)
        {
            if (ReadingChanged == null)
                return;

            if (UseSyncContext)
            {
                Platform.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(e));
            }
            else
            {
                ReadingChanged?.Invoke(e);
            }
        }
    }

    public delegate void GyroscopeChangedEventHandler(GyroscopeChangedEventArgs e);

    public class GyroscopeChangedEventArgs : EventArgs
    {
        internal GyroscopeChangedEventArgs(GyroscopeData reading) => Reading = reading;

        public GyroscopeData Reading { get; }
    }

    public struct GyroscopeData
    {
        internal GyroscopeData(double x, double y, double z)
        {
            AngularVelocityX = x;
            AngularVelocityY = y;
            AngularVelocityZ = z;
        }

        public double AngularVelocityX { get; }

        public double AngularVelocityY { get; }

        public double AngularVelocityZ { get; }
    }
}
