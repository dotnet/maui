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

        [DllImport(Constants.CoreFoundationLibrary)]
        static extern void CFRelease(IntPtr cf);

        static string GetModel() =>
            IOKit.GetPlatformExpertPropertyValue<NSData>("model")?.ToString() ?? string.Empty;

        static string GetManufacturer() => "Apple";

        static string GetDeviceName()
        {
            var computerNameHandle = SCDynamicStoreCopyComputerName(IntPtr.Zero, IntPtr.Zero);

            if (computerNameHandle == IntPtr.Zero)
                return null;

            try
            {
                return NSString.FromHandle(computerNameHandle);
            }
            finally
            {
                CFRelease(computerNameHandle);
            }
        }

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
