using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace Xamarin.Essentials
{
    public static partial class DeviceInfo
    {
        [DllImport("/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration")]
        static extern IntPtr SCDynamicStoreCopyComputerName(IntPtr store, IntPtr encoding);

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern uint IOServiceGetMatchingService(uint masterPort, IntPtr matching);

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern IntPtr IOServiceMatching(string s);

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern IntPtr IORegistryEntryCreateCFProperty(uint entry, IntPtr key, IntPtr allocator, uint options);

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern int IOObjectRelease(uint o);

        [DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        static extern void CFRelease(IntPtr obj);

        static readonly NSString modelKey = (NSString)"model";

        static string GetModel()
        {
            uint platformExpert = 0;
            IntPtr model = IntPtr.Zero;
            try
            {
                platformExpert = IOServiceGetMatchingService(0, IOServiceMatching("IOPlatformExpertDevice"));
                if (platformExpert != 0)
                {
                    model = IORegistryEntryCreateCFProperty(platformExpert, modelKey.Handle, IntPtr.Zero, 0);
                    if (model != IntPtr.Zero)
                    {
                        using (var data = Runtime.GetNSObject<NSData>(model))
                        {
                            return data.ToString();
                        }
                    }
                }
            }
            finally
            {
                if (model != IntPtr.Zero)
                    CFRelease(model);
                if (platformExpert != 0)
                    IOObjectRelease(platformExpert);
            }

            return string.Empty;
        }

        static string GetManufacturer() => "Apple";

        static string GetDeviceName() => NSString.FromHandle(SCDynamicStoreCopyComputerName(IntPtr.Zero, IntPtr.Zero));

        static string GetVersionString()
        {
            using (var info = new NSProcessInfo())
            {
                return info.OperatingSystemVersion.ToString();
            }
        }

        static DevicePlatform GetPlatform() => DevicePlatform.macOS;

        static DeviceIdiom GetIdiom() => DeviceIdiom.Desktop;

        static DeviceType GetDeviceType() => DeviceType.Physical;
    }
}
