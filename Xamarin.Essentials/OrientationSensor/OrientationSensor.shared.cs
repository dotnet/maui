using System;
using System.Numerics;

namespace Xamarin.Essentials
{
    public static partial class OrientationSensor
    {
        static bool useSyncContext;

        public static event EventHandler<OrientationSensorChangedEventArgs> ReadingChanged;

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

        internal static void OnChanged(OrientationSensorData reading)
            => OnChanged(new OrientationSensorChangedEventArgs(reading));

        internal static void OnChanged(OrientationSensorChangedEventArgs e)
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

    public class OrientationSensorChangedEventArgs : EventArgs
    {
        internal OrientationSensorChangedEventArgs(OrientationSensorData reading) => Reading = reading;

        public OrientationSensorData Reading { get; }
    }

    public struct OrientationSensorData
    {
        internal OrientationSensorData(double x, double y, double z, double w)
            : this((float)x, (float)y, (float)z, (float)w)
        {
        }

        internal OrientationSensorData(float x, float y, float z, float w)
        {
            Orientation = new Quaternion(x, y, z, w);
        }

        public Quaternion Orientation { get; }
    }
}
