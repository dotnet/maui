using Tizen.Sensor;
using TizenAccelerometer = Tizen.Sensor.Accelerometer;

namespace Xamarin.Essentials
{
    public static partial class Accelerometer
    {
        internal static TizenAccelerometer DefaultSensor =>
            (TizenAccelerometer)Platform.GetDefaultSensor(SensorType.Accelerometer);

        internal static bool IsSupported =>
            TizenAccelerometer.IsSupported;

        static void PlatformStart(SensorSpeed sensorSpeed)
        {
            DefaultSensor.Interval = sensorSpeed.ToPlatform();
            DefaultSensor.DataUpdated += DataUpdated;
            DefaultSensor.Start();
        }

        static void PlatformStop()
        {
            DefaultSensor.DataUpdated -= DataUpdated;
            DefaultSensor.Stop();
        }

        static void DataUpdated(object sender, AccelerometerDataUpdatedEventArgs e)
        {
            OnChanged(new AccelerometerData(e.X, e.Y, e.Z));
        }
    }
}
