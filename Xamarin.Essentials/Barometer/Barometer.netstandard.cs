namespace Xamarin.Essentials
{
    public static partial class Barometer
    {
        internal static bool PlatformIsSupported =>
            throw new NotImplementedInReferenceAssemblyException();

        internal static void PlatformStart(SensorSpeed sensorSpeed) =>
            throw new NotImplementedInReferenceAssemblyException();

        internal static void PlatformStop() =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
