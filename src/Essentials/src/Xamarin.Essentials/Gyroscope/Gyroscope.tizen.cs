using Tizen.Sensor;
using TizenGyroscope = Tizen.Sensor.Gyroscope;

namespace Xamarin.Essentials
{
    public static partial class Gyroscope
    {
        internal static TizenGyroscope DefaultSensor =>
            (TizenGyroscope)Platform.GetDefaultSensor(SensorType.Gyroscope);

        internal static bool IsSupported =>
            TizenGyroscope.IsSupported;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            DefaultSensor.Interval = sensorSpeed.ToPlatform();
            DefaultSensor.DataUpdated += DataUpdated;
            DefaultSensor.Start();
        }

        internal static void PlatformStop()
        {
            DefaultSensor.DataUpdated -= DataUpdated;
            DefaultSensor.Stop();
        }

        static void DataUpdated(object sender, GyroscopeDataUpdatedEventArgs e)
        {
            OnChanged(new GyroscopeData(e.X, e.Y, e.Z));
        }
    }
}
