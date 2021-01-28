using Tizen.Sensor;
using TizenRotationVectorSensor = Tizen.Sensor.RotationVectorSensor;

namespace Xamarin.Essentials
{
    public static partial class OrientationSensor
    {
        static TizenRotationVectorSensor DefaultSensor
            => (TizenRotationVectorSensor)Platform.GetDefaultSensor(SensorType.OrientationSensor);

        internal static bool IsSupported
            => TizenRotationVectorSensor.IsSupported;

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

        static void DataUpdated(object sender, RotationVectorSensorDataUpdatedEventArgs e)
        {
            OnChanged(new OrientationSensorData(e.X, e.Y, e.Z, e.W));
        }
    }
}
