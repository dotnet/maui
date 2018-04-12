using System;
using System.Threading;

namespace Xamarin.Essentials
{
    public static partial class Compass
    {
        static bool useSyncContext;

        public static event CompassChangedEventHandler ReadingChanged;

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

        internal static void OnChanged(CompassData reading)
            => OnChanged(new CompassChangedEventArgs(reading));

        internal static void OnChanged(CompassChangedEventArgs e)
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

    public delegate void CompassChangedEventHandler(CompassChangedEventArgs e);

    public class CompassChangedEventArgs : EventArgs
    {
        internal CompassChangedEventArgs(CompassData reading) => Reading = reading;

        public CompassData Reading { get; }
    }

    public struct CompassData
    {
        internal CompassData(double headingMagneticNorth)
        {
            HeadingMagneticNorth = headingMagneticNorth;
        }

        public double HeadingMagneticNorth { get; }
    }
}
