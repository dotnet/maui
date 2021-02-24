using System;
using Windows.Devices.Sensors;
using WindowsMagnetometer = Windows.Devices.Sensors.Magnetometer;

namespace Microsoft.Maui.Essentials
{
    public static partial class Magnetometer
    {
        // keep around a reference so we can stop this same instance
        static WindowsMagnetometer sensor;

        internal static WindowsMagnetometer DefaultSensor =>
            WindowsMagnetometer.GetDefault();

        internal static bool IsSupported =>
            DefaultSensor != null;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            sensor = DefaultSensor;

            var interval = sensorSpeed.ToPlatform();
            sensor.ReportInterval = sensor.MinimumReportInterval >= interval ? sensor.MinimumReportInterval : interval;

            sensor.ReadingChanged += DataUpdated;
        }

        static void DataUpdated(object sender, MagnetometerReadingChangedEventArgs e)
        {
            var reading = e.Reading;
            var data = new MagnetometerData(reading.MagneticFieldX, reading.MagneticFieldY, reading.MagneticFieldZ);
            OnChanged(data);
        }

        internal static void PlatformStop()
        {
            sensor.ReadingChanged -= DataUpdated;
            sensor.ReportInterval = 0;
        }
    }
}
