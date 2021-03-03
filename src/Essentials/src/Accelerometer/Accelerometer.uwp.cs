using Windows.Devices.Sensors;
using WindowsAccelerometer = Windows.Devices.Sensors.Accelerometer;

namespace Microsoft.Maui.Essentials
{
    public static partial class Accelerometer
    {
        // keep around a reference so we can stop this same instance
        static WindowsAccelerometer sensor;

        internal static WindowsAccelerometer DefaultSensor =>
            WindowsAccelerometer.GetDefault();

        internal static bool IsSupported =>
            DefaultSensor != null;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            sensor = DefaultSensor;

            var interval = sensorSpeed.ToPlatform();
            sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

            sensor.ReadingChanged += DataUpdated;
        }

        static void DataUpdated(object sender, AccelerometerReadingChangedEventArgs e)
        {
            var reading = e.Reading;
            var data = new AccelerometerData(reading.AccelerationX * -1, reading.AccelerationY * -1, reading.AccelerationZ * -1);
            OnChanged(data);
        }

        internal static void PlatformStop()
        {
            sensor.ReadingChanged -= DataUpdated;
            sensor.ReportInterval = 0;
        }
    }
}
