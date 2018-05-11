namespace Xamarin.Essentials
{
    public static partial class Gyroscope
    {
        internal static bool IsSupported =>
            throw new NotImplementedInReferenceAssemblyException();

        internal static void PlatformStart(SensorSpeed sensorSpeed) =>
            throw new NotImplementedInReferenceAssemblyException();

        internal static void PlatformStop() =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
