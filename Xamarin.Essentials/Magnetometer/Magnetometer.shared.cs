using System;
using System.Numerics;

namespace Xamarin.Essentials
{
    public static partial class Magnetometer
    {
        static bool useSyncContext;

        public static event EventHandler<MagnetometerChangedEventArgs> ReadingChanged;

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

        internal static void OnChanged(MagnetometerData reading) =>
            OnChanged(new MagnetometerChangedEventArgs(reading));

        internal static void OnChanged(MagnetometerChangedEventArgs e)
        {
            if (useSyncContext)
                MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
            else
                ReadingChanged?.Invoke(null, e);
        }
    }

    public class MagnetometerChangedEventArgs : EventArgs
    {
        internal MagnetometerChangedEventArgs(MagnetometerData reading) =>
            Reading = reading;

        public MagnetometerData Reading { get; }
    }

    public readonly struct MagnetometerData : IEquatable<MagnetometerData>
    {
        internal MagnetometerData(double x, double y, double z)
            : this((float)x, (float)y, (float)z)
        {
        }

        internal MagnetometerData(float x, float y, float z) =>
            MagneticField = new Vector3(x, y, z);

        public Vector3 MagneticField { get; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is MagnetometerData compassData))
                return false;
            return Equals(compassData);
        }

        public bool Equals(MagnetometerData other) => MagneticField.Equals(other.MagneticField);

        public static bool operator ==(MagnetometerData left, MagnetometerData right) =>
            Equals(left, right);

        public static bool operator !=(MagnetometerData left, MagnetometerData right) =>
           !Equals(left, right);

        public override int GetHashCode() => MagneticField.GetHashCode();
    }
}
