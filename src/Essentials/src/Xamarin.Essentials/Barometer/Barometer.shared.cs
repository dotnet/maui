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
                throw new InvalidOperationException("Barometer has already been started.");

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
        public BarometerChangedEventArgs(BarometerData reading) =>
            Reading = reading;

        public BarometerData Reading { get; }
    }

    public readonly struct BarometerData : IEquatable<BarometerData>
    {
        public BarometerData(double pressure) =>
            PressureInHectopascals = pressure;

        public double PressureInHectopascals { get; }

        public static bool operator ==(BarometerData left, BarometerData right) =>
            left.Equals(right);

        public static bool operator !=(BarometerData left, BarometerData right) =>
            !left.Equals(right);

        public override bool Equals(object obj) =>
            (obj is BarometerData data) && Equals(data);

        public bool Equals(BarometerData other) =>
            PressureInHectopascals.Equals(other.PressureInHectopascals);

        public override int GetHashCode() =>
            PressureInHectopascals.GetHashCode();

        public override string ToString() => $"{nameof(PressureInHectopascals)}: {PressureInHectopascals}";
    }
}
