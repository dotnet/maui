namespace Xamarin.Essentials
{
    public static partial class Accelerometer
    {
        internal static bool IsSupported =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static void PlatformStart(SensorSpeed sensorSpeed) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static void PlatformStop() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
