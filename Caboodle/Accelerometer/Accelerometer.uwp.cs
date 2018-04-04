using Windows.Devices.Sensors;
using WindowsAccelerometer = Windows.Devices.Sensors.Accelerometer;

namespace Microsoft.Caboodle
{
    public static partial class Accelerometer
    {
        // Magic numbers from https://docs.microsoft.com/en-us/uwp/api/windows.devices.sensors.compass.reportinterval#Windows_Devices_Sensors_Compass_ReportInterval
        internal const uint FastestInterval = 8;
        internal const uint GameInterval = 22;
        internal const uint NormalInterval = 33;

        static WindowsAccelerometer sensor;

        internal static WindowsAccelerometer DefaultSensor =>
            WindowsAccelerometer.GetDefault();

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

        static void DataUpdated(object sender, AccelerometerReadingChangedEventArgs e)
        {
            var reading = e.Reading;
            var data = new AccelerometerData(reading.AccelerationX, reading.AccelerationY, reading.AccelerationZ);
            OnChanged(data);
        }

        internal static void PlatformStop()
        {
            if (sensor == null)
                return;

            sensor.ReadingChanged -= DataUpdated;
        }
    }
}
