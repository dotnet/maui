using System;

namespace Xamarin.Essentials
{
    public static partial class Barometer
    {
        public static event EventHandler<BarometerChangedEventArgs> ReadingChanged;

        public static bool IsMonitoring { get; private set; }

        public static bool IsSupported => PlatformIsSupported;

        public static void Start(SensorSpeed sensorSpeed)
        {
            if (!IsSupported)
               throw new FeatureNotSupportedException();

            if (IsMonitoring)
                return;

            IsMonitoring = true;
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

        internal static void OnChanged(BarometerData reading)
                => OnChanged(new BarometerChangedEventArgs(reading));

        static void OnChanged(BarometerChangedEventArgs e)
            => ReadingChanged?.Invoke(null, e);
    }

    public struct BarometerData
    {
        internal BarometerData(double hPAPressure) =>
            Pressure = hPAPressure;

        public double Pressure { get; }
    }

    public class BarometerChangedEventArgs : EventArgs
    {
        internal BarometerChangedEventArgs(BarometerData pressureData)
            => BarometerData = pressureData;

        public BarometerData BarometerData { get; } // In Hectopascals (hPA)
    }
}
