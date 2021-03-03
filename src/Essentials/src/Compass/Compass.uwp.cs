using Windows.Devices.Sensors;

using WindowsCompass = Windows.Devices.Sensors.Compass;

namespace Microsoft.Maui.Essentials
{
    public static partial class Compass
    {
        // Magic numbers from https://docs.microsoft.com/en-us/uwp/api/windows.devices.sensors.compass.reportinterval#Windows_Devices_Sensors_Compass_ReportInterval
        internal const uint FastestInterval = 8;
        internal const uint GameInterval = 22;
        internal const uint NormalInterval = 33;

        // keep around a reference so we can stop this same instance
        static WindowsCompass sensor;

        internal static WindowsCompass DefaultCompass =>
            WindowsCompass.GetDefault();

        internal static bool IsSupported =>
            DefaultCompass != null;

        internal static void PlatformStart(SensorSpeed sensorSpeed, bool applyLowPassFilter)
        {
            sensor = DefaultCompass;

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

            sensor.ReadingChanged += CompassReportedInterval;
        }

        static void CompassReportedInterval(object sender, CompassReadingChangedEventArgs e)
        {
            var data = new CompassData(e.Reading.HeadingMagneticNorth);
            OnChanged(data);
        }

        internal static void PlatformStop()
        {
            sensor.ReadingChanged -= CompassReportedInterval;
            sensor.ReportInterval = 0;
        }
    }
}
