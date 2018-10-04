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

        internal static void OnChanged(GyroscopeData reading) =>
            OnChanged(new GyroscopeChangedEventArgs(reading));

        internal static void OnChanged(GyroscopeChangedEventArgs e)
        {
            if (useSyncContext)
                MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
            else
                ReadingChanged?.Invoke(null, e);
        }
    }

    public class GyroscopeChangedEventArgs : EventArgs
    {
        internal GyroscopeChangedEventArgs(GyroscopeData reading) =>
            Reading = reading;

        public GyroscopeData Reading { get; }
    }

    public readonly struct GyroscopeData : IEquatable<GyroscopeData>
    {
        internal GyroscopeData(double x, double y, double z)
            : this((float)x, (float)y, (float)z)
        {
        }

        internal GyroscopeData(float x, float y, float z) =>
            AngularVelocity = new Vector3(x, y, z);

        public Vector3 AngularVelocity { get; }

        public override bool Equals(object obj) =>
            (obj is GyroscopeData data) && Equals(data);

        public bool Equals(GyroscopeData other) =>
            AngularVelocity.Equals(other.AngularVelocity);

        public static bool operator ==(GyroscopeData left, GyroscopeData right) =>
          Equals(left, right);

        public static bool operator !=(GyroscopeData left, GyroscopeData right) =>
           !Equals(left, right);

        public override int GetHashCode() =>
            AngularVelocity.GetHashCode();

        public override string ToString() =>
            $"X: {AngularVelocity.X}, Y: {AngularVelocity.Y}, Z: {AngularVelocity.Z}";
    }
}
