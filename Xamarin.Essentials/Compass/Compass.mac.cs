namespace Xamarin.Essentials
{
    public static partial class Compass
    {
        internal static bool IsSupported =>
            throw new System.PlatformNotSupportedException();

        internal static void PlatformStart(SensorSpeed sensorSpeed, bool applyLowPassFilter) =>
            throw new System.PlatformNotSupportedException();

        internal static void PlatformStop() =>
            throw new System.PlatformNotSupportedException();
    }
}
