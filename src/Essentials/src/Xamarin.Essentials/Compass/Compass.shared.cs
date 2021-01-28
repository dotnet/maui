using System;

namespace Xamarin.Essentials
{
    public static partial class Compass
    {
        static bool useSyncContext;

        public static event EventHandler<CompassChangedEventArgs> ReadingChanged;

        public static bool IsMonitoring { get; private set; }

        public static void Start(SensorSpeed sensorSpeed) => Start(sensorSpeed, true);

        public static void Start(SensorSpeed sensorSpeed, bool applyLowPassFilter)
        {
            if (!IsSupported)
                throw new FeatureNotSupportedException();

            if (IsMonitoring)
                throw new InvalidOperationException("Compass has already been started.");

            IsMonitoring = true;
            useSyncContext = sensorSpeed == SensorSpeed.Default || sensorSpeed == SensorSpeed.UI;

            try
            {
                PlatformStart(sensorSpeed, applyLowPassFilter);
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

        internal static void OnChanged(CompassData reading) =>
            OnChanged(new CompassChangedEventArgs(reading));

        internal static void OnChanged(CompassChangedEventArgs e)
        {
            if (useSyncContext)
                MainThread.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(null, e));
            else
                ReadingChanged?.Invoke(null, e);
        }
    }

    public class CompassChangedEventArgs : EventArgs
    {
        public CompassChangedEventArgs(CompassData reading) =>
            Reading = reading;

        public CompassData Reading { get; }
    }

    public readonly struct CompassData : IEquatable<CompassData>
    {
        public CompassData(double headingMagneticNorth) =>
            HeadingMagneticNorth = headingMagneticNorth;

        public double HeadingMagneticNorth { get; }

        public override bool Equals(object obj) =>
            (obj is CompassData data) && Equals(data);

        public bool Equals(CompassData other) =>
            HeadingMagneticNorth.Equals(other.HeadingMagneticNorth);

        public static bool operator ==(CompassData left, CompassData right) =>
            left.Equals(right);

        public static bool operator !=(CompassData left, CompassData right) =>
           !left.Equals(right);

        public override int GetHashCode() =>
            HeadingMagneticNorth.GetHashCode();

        public override string ToString() =>
            $"{nameof(HeadingMagneticNorth)}: {HeadingMagneticNorth}";
    }
}
