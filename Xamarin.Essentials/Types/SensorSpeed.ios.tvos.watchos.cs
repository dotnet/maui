namespace Xamarin.Essentials
{
    static class SensorSpeedExtensions
    {
        internal static double ToPlatform(this SensorSpeed sensorSpeed)
        {
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    return .02;
                case SensorSpeed.Game:
                    return .04;
                case SensorSpeed.UI:
                    return .08;
            }

            return .225;
        }
    }
}
