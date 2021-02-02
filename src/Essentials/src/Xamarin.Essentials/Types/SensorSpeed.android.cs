using Android.Hardware;

namespace Xamarin.Essentials
{
    static class SensorSpeedExtensions
    {
        internal static SensorDelay ToPlatform(this SensorSpeed sensorSpeed)
        {
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    return SensorDelay.Fastest;
                case SensorSpeed.Game:
                    return SensorDelay.Game;
                case SensorSpeed.UI:
                    return SensorDelay.Ui;
            }

            return SensorDelay.Normal;
        }
    }
}
