namespace Xamarin.Essentials
{
    public static partial class OrientationSensor
    {
        internal static bool IsSupported =>
            throw new NotImplementedInReferenceAssemblyException();

        static void PlatformStart(SensorSpeed sensorSpeed) =>
            throw new NotImplementedInReferenceAssemblyException();

        static void PlatformStop() =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
