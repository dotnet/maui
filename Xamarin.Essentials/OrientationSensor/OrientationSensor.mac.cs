namespace Xamarin.Essentials
{
    public static partial class OrientationSensor
    {
        internal static bool IsSupported =>
            throw new System.PlatformNotSupportedException();

        static void PlatformStart(SensorSpeed sensorSpeed) =>
            throw new System.PlatformNotSupportedException();

        static void PlatformStop() =>
            throw new System.PlatformNotSupportedException();
    }
}
