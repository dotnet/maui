using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace Xamarin.Essentials
{
    public static partial class DeviceInfo
    {
        [DllImport(Constants.SystemConfigurationLibrary)]
        static extern IntPtr SCDynamicStoreCopyComputerName(IntPtr store, IntPtr encoding);

        static string GetModel() =>
            IOKit.GetPlatformExpertPropertyValue<NSData>("model")?.ToString() ?? string.Empty;

        static string GetManufacturer() => "Apple";

        static string GetDeviceName() =>
            NSString.FromHandle(SCDynamicStoreCopyComputerName(IntPtr.Zero, IntPtr.Zero));

        static string GetVersionString()
        {
            using var info = new NSProcessInfo();
            return info.OperatingSystemVersion.ToString();
        }

        static DevicePlatform GetPlatform() => DevicePlatform.macOS;

        static DeviceIdiom GetIdiom() => DeviceIdiom.Desktop;

        static DeviceType GetDeviceType() => DeviceType.Physical;
    }
}
