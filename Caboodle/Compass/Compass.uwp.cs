using System;
using System.Threading;
using Windows.Devices.Sensors;

using WindowsCompass = Windows.Devices.Sensors.Compass;

namespace Microsoft.Caboodle
{
    public static partial class Compass
    {
        // Magic numbers from https://docs.microsoft.com/en-us/uwp/api/windows.devices.sensors.compass.reportinterval#Windows_Devices_Sensors_Compass_ReportInterval
        internal const uint FastestInterval = 8;
        internal const uint GameInterval = 22;
        internal const uint NormalInterval = 33;

        internal static WindowsCompass DefaultCompass =>
            WindowsCompass.GetDefault();

        internal static bool IsSupported =>
            DefaultCompass != null;

        internal static void PlatformStart(SensorSpeed sensorSpeed, Action<CompassData> handler)
        {
            var compass = DefaultCompass;
            var useSyncContext = false;
            var interval = NormalInterval;
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    interval = FastestInterval;
                    break;
                case SensorSpeed.Game:
                    interval = GameInterval;
                    break;
                default:
                    useSyncContext = true;
                    break;
            }

            compass.ReportInterval = compass.MinimumReportInterval >= interval ? interval : compass.MinimumReportInterval;

            MonitorCTS.Token.Register(CancelledToken);

            void CancelledToken()
            {
                Platform.BeginInvokeOnMainThread(() =>
                {
                    compass.ReadingChanged -= CompassReportedInterval;
                    DisposeToken();
                });
            }

            compass.ReadingChanged += CompassReportedInterval;

            void CompassReportedInterval(object sender, CompassReadingChangedEventArgs e)
            {
                var data = new CompassData(e.Reading.HeadingMagneticNorth);
                if (useSyncContext)
                {
                    Platform.BeginInvokeOnMainThread(() => handler?.Invoke(data));
                }
                else
                {
                    handler?.Invoke(data);
                }
            }
        }
    }
}
