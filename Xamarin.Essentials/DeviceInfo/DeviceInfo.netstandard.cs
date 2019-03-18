namespace Xamarin.Essentials
{
    public static partial class DeviceInfo
    {
        static string GetModel() => throw new NotImplementedInReferenceAssemblyException();

        static string GetManufacturer() => throw new NotImplementedInReferenceAssemblyException();

        static string GetDeviceName() => throw new NotImplementedInReferenceAssemblyException();

        static string GetVersionString() => throw new NotImplementedInReferenceAssemblyException();

        static DevicePlatform GetPlatform() => DevicePlatform.Unknown;

        static DeviceIdiom GetIdiom() => DeviceIdiom.Unknown;

        static DeviceType GetDeviceType() => DeviceType.Unknown;
    }
}
