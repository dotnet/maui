namespace Microsoft.Maui.Essentials
{
    static class SensorSpeedExtensions
    {
        internal static uint ToPlatform(this SensorSpeed sensorSpeed)
        {
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    return 20;
                case SensorSpeed.Game:
                    return 40;
                case SensorSpeed.UI:
                    return 80;
            }

            return 225;
        }
    }
}
