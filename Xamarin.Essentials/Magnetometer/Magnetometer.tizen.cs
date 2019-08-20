using Tizen.Sensor;
using TizenMagnetometer = Tizen.Sensor.Magnetometer;

namespace Xamarin.Essentials
{
    public static partial class Magnetometer
    {
        internal static TizenMagnetometer DefaultSensor =>
            (TizenMagnetometer)Platform.GetDefaultSensor(SensorType.Magnetometer);

        internal static bool IsSupported =>
            TizenMagnetometer.IsSupported;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            DefaultSensor.Interval = sensorSpeed.ToPlatform();
            DefaultSensor.DataUpdated += DataUpdated;
            DefaultSensor.Start();
        }

        internal static void PlatformStop()
        {
            DefaultSensor.DataUpdated -= DataUpdated;
            DefaultSensor.Stop();
        }

        static void DataUpdated(object sender, MagnetometerDataUpdatedEventArgs e)
        {
            OnChanged(new MagnetometerData(e.X, e.Y, e.Z));
        }
    }
}
