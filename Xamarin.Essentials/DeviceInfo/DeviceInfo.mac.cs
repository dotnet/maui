namespace Xamarin.Essentials
{
    public static partial class DeviceInfo
    {
        static string GetModel() => throw new System.PlatformNotSupportedException();

        static string GetManufacturer() => throw new System.PlatformNotSupportedException();

        static string GetDeviceName() => throw new System.PlatformNotSupportedException();

        static string GetVersionString() => throw new System.PlatformNotSupportedException();

        static DevicePlatform GetPlatform() => DevicePlatform.Unknown;

        static DeviceIdiom GetIdiom() => DeviceIdiom.Unknown;

        static DeviceType GetDeviceType() => DeviceType.Unknown;
    }
}
