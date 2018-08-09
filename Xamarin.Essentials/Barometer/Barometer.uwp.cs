using System;
using Windows.Devices.Sensors;
using WinBarometer = Windows.Devices.Sensors.Barometer;

namespace Xamarin.Essentials
{
    public static partial class Barometer
    {
        static WinBarometer sensor;

        static WinBarometer DefaultBarometer => WinBarometer.GetDefault();

        static bool PlatformIsSupported =>
            DefaultBarometer != null;

        static void PlatformStart(SensorSpeed sensorSpeed)
        {
            sensor = DefaultBarometer;
            var nativeSpeed = sensorSpeed.ToPlatform();
            sensor.ReportInterval = sensor.MinimumReportInterval < nativeSpeed
                ? sensor.MinimumReportInterval : nativeSpeed;
            sensor.ReadingChanged += BarometerReportedInterval;
        }

        static void BarometerReportedInterval(object sender, BarometerReadingChangedEventArgs e)
            => OnChanged(new BarometerData(e.Reading.StationPressureInHectopascals));

        static void PlatformStop()
        {
            sensor.ReadingChanged -= BarometerReportedInterval;
            sensor.ReportInterval = 0;
            sensor = null;
        }
    }
}
