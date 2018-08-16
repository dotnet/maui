using System;

namespace Xamarin.Essentials
{
    public static partial class Barometer
    {
        static bool useSyncContext;

        public static event EventHandler<BarometerChangedEventArgs> ReadingChanged;

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

        internal static void OnChanged(BarometerData reading) =>
            OnChanged(new BarometerChangedEventArgs(reading));

        static void OnChanged(BarometerChangedEventArgs e)
        {
            if (useSyncContext)
                MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
            else
                ReadingChanged?.Invoke(null, e);
        }
    }

    public class BarometerChangedEventArgs : EventArgs
    {
        internal BarometerChangedEventArgs(BarometerData reading) =>
            Reading = reading;

        public BarometerData Reading { get; }
    }

    public readonly struct BarometerData : IEquatable<BarometerData>
    {
        internal BarometerData(double pressure) =>
            Pressure = pressure;

        public double Pressure { get; }

        public static bool operator ==(BarometerData left, BarometerData right) =>
            Equals(left, right);

        public static bool operator !=(BarometerData left, BarometerData right) =>
            !Equals(left, right);

        public override bool Equals(object obj) =>
            (obj is BarometerData data) && Equals(data);

        public bool Equals(BarometerData other) =>
            Pressure.Equals(other.Pressure);

        public override int GetHashCode() =>
            Pressure.GetHashCode();
    }
}
