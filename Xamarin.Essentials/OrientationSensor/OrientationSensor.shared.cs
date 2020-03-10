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
                throw new InvalidOperationException("Orientation sensor has already been started.");

            IsMonitoring = true;
            useSyncContext = sensorSpeed == SensorSpeed.Default || sensorSpeed == SensorSpeed.UI;

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

        internal static void OnChanged(OrientationSensorData reading) =>
            OnChanged(new OrientationSensorChangedEventArgs(reading));

        internal static void OnChanged(OrientationSensorChangedEventArgs e)
        {
            if (useSyncContext)
                MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
            else
                ReadingChanged?.Invoke(null, e);
        }
    }

    public class OrientationSensorChangedEventArgs : EventArgs
    {
        public OrientationSensorChangedEventArgs(OrientationSensorData reading) =>
            Reading = reading;

        public OrientationSensorData Reading { get; }
    }

    public readonly struct OrientationSensorData : IEquatable<OrientationSensorData>
    {
        public OrientationSensorData(double x, double y, double z, double w)
            : this((float)x, (float)y, (float)z, (float)w)
        {
        }

        public OrientationSensorData(float x, float y, float z, float w) =>
            Orientation = new Quaternion(x, y, z, w);

        public Quaternion Orientation { get; }

        public override bool Equals(object obj) =>
            (obj is OrientationSensorData data) && Equals(data);

        public bool Equals(OrientationSensorData other) =>
            Orientation.Equals(other.Orientation);

        public static bool operator ==(OrientationSensorData left, OrientationSensorData right) =>
            left.Equals(right);

        public static bool operator !=(OrientationSensorData left, OrientationSensorData right) =>
           !left.Equals(right);

        public override int GetHashCode() =>
            Orientation.GetHashCode();

        public override string ToString() =>
            $"{nameof(Orientation.X)}: {Orientation.X}, " +
            $"{nameof(Orientation.Y)}: {Orientation.Y}, " +
            $"{nameof(Orientation.Z)}: {Orientation.Z}, " +
            $"{nameof(Orientation.W)}: {Orientation.W}";
    }
}
