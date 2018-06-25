using System;
using System.Numerics;

namespace Xamarin.Essentials
{
    public static partial class Magnetometer
    {
        static bool useSyncContext;

        public static event MagnetometerChangedEventHandler ReadingChanged;

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

        internal static void OnChanged(MagnetometerData reading)
            => OnChanged(new MagnetometerChangedEventArgs(reading));

        internal static void OnChanged(MagnetometerChangedEventArgs e)
        {
            var handler = ReadingChanged;
            if (handler == null)
                return;

            if (useSyncContext)
                MainThread.BeginInvokeOnMainThread(() => handler?.Invoke(e));
            else
                handler?.Invoke(e);
        }
    }

    public delegate void MagnetometerChangedEventHandler(MagnetometerChangedEventArgs e);

    public class MagnetometerChangedEventArgs : EventArgs
    {
        internal MagnetometerChangedEventArgs(MagnetometerData reading) => Reading = reading;

        public MagnetometerData Reading { get; }
    }

    public struct MagnetometerData
    {
        internal MagnetometerData(double x, double y, double z)
            : this((float)x, (float)y, (float)z)
        {
        }

        internal MagnetometerData(float x, float y, float z) =>
            MagneticField = new Vector3(x, y, z);

        public Vector3 MagneticField { get; }
    }
}
