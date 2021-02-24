using Windows.Devices.Sensors;
using WindowsGyro = Windows.Devices.Sensors.Gyrometer;

namespace Microsoft.Maui.Essentials
{
    public static partial class Gyroscope
    {
        // keep around a reference so we can stop this same instance
        static WindowsGyro sensor;

        internal static WindowsGyro DefaultSensor =>
            WindowsGyro.GetDefault();

        internal static bool IsSupported =>
            DefaultSensor != null;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            sensor = DefaultSensor;

            var interval = sensorSpeed.ToPlatform();
            sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

            sensor.ReadingChanged += DataUpdated;
        }

        static void DataUpdated(object sender, GyrometerReadingChangedEventArgs e)
        {
            var reading = e.Reading;
            var data = new GyroscopeData(reading.AngularVelocityX, reading.AngularVelocityY, reading.AngularVelocityZ);
            OnChanged(data);
        }

        internal static void PlatformStop()
        {
            sensor.ReadingChanged -= DataUpdated;
            sensor.ReportInterval = 0;
        }
    }
}
