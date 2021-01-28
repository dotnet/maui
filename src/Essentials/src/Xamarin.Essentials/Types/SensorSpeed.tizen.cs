namespace Xamarin.Essentials
{
    static class SensorSpeedExtensions
    {
        internal static uint ToPlatform(this SensorSpeed sensorSpeed)
        {
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    return 10;
                case SensorSpeed.Game:
                    return 20;
                case SensorSpeed.UI:
                    return 60;
            }

            return 100;
        }
    }
}
