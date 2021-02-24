using Windows.Devices.Sensors;
using WindowsOrientationSensor = Windows.Devices.Sensors.OrientationSensor;

namespace Microsoft.Maui.Essentials
{
    public static partial class OrientationSensor
    {
        // keep around a reference so we can stop this same instance
        static WindowsOrientationSensor sensor;

        internal static WindowsOrientationSensor DefaultSensor =>
          WindowsOrientationSensor.GetDefault();

        internal static bool IsSupported =>
            DefaultSensor != null;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            sensor = DefaultSensor;

            var interval = sensorSpeed.ToPlatform();

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
            sensor.ReportInterval = 0;
        }
    }
}
