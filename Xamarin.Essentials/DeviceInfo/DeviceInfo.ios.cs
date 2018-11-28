using System;
using System.Diagnostics;
using ObjCRuntime;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class DeviceInfo
    {
        static string GetModel()
        {
            try
            {
                return Essentials.Platform.GetSystemLibraryProperty("hw.machine");
            }
            catch (Exception)
            {
                Debug.WriteLine("Unable to query hardware model, returning current device model.");
            }
            return UIDevice.CurrentDevice.Model;
        }

        static string GetManufacturer() => "Apple";

        static string GetDeviceName() => UIDevice.CurrentDevice.Name;

        static string GetVersionString() => UIDevice.CurrentDevice.SystemVersion;

        static DevicePlatform GetPlatform() => DevicePlatform.iOS;

        static DeviceIdiom GetIdiom()
        {
            switch (UIDevice.CurrentDevice.UserInterfaceIdiom)
            {
                case UIUserInterfaceIdiom.Pad:
                    return DeviceIdiom.Tablet;
                case UIUserInterfaceIdiom.Phone:
                    return DeviceIdiom.Phone;
                case UIUserInterfaceIdiom.TV:
                    return DeviceIdiom.TV;
                case UIUserInterfaceIdiom.CarPlay:
                case UIUserInterfaceIdiom.Unspecified:
                default:
                    return DeviceIdiom.Unknown;
            }
        }

        static DeviceType GetDeviceType()
            => Runtime.Arch == Arch.DEVICE ? DeviceType.Physical : DeviceType.Virtual;
    }
}
