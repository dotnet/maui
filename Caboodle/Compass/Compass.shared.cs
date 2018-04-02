using System;
using System.Threading;

namespace Microsoft.Caboodle
{
    public static partial class Compass
    {
        public static event CompassChangedEventHandler ReadingChanged;

        public static bool IsMonitoring { get; private set; }

        internal static bool UseSyncContext { get; set; }

        public static void Start(SensorSpeed sensorSpeed)
        {
            if (!IsSupported)
            {
                throw new FeatureNotSupportedException();
            }

            if (IsMonitoring)
            {
                return;
            }

            PlatformStart(sensorSpeed);
            IsMonitoring = true;
        }

        public static void Stop()
        {
            PlatformStop();
            IsMonitoring = false;
        }

        internal static void OnChanged(CompassData reading)
            => OnChanged(new CompassChangedEventArgs(reading));

        internal static void OnChanged(CompassChangedEventArgs e)
        {
            if (ReadingChanged == null)
                return;

            if (UseSyncContext)
            {
                Platform.BeginInvokeOnMainThread(() => ReadingChanged?.Invoke(e));
            }
            else
            {
                ReadingChanged?.Invoke(e);
            }
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
