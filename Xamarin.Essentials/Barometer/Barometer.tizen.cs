using Tizen.Sensor;
using TizenBarometerSensor = Tizen.Sensor.PressureSensor;

namespace Xamarin.Essentials
{
    public static partial class Barometer
    {
        static TizenBarometerSensor DefaultSensor
            => (TizenBarometerSensor)Platform.GetDefaultSensor(SensorType.Barometer);

        internal static bool IsSupported
            => TizenBarometerSensor.IsSupported;

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

        static void DataUpdated(object sender, PressureSensorDataUpdatedEventArgs e)
        {
            OnChanged(new BarometerData(e.Pressure));
        }
    }
}
