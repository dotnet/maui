using System;

namespace Xamarin.Essentials
{
    public static partial class Gyroscope
    {
        static bool useSyncContext;

        public static event GyroscopeChangedEventHandler ReadingChanged;

        public static bool IsMonitoring { get; private set; }

        public static void Start(SensorSpeed sensorSpeed)
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            if (IsMonitoring)
                return;

            IsMonitoring = true;
            useSyncContext = sensorSpeed == SensorSpeed.Normal || sensorSpeed == SensorSpeed.Ui;

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
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            if (!IsMonitoring)
                return;

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

        internal static void OnChanged(GyroscopeData reading)
            => OnChanged(new GyroscopeChangedEventArgs(reading));

        internal static void OnChanged(GyroscopeChangedEventArgs e)
        {
            var handler = ReadingChanged;
            if (handler == null)
                return;

            if (useSyncContext)
                Platform.BeginInvokeOnMainThread(() => handler?.Invoke(e));
            else
                handler?.Invoke(e);
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
