using System;
using System.Numerics;

namespace Xamarin.Essentials
{
    public static partial class Gyroscope
    {
        static bool useSyncContext;

        public static event EventHandler<GyroscopeChangedEventArgs> ReadingChanged;

        public static bool IsMonitoring { get; private set; }

        public static void Start(SensorSpeed sensorSpeed)
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            if (IsMonitoring)
                return;

            IsMonitoring = true;
            useSyncContext = sensorSpeed == SensorSpeed.Normal || sensorSpeed == SensorSpeed.UI;

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
                MainThread.BeginInvokeOnMainThread(() => handler?.Invoke(null, e));
            else
                handler?.Invoke(null, e);
        }
    }

    public class GyroscopeChangedEventArgs : EventArgs
    {
        internal GyroscopeChangedEventArgs(GyroscopeData reading) => Reading = reading;

        public GyroscopeData Reading { get; }
    }

    public struct GyroscopeData
    {
        internal GyroscopeData(double x, double y, double z)
            : this((float)x, (float)y, (float)z)
        {
        }

        internal GyroscopeData(float x, float y, float z) =>
            AngularVelocity = new Vector3(x, y, z);

        public Vector3 AngularVelocity { get; }
    }
}
