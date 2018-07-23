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

        static string GetPlatform() => Platforms.iOS;

        static string GetIdiom()
        {
            switch (UIDevice.CurrentDevice.UserInterfaceIdiom)
            {
                case UIUserInterfaceIdiom.Pad:
                    return Idioms.Tablet;
                case UIUserInterfaceIdiom.Phone:
                    return Idioms.Phone;
                case UIUserInterfaceIdiom.TV:
                    return Idioms.TV;
                case UIUserInterfaceIdiom.CarPlay:
                case UIUserInterfaceIdiom.Unspecified:
                default:
                    return Idioms.Unsupported;
            }
        }

        static DeviceType GetDeviceType()
            => Runtime.Arch == Arch.DEVICE ? DeviceType.Physical : DeviceType.Virtual;
    }
}
