using Windows.Devices.Sensors;
using WindowsOrientationSensor = Windows.Devices.Sensors.OrientationSensor;

namespace Xamarin.Essentials
{
    public static partial class OrientationSensor
    {
        // Magic numbers from https://docs.microsoft.com/en-us/uwp/api/windows.devices.sensors.compass.reportinterval#Windows_Devices_Sensors_Compass_ReportInterval
        internal const uint FastestInterval = 8;
        internal const uint GameInterval = 22;
        internal const uint NormalInterval = 33;

        // keep around a reference so we can stop this same instance
        static WindowsOrientationSensor sensor;

        internal static WindowsOrientationSensor DefaultSensor =>
          WindowsOrientationSensor.GetDefault();

        internal static bool IsSupported =>
            DefaultSensor != null;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            sensor = DefaultSensor;

            var interval = NormalInterval;
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    interval = FastestInterval;
                    break;
                case SensorSpeed.Game:
                    interval = GameInterval;
                    break;
            }

            sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

            sensor.ReadingChanged += DataUpdated;
        }

        static void DataUpdated(object sender, OrientationSensorReadingChangedEventArgs e)
        {
            var reading = e.Reading;
            var data = new OrientationSensorData(reading.Quaternion.X, reading.Quaternion.Y, reading.Quaternion.Z, reading.Quaternion.W);
            OnChanged(data);
        }

        internal static void PlatformStop()
        {
            sensor.ReadingChanged -= DataUpdated;
        }
    }
}
